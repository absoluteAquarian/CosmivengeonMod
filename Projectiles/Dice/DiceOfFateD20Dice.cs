using CosmivengeonMod.API.Commands;
using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Players;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Dice{
	public class DiceOfFateD20Dice : ModProjectile{
		public override string Texture => "CosmivengeonMod/Items/Tools/Dice/DiceOfFateD20";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("D20");
		}

		public override void SetDefaults(){
			projectile.tileCollide = true;
			projectile.width = 34;
			projectile.height = 34;
			projectile.scale = 8f / projectile.width;
		}

		internal bool chooseNumber;
		private SpriteEffects effects;

		internal int random = -1;

		private float realAlpha = 0;

		private int GetMaxLuck(int unluckyFactor){
			if(unluckyFactor == 0)
				return 20;
			else if(unluckyFactor < 3)
				return 15;
			else if(unluckyFactor < 5)
				return 10;
			else
				return 8;
		}

		public override void AI(){
			float oldYVel = projectile.velocity.Y;
			projectile.velocity.Y += 21f / 60f;

			projectile.velocity.X *= 1f - 1.5f / 60f;

			if(Math.Abs(projectile.velocity.X) < 0.08f)
				projectile.velocity.X = 0;

			//Keep it alive as long as it hasn't stopped yet
			if(!chooseNumber){
				projectile.timeLeft = 60;
				projectile.netUpdate = true;
			}

			//Only the server should update the projectile
			if(Main.myPlayer != projectile.owner || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if(projectile.velocity.LengthSquared() > 10 * 10)
				projectile.velocity = Vector2.Normalize(projectile.velocity) * 10f;

			if(!chooseNumber){
				if(Main.rand.NextFloat() * 5f * (projectile.velocity.Length() / 10f) >= 0.5f){
					effects = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically;
					projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
				}
			}else{
				realAlpha += 255f / 90f;
				projectile.alpha = (int)realAlpha;
			}

			if(projectile.oldVelocity.X == 0 && projectile.velocity.X == 0f && oldYVel == 0 && !chooseNumber){
				chooseNumber = true;
				projectile.timeLeft = 90;

				//Roll was set by a command if "random" isn't -1
				if(random == -1){
					//Events for a 1 and 20 are very extreme.  Make sure they only happen after two random calls
					random = Main.rand.Next(1, 21);
					if((random == 1 || random == 20) && Main.rand.NextFloat() > 0.2f)
						random = Main.rand.Next(1, 21);

					//If the unlucky factor is > 0, then make bad rolls happen more often that good rolls
					int unlucky = (int)projectile.ai[0];
					int maxLuck = GetMaxLuck(unlucky);
					if(random > maxLuck)
						random = Main.rand.Next(1, maxLuck + 1);
				}

				Color color = random < 5
					? CombatText.LifeRegenNegative
					: (random < 10
						? CombatText.DamagedHostileCrit
						: (random < 15
							? CombatText.DamagedHostile
							: CombatText.LifeRegen));

				CombatText.NewText(new Rectangle((int)projectile.Center.X - 8, (int)projectile.Center.Y - 40, 16, 16), color, random, dramatic: true);
			}
		}

		public override bool? CanCutTiles() => true;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough){
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			if(projectile.velocity.Y != oldVelocity.Y){
				projectile.velocity.Y = (int)(-oldVelocity.Y * (oldVelocity.Y > 0 ? 0.78f : 1f));

				//Stop bouncing if the bounce up was too slow
				if(-0.3f < projectile.velocity.Y && projectile.velocity.Y < 0)
					projectile.velocity.Y = 0;
			}
			if(projectile.velocity.X != oldVelocity.X)
				projectile.velocity.X = (int)(-oldVelocity.X * 0.78f);

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = ModContent.GetTexture(Texture);
			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, texture.Size() / 2f, projectile.scale, effects, 0f);
			return false;
		}

		public override void Kill(int timeLeft){
			Player owner = Main.player[projectile.owner];

			//Stuff should not run on clients other than the one for the owner (or the server)
			if(Main.myPlayer != owner.whoAmI || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			void InformEveryone(string result){
				string toPrint = $"{owner.name} rolled a {((random == 1 || random == 20) ? $"NAT {random}" : random.ToString())} and received: {result}";

				if(Main.netMode == NetmodeID.SinglePlayer)
					Main.NewText(toPrint);
				else if(Main.netMode == NetmodeID.Server)
					NetMessage.BroadcastChatMessage(NetworkText.FromLiteral(toPrint), Color.White);
			}

			//Effects go from really bad (1) to really good (20), with 10 and 11 being "neutral" effects
			//Only one effect is chosen per roll
			int rollEvent;
			bool validEvent;
			switch(random){
				#region Roll 1
				case 1:
					/*   Effects:
					 *   
					 *   - kill the player (softcore), halve current health (mediumcore) or quarter current health (hardcore)
					 *   - make the player drop all of the money in their inventory
					 *   - give the player two really bad debuffs
					 *   - teleport the player into the underground dungeon if Skeletron hasn't been defeated (softcore only)
					 *     - if Skeletron has been defeated or the player isn't in Softcore Mode, reroll this event
					 *   - teleport the player into a lava pit in hell
					 *     - if no lava is present, reroll this event
					 */
					int teleXDungeon = 0, teleYDungeon = 0, teleXHell = 0, teleYHell = 0;
					bool dungeonChecked = false, hellChecked = false;
					do{
						rollEvent = Main.rand.Next(0, 5);

						if(rollEvent == 3){
							if(owner.difficulty == 0){
								if(!dungeonChecked && !NPC.downedBoss3){
									dungeonChecked = true;
									validEvent = TryTeleportPlayerToDungeon(out teleXDungeon, out teleYDungeon);
								}else
									validEvent = false;
							}else
								validEvent = false;
						}else if(rollEvent == 4){
							if(!hellChecked){
								hellChecked = true;
								validEvent = TryTeleportPlayerToBrazil(out teleXHell, out teleYHell);
							}else
								validEvent = false;
						}else
							validEvent = true;
					}while(!validEvent);

					switch(rollEvent){
						case 0:
							//Ouch!
							InformEveryone(owner.difficulty == 0 ? "Death." : "Pain.");

							if(owner.difficulty == 0)
								owner.KillMe(PlayerDeathReason.ByCustomReason($"{owner.name} got unlucky!"), 9999, 0);
							else{
								int life = owner.statLife;
								int newLife = owner.statLife / (owner.difficulty == 1 ? 2 : 4);

								if(owner.statLife < 1)
									owner.statLife = 1;

								owner.Hurt(PlayerDeathReason.ByCustomReason($"{owner.name} got unlucky!"), life - newLife, 0, Crit: true);
							}
							break;
						case 1:
							InformEveryone("Holes in their pockets!");

							for(int i = 0; i < 54; i++){
								Item item = owner.inventory[i];
								if(item.type == ItemID.CopperCoin || item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin){
									Item.NewItem(owner.Center, item.type, item.stack);
									item.TurnToAir();
								}
							}
							break;
						case 2:
							InformEveryone("Two nasty debuffs!");

							for(int i = 0; i < 2; i++){
								(int, int) debuff = Main.rand.Next(new (int, int)[]{
									(BuffID.Suffocation, 120 * 60),
									(BuffID.CursedInferno, 180 * 60),
									(BuffID.Stoned, 60 * 60),
									(BuffID.Burning, 120 * 60),
									(BuffID.Cursed, 12 * 60),
									(BuffID.NoBuilding, 20 * 60),
									(ModContent.BuffType<PrimordialWrath>(), 45 * 60)
								});
								owner.AddBuff(debuff.Item1, debuff.Item2);
							}
							break;
						case 3:
							InformEveryone("A one-way trip to the bone zone!");

							owner.Teleport(new Vector2(teleXDungeon, teleYDungeon) * 16, Style: 1);
							break;
						case 4:
							InformEveryone("A free passport to Brazil!");

							owner.Teleport(new Vector2(teleXHell, teleYHell) * 16, Style: 1);
							break;
					}
					break;
				#endregion
				#region Roll 2
				case 2:
					/*   Effects
					 *   
					 *   - spawn a horde of Zombies (pre-hardmode), Possessed Armor (hardmode) or Herplings (post-Moon Lord) on the player
					 *   - fill the player's inventory with random fishing junk
					 *   - force it to be a Blood Mooon (pre-hardmode) or Solar Elipse (hardmode)
					 *   - completely drain the player's Stamina (Desolation Mode only) and Mana
					 */
					rollEvent = Main.rand.Next(0, 4);

					switch(rollEvent){
						case 0:
							InformEveryone("A horde of monsters!");

							for(int i = 0; i < 20; i++){
								int randX, randY;
								int tries = 20;
								do{
									randX = Main.rand.Next(-5 * 16, 5 * 16 + 1) + (int)owner.Bottom.X;
									randY = Main.rand.Next(-2 * 16, 1) + (int)owner.Bottom.Y;

									tries--;
									if(tries <= 0)
										break;
								}while(Framing.GetTileSafely(randX / 16, randY / 16).active());

								int hordeType;
								if(!Main.hardMode)
									hordeType = Main.rand.NextFloat() > 0.2f ? NPCID.Zombie : NPCID.UndeadMiner;
								else if(!NPC.downedPlantBoss)
									hordeType = NPCID.PossessedArmor;
								else if(!NPC.downedMoonlord)
									hordeType = WorldGen.crimson ? NPCID.Herpling : NPCID.Corruptor;
								else
									hordeType = Main.rand.Next(new int[]{
										NPCID.SolarCorite,
										NPCID.SolarCrawltipedeHead,
										NPCID.SolarDrakomireRider,
										NPCID.SolarSolenian,
										NPCID.SolarSpearman,
										NPCID.SolarSroller,

										NPCID.StardustCellBig,
										NPCID.StardustWormHead,
										NPCID.StardustSpiderBig,
										NPCID.StardustJellyfishBig,

										NPCID.NebulaBeast,
										NPCID.NebulaBrain,
										NPCID.NebulaHeadcrab,
										NPCID.NebulaSoldier,

										NPCID.VortexHornet,
										NPCID.VortexHornetQueen,
										NPCID.VortexRifleman,
										NPCID.VortexSoldier
									});

								NPC npc = Main.npc[NPC.NewNPC(randX, randY, hordeType)];
								npc.Bottom = new Vector2(randX, randY);
							}
							break;
						case 1:
							InformEveryone("An inventory full of junk!");

							for(int i = 0; i < 50; i++){
								Item item = owner.inventory[i];
								if(item.IsAir){
									item = owner.inventory[i] = new Item();
									item.SetDefaults(Main.rand.Next(new int[]{
										ItemID.OldShoe, ItemID.TinCan, ItemID.FishingSeaweed
									}));
									item.stack = 1;
								}
							}
							break;
						case 2:
							InformEveryone($"A terrible {(Main.hardMode ? "day" : "night")} to have a curse.");

							if(Main.hardMode){
								//Set the world to a solar eclipse
								Main.time = 0;
								Main.dayTime = true;

								Main.eclipse = true;
							}else{
								//Set the world to a blood moon
								Main.time = 0;
								Main.dayTime = false;

								Main.bloodMoon = true;
							}

							//Let everyone know that something wack happened
							if(Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendData(MessageID.WorldData);
							break;
						case 3:
							InformEveryone(WorldEvents.desoMode ? "Heavy fatigue and no mana!" : "No mana!");

							if(WorldEvents.desoMode){
								StaminaPlayer mp = owner.GetModPlayer<StaminaPlayer>();
								mp.stamina.ForceExhaustion();
							}

							owner.statMana = 0;
							owner.manaRegenDelay = 600;
							break;
					}
					break;
				#endregion
				#region Roll 3
				case 3:
					/*   Effects:
					 *   
					 *   - NPC shops and nurse healing costs more for 10 minutes
					 *   - player is inflicted one really bad debuff for a short amount of time
					 *   - player is inflicted an annoying debuff for a while
					 *   - player's buffs are cleared
					 */
					rollEvent = Main.rand.Next(0, 4);

					switch(rollEvent){
						case 0:
							InformEveryone("Bad shop prices for 10 minutes!");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: false, duration: 10 * 60 * 60);
							break;
						case 1:
							InformEveryone("One nasty debuff!");

							(int, int) debuff = Main.rand.Next(new (int, int)[]{
								(BuffID.Suffocation, 20 * 60),
								(BuffID.CursedInferno, 18 * 60),
								(BuffID.Stoned, 10 * 60),
								(BuffID.Burning, 60 * 60),
								(BuffID.Cursed, 4 * 60),
								(BuffID.NoBuilding, 7 * 60),
								(ModContent.BuffType<PrimordialWrath>(), 12 * 60)
							});
							owner.AddBuff(debuff.Item1, debuff.Item2);
							break;
						case 2:
							InformEveryone("One annoying debuff!");

							debuff = Main.rand.Next(new (int, int)[]{
								(BuffID.Slow, 60 * 60),
								(BuffID.Silenced, 60 * 60),
								(BuffID.Weak, 120 * 60),
								(BuffID.BrokenArmor, 90 * 60),
								(BuffID.Darkness, 60 * 60),
								(BuffID.Bleeding, 20 * 60),
								(BuffID.Poisoned, 20 * 60)
							});
							owner.AddBuff(debuff.Item1, debuff.Item2);
							break;
						case 3:
							InformEveryone("Cleared buffs!");

							for(int i = 0; i < Player.MaxBuffs; i++){
								if(owner.buffType[i] == 0 || owner.buffTime[i] == 0)
									continue;

								if(!Main.debuff[owner.buffType[i]]){
									owner.DelBuff(i);
									i--;
								}
							}
							break;
					}
					break;
				#endregion
				#region Roll 4
				case 4:
					/*   Effects:
					 *   
					 *   - the player's spawnpoint is removed (if a bed has been placed and set as the spawnpoint)
					 *   - the player is teleported to a random location on the map
					 *   - the player has reversed gravity applied for a certain amount of time
					 */
					rollEvent = Main.rand.Next(0, 3);

					if(rollEvent == 0 && owner.SpawnX == Main.spawnTileX && owner.SpawnY == Main.spawnTileY)
						while((rollEvent = Main.rand.Next(0, 3)) == 0);

					switch(rollEvent){
						case 0:
							InformEveryone("Their spawnpoint removed!");

							owner.RemoveSpawn();
							break;
						case 1:
							//Teleportation potion effect
							InformEveryone("A free teleportation to somewhere in the world!");

							if(Main.netMode == NetmodeID.SinglePlayer)
								owner.TeleportationPotion();
							else if(Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendData(MessageID.TeleportationPotion);
							break;
						case 2:
							InformEveryone("Forced reverse gravity!");

							owner.GetModPlayer<DicePlayer>().SetForcedGravity();
							break;
					}
					break;
				#endregion
				#region Roll 5
				case 5:
					/*   Effects:
					 *   
					 *   - a random town NPC dies
					 *   - minus 50 fishing power for two days
					 *   - an annoying debuff for a short amount of time
					 */
					rollEvent = Main.rand.Next(0, 3);

					switch(rollEvent){
						case 0:
							InformEveryone("One dead town NPC!");

							List<int> townies = new List<int>();

							for(int i = 0; i < Main.maxNPCs; i++){
								NPC npc = Main.npc[i];
								if(npc.active && npc.townNPC)
									townies.Add(i);
							}

							if(townies.Count > 0){
								NPC victim = Main.npc[Main.rand.Next(townies)];

								victim.StrikeNPCNoInteraction(victim.lifeMax * 4, 0f, 0, crit: true);
							}
							break;
						case 1:
							InformEveryone("Lowered fishing skill!");

							owner.GetModPlayer<DicePlayer>().SetFishModifier(detriment: true);
							break;
						case 2:
							InformEveryone("One annoying debuff!");

							(int, int) debuff = Main.rand.Next(new (int, int)[]{
								(BuffID.Slow, 15 * 60),
								(BuffID.Silenced, 20 * 60),
								(BuffID.Weak, 45 * 60),
								(BuffID.BrokenArmor, 60 * 60),
								(BuffID.Darkness, 20 * 60),
								(BuffID.Bleeding, 12 * 60),
								(BuffID.Poisoned, 12 * 60)
							});
							owner.AddBuff(debuff.Item1, debuff.Item2);
							break;
					}

					break;
				#endregion
				#region Roll 6
				case 6:
					/*   Effects:
					 *   
					 *   - NPC shops and nurse prices cost more for 4 minutes
					 *   - (Desolation Mode only) completely drain the player's Stamina
					 *   - completely drain the player's Mana
					 */
					do{
						rollEvent = Main.rand.Next(0, 3);
					}while(rollEvent == 1 && !WorldEvents.desoMode);

					switch(rollEvent){
						case 0:
							InformEveryone("Bad shop prices for 4 minutes!");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: false, 4 * 60 * 60);
							break;
						case 1:
							InformEveryone("Heavy fatigue!");

							owner.GetModPlayer<StaminaPlayer>().stamina.ForceExhaustion();
							break;
						case 2:
							InformEveryone("No mana!");

							owner.statMana = 0;
							owner.manaRegenDelay = 180;
							break;
					}
					break;
				#endregion
				#region Roll 7
				case 7:
					/*   Effects:
					 *   
					 *   - slap the player
					 *   - lowered fishing skill for one day
					 */
					rollEvent = Main.rand.Next(0, 2);

					switch(rollEvent){
						case 0:
							InformEveryone("A slap to the face!");

							owner.Hurt(PlayerDeathReason.ByCustomReason($"{owner.name} got slapped."), 20, 0, Crit: true);

							owner.velocity = new Vector2(0, -8f);
							break;
						case 1:
							InformEveryone("Lowered fishing skill!");

							owner.GetModPlayer<DicePlayer>().SetFishModifier(detriment: true, TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
					}
					break;
				#endregion
				#region Roll 8
				case 8:
					/*   Effects:
					 *   
					 *   - do nothing (rare)
					 *   - give the player 1 dirt
					 *   - give the player Mana Sickness for 4 minutes
					 *   - give the player Potion Sickness for 2 minutes
					 *   - freeze the player for 5 seconds
					 *   - stone the player for 5 seconds
					 */
					do{
						rollEvent = Main.rand.Next(0, 6);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.1f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							InformEveryone("Some dirt.");

							owner.QuickSpawnItem(ItemID.DirtBlock, 1);
							break;
						case 2:
							InformEveryone("Mana Sickness for 4 minutes!");

							owner.AddBuff(BuffID.ManaSickness, 4 * 60 * 60);
							break;
						case 3:
							InformEveryone("Potion Sickness for 2 minutes!");

							owner.AddBuff(BuffID.PotionSickness, 2 * 60 * 60);
							owner.potionDelay = 2 * 60;
							break;
						case 4:
							InformEveryone("Their own personal icicle!");

							owner.AddBuff(BuffID.Frozen, 5 * 60);
							break;
						case 5:
							InformEveryone("Stoned for 5 seconds!");

							owner.AddBuff(BuffID.Stoned, 5 * 60);
							break;
					}
					break;
				#endregion
				#region Roll 9
				case 9:
					/*   Effects:
					 *   
					 *   - do nothing (somewhat rare)
					 *   - give the player one Copper Coin
					 *   - give the player 50 dirt
					 *   - give the player a broken Copper weapon/tool
					 */
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.25f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							InformEveryone("1 copper coin!");

							owner.QuickSpawnItem(ItemID.CopperCoin, 1);
							break;
						case 2:
							InformEveryone("A small pile of dirt.");

							owner.QuickSpawnItem(ItemID.DirtBlock, 50);
							break;
						case 3:
							int drop = Main.rand.Next(new int[]{
								ItemID.CopperPickaxe,
								ItemID.CopperAxe,
								ItemID.CopperHammer,
								ItemID.CopperShortsword,
								ItemID.CopperBroadsword,
								ItemID.CopperBow
							});

							int i = Item.NewItem(owner.Center, drop, 1, prefixGiven: PrefixID.Broken, noGrabDelay: true);
							Item item = Main.item[i];

							InformEveryone($"One [i/p{item.prefix}:{item.type}]!");
							break;
					}
					break;
				#endregion
				#region Roll 10
				case 10:
					/*   Effects:
					 *   
					 *   - do nothing (common)
					 *   - give the player 1 Silver Coin
					 *   - give the player 1-5 Copper/Tin Bars, 1-4 Iron/Lead Bars, 1-3 Silver/Tungsten Bars or 1-2 Gold/Platinum Bars
					 *   - give the player an Amethyst, Topaz or Sapphire
					 */
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.5f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							InformEveryone("1 Silver Coin!");

							owner.QuickSpawnItem(ItemID.SilverCoin, 1);
							break;
						case 2:
							(int, int) drop = Main.rand.Next(new (int, int)[]{
								(WorldGen.CopperTierOre == TileID.Copper ? ItemID.CopperBar : ItemID.TinBar,
									Main.rand.Next(1, 6)),
								(WorldGen.IronTierOre == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(1, 5)),
								(WorldGen.SilverTierOre == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(1, 4)),
								(WorldGen.GoldTierOre == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(1, 3))
							});

							Item item = new Item();
							item.SetDefaults(drop.Item1);

							owner.QuickSpawnItem(item, drop.Item2);

							InformEveryone($"{drop.Item2} {item.Name}{(drop.Item2 > 1 ? "s" : "")}!");
							break;
						case 3:
							int type = Main.rand.Next(new int[]{
								ItemID.Amethyst,
								ItemID.Topaz,
								ItemID.Sapphire
							});

							owner.QuickSpawnItem(type, 1);

							InformEveryone($"1 {Lang.GetItemName(type)}!");
							break;
					}

					break;
				#endregion
				#region Roll 11
				case 11:
					/*   Effects:
					 *   
					 *   - do nothing (common)
					 *   - give the player 20 Silver Coins
					 *   - give the player 1-5 Journeyman's Bait or Appentice Bait
					 */
					do{
						rollEvent = Main.rand.Next(0, 3);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.5f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							InformEveryone("20 Silver Coins!");

							owner.QuickSpawnItem(ItemID.SilverCoin, 20);
							break;
						case 2:
							(int, int) drop = (Main.rand.NextFloat() < 0.6f ? ItemID.JourneymanBait : ItemID.ApprenticeBait, Main.rand.Next(1, 6));

							InformEveryone($"{drop.Item2} {Lang.GetItemName(drop.Item1)}");

							owner.QuickSpawnItem(drop.Item1, drop.Item2);
							break;
					}
					break;
				#endregion
				#region Roll 12
				case 12:
					/*   Effects:
					 *   
					 *   - do nothing (somewhat rare)
					 *   - give the player 3-10 Copper/Tin Bars, 2-8 Iron/Lead Bars, 1-5 Silver/Tungsten Bars or 1-4 Gold/Platinum Bars
					 *   - give the player 3 Amethyst, 2 Topaz, 2 Sapphire, 1 Emerald, 1 Ruby or 1 Diamond
					 *   - give the player 1 Gold Coin
					 */
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.25f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							(int, int) drop = Main.rand.Next(new (int, int)[]{
								(WorldGen.CopperTierOre == TileID.Copper ? ItemID.CopperBar : ItemID.TinBar,
									Main.rand.Next(3, 11)),
								(WorldGen.IronTierOre == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(2, 8)),
								(WorldGen.SilverTierOre == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(1, 6)),
								(WorldGen.GoldTierOre == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(1, 5))
							});

							Item item = new Item();
							item.SetDefaults(drop.Item1);

							owner.QuickSpawnItem(item, drop.Item2);

							InformEveryone($"{drop.Item2} {item.Name}{(drop.Item2 > 1 ? "s" : "")}!");
							break;
						case 2:
							drop = Main.rand.Next(new (int, int)[]{
								(ItemID.Amethyst, 3),
								(ItemID.Topaz, 2),
								(ItemID.Sapphire, 2),
								(ItemID.Emerald, 1),
								(ItemID.Ruby, 1),
								(ItemID.Diamond, 1)
							});
							
							item = new Item();
							item.SetDefaults(drop.Item1);

							owner.QuickSpawnItem(item, drop.Item2);

							InformEveryone($"{drop.Item2} {item.Name}{(drop.Item2 > 1 ? "s" : "")}!");
							break;
						case 3:
							InformEveryone("1 Gold Coin!");

							owner.QuickSpawnItem(ItemID.GoldCoin, 1);
							break;
					}
					break;
				#endregion
				#region Roll 13
				case 13:
					/*   Effects:
					 *   
					 *   - do nothing (rare)
					 *   - starting materials (100 Wood, 50 Stone, 60 Gel, 2 Life Crystals, 2 Mana Crystals)
					 *   - increased fishing skill for 1 day
					 *   - a random early accessory (Aglet, Anklet of the Wind, Cloud in a Bottle, Hermes Boots)
					 */
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while(rollEvent == 0 && Main.rand.NextFloat() >= 0.1f);

					switch(rollEvent){
						case 0:
							InformEveryone("Nothing!");
							break;
						case 1:
							InformEveryone("A kit of starter items!");

							owner.QuickSpawnItem(ItemID.Wood, 100);
							owner.QuickSpawnItem(ItemID.StoneBlock, 50);
							owner.QuickSpawnItem(ItemID.Gel, 60);
							owner.QuickSpawnItem(ItemID.LifeCrystal, 2);
							owner.QuickSpawnItem(ItemID.ManaCrystal, 2);
							break;
						case 2:
							InformEveryone("Increased fishing skill!");

							owner.GetModPlayer<DicePlayer>().SetFishModifier(detriment: false, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 3:
							int drop = Main.rand.Next(new int[]{
								ItemID.Aglet,
								ItemID.AnkletoftheWind,
								ItemID.CloudinaBottle,
								ItemID.HermesBoots
							});

							int i = Item.NewItem(owner.Center, drop, 1, prefixGiven: -1, noGrabDelay: true);
							Item item = Main.item[i];

							InformEveryone($"One [i/p{item.prefix}:{item.type}]!");
							break;
					}

					break;
				#endregion
				#region Roll 14
				case 14:
					/*   Effects:
					 *   
					 *   - gives the player 5 Gold Coins
					 *   - gives the player 6-20 Iron/Lead Bars, 4-12 Silver/Tungsten Bars or 4-8 Gold/Platinum Bars
					 *   - gives the player 10 Amethyst, 8 Topaz, 7 Saphhire, 5 Emeralds, 3 Ruby or 1 Diamond
					 *   - (Only post-EoW/BoC) gives the player 18-35 Tissue Samples/Shadow Scale and 8-15 Demonite Bars
					 *   - (Only in Hardmode) gives the player 8-12 Hellstone bars
					 */
					do{
						rollEvent = Main.rand.Next(0, 5);
					}while((rollEvent == 3 && !NPC.downedBoss2) || (rollEvent == 4 && !Main.hardMode));

					switch(rollEvent){
						case 0:
							InformEveryone("5 Gold Coins!");

							owner.QuickSpawnItem(ItemID.GoldCoin, 5);
							break;
						case 1:
							(int, int) drop = Main.rand.Next(new (int, int)[]{
								(WorldGen.IronTierOre == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(6, 21)),
								(WorldGen.SilverTierOre == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(4, 13)),
								(WorldGen.GoldTierOre == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(4, 9))
							});

							Item item = new Item();
							item.SetDefaults(drop.Item1);

							owner.QuickSpawnItem(item, drop.Item2);

							InformEveryone($"{drop.Item2} {item.Name}s!");
							break;
						case 2:
							drop = Main.rand.Next(new (int, int)[]{
								(ItemID.Amethyst, 10),
								(ItemID.Topaz, 8),
								(ItemID.Sapphire, 7),
								(ItemID.Emerald, 5),
								(ItemID.Ruby, 3),
								(ItemID.Diamond, 1)
							});
							
							item = new Item();
							item.SetDefaults(drop.Item1);

							owner.QuickSpawnItem(item, drop.Item2);

							InformEveryone($"{drop.Item2} {item.Name}{(drop.Item2 > 1 ? "s" : "")}!");
							break;
						case 3:
							(int, int) material = (WorldGen.crimson ? ItemID.TissueSample : ItemID.ShadowScale, Main.rand.Next(18, 36));
							(int, int) bar = (WorldGen.crimson ? ItemID.CrimtaneBar : ItemID.DemoniteBar, Main.rand.Next(8, 16));

							owner.QuickSpawnItem(bar.Item1, bar.Item2);
							owner.QuickSpawnItem(material.Item1, material.Item2);

							InformEveryone($"Some {(WorldGen.crimson ? "Crimtane Bars and Tissue Samples" : "Demonite Bars and Shadow Scales")}!");
							break;
						case 4:
							InformEveryone("Some Hellstone bars!");

							owner.QuickSpawnItem(ItemID.HellstoneBar, Main.rand.Next(8, 13));
							break;
					}
					break;
				#endregion
				#region Roll 15
				case 15:
					/*   Effects:
					 *   
					 *   - decreased shop prices for 20 minutes
					 *   - 20% max life healed
					 *   - (only if the Angler is present) a random quest fish
					 *   - a random small buff
					 */
					bool noAngler = !NPC.AnyNPCs(NPCID.Angler);
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while(rollEvent == 2 && noAngler);

					switch(rollEvent){
						case 0:
							InformEveryone("Decreased shop prices!");
							break;
						case 1:
							InformEveryone("Some health recovered!");

							owner.HealEffect((int)(owner.statLifeMax2 * 0.2f), broadcast: true);
							break;
						case 2:
							InformEveryone("A random quest fish!");

							Item quest = new Item();
							quest.SetDefaults(Main.rand.Next(Main.anglerQuestItemNetIDs));

							owner.QuickSpawnItem(quest, 1);
							break;
						case 3:
							InformEveryone("A random regular buff!");

							(int, int) buff = Main.rand.Next(new (int, int)[]{
								(BuffID.Regeneration, 2 * 60 * 60),
								(BuffID.Swiftness, 3 * 60 * 60),
								(BuffID.Thorns, 5 * 60 * 60),
								(BuffID.Ironskin, 3 * 60 * 60),
								(BuffID.Hunter, 4 * 60 * 60),
								(BuffID.Archery, 3 * 60 * 60),
								(BuffID.WellFed, 3 * 60 * 60)
							});

							owner.AddBuff(buff.Item1, buff.Item2);
							break;
					}
					break;
				#endregion
				#region Roll 16
				case 16:
					/*   Effects:
					 *   
					 *   - the player is given 20 Gold Coins
					 *   - (Hardmode only) the player is given 1-10 bars of one of the Hardmode ores
					 *   - the player is given 10 Life Crystals (Pre-Hardmode) or 6 Life Fruit (Hardmode)
					 *   - NPC shop prices and nurse heal cost are lowered for 1 day
					 *   - (Post-Mechs only) the player is given 1-8 Hallowed Bars
					 *   - the player is given 1-5 Master Bait
					 *   - (Hardmode only) the player is given 1 Truffle Worm
					 */
					do{
						rollEvent = Main.rand.Next(0, 7);
					}while((rollEvent == 1 && !Main.hardMode)
						|| (rollEvent == 4 && !(NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3))
						|| (rollEvent == 6 && !Main.hardMode));

					switch(rollEvent){
						case 0:
							InformEveryone("20 Gold Coins!");

							owner.QuickSpawnItem(ItemID.GoldCoin, 20);
							break;
						case 1:
							int drop = Main.rand.Next(new int[]{
								ItemID.CobaltBar, ItemID.PalladiumBar, ItemID.MythrilBar, ItemID.OrichalcumBar, ItemID.AdamantiteBar, ItemID.TitaniumBar
							});

							Item item = new Item();
							item.SetDefaults(drop);

							int count = Main.rand.Next(1, 11);
							owner.QuickSpawnItem(item, count);

							InformEveryone($"{count} {item.Name}{(count > 1 ? "s" : "")}");
							break;
						case 2:
							InformEveryone(Main.hardMode
								? "6 Life Fruit"
								: "10 Life Crystals");

							if(Main.hardMode)
								owner.QuickSpawnItem(ItemID.LifeFruit, 6);
							else
								owner.QuickSpawnItem(ItemID.LifeCrystal, 10);
							break;
						case 3:
							InformEveryone("Decreased shop prices!");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: true, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 4:
							count = Main.rand.Next(1, 9);

							owner.QuickSpawnItem(ItemID.HallowedBar, count);

							InformEveryone($"{count} Hallowed Bar{(count > 1 ? "s" : "")}");
							break;
						case 5:
							count = Main.rand.Next(1, 6);

							owner.QuickSpawnItem(ItemID.MasterBait, count);

							InformEveryone($"{count} Master Bait!");
							break;
						case 6:
							InformEveryone("1 Truffle Worm!");

							owner.QuickSpawnItem(ItemID.TruffleWorm, 1);
							break;
					}
					break;
				#endregion
				#region Roll 17
				case 17:
					/*   Effects:
					 *   
					 *   - 1 really good buff
					 *   - increased i-frames for 5 minutes
					 *   - (Post-Plantera only) 1-6 Chlorophyte Bars
					 *   - (Post-Lunatic Cultist only) 1-4 fragments for each type of Lunar Pillar
					 *   - (Post-Moon Lord only) 1-5 Luminite Bars
					 */
					do{
						rollEvent = Main.rand.Next(0, 4);
					}while((rollEvent == 2 && !NPC.downedPlantBoss) || (rollEvent == 3 && !NPC.downedAncientCultist) || (rollEvent == 4 && !NPC.downedMoonlord));

					switch(rollEvent){
						case 0:
							InformEveryone("One powerful buff!");

							(int, int) buff = Main.rand.Next(new (int, int)[]{
								(BuffID.Wrath, 10 * 60 * 60),
								(BuffID.Rage, 10 * 60 * 60),
								(BuffID.Endurance, 6 * 60 * 60),
								(BuffID.WellFed, 45 * 60 * 60),
								(BuffID.Summoning, 20 * 60 * 60),
								(BuffID.MagicPower, 10 * 60 * 60),
								(BuffID.ManaRegeneration, 15 * 60 * 60),
								(BuffID.Lifeforce, 10 * 60 * 60)
							});

							owner.AddBuff(buff.Item1, buff.Item2);
							break;
						case 1:
							InformEveryone("Longer intangibility!");

							owner.GetModPlayer<DicePlayer>().SetMoreIFrames(5 * 60 * 60);
							break;
						case 2:
							int count = Main.rand.Next(1, 7);

							owner.QuickSpawnItem(ItemID.ChlorophyteBar, count);

							InformEveryone($"{count} Chlorophyte Bar{(count > 1 ? "s" : "")}");
							break;
						case 3:
							InformEveryone("Lunar pillar fragments!");

							int[] fragments = new int[]{ ItemID.FragmentSolar, ItemID.FragmentNebula, ItemID.FragmentVortex, ItemID.FragmentStardust };
							for(int i = 0; i < 4; i++)
								owner.QuickSpawnItem(fragments[i], Main.rand.Next(1, 5));
							break;
						case 4:
							count = Main.rand.Next(1, 6);

							owner.QuickSpawnItem(ItemID.LunarBar, count);

							InformEveryone($"{count} Luminite Bar{(count > 1 ? "s" : "")}");
							break;
					}
					break;
				#endregion
				#region Roll 18
				case 18:
					/*   Effects:
					 *   
					 *   - give the player 1 Platinum Coin
					 *   - decreased NPC shop prices and nurse heal count for 1 ingame day
					 *   - no Stamina decay for 2 minutes
					 *   - 100 of each wood type
					 *   - a Bone Key
					 */
					rollEvent = Main.rand.Next(0, 5);

					switch(rollEvent){
						case 0:
							InformEveryone("1 Platinum Coin!");

							owner.QuickSpawnItem(ItemID.PlatinumCoin, 1);
							break;
						case 1:
							InformEveryone("Decreased shop prices!");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: true, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 2:
							InformEveryone("No Stamina Decay for 2 minutes!");

							owner.GetModPlayer<DicePlayer>().SetNSD(2 * 60 * 60);
							break;
						case 3:
							InformEveryone("An abundance of wood!");

							int[] types = new int[]{
								ItemID.Wood, ItemID.BorealWood, ItemID.PalmWood, ItemID.Pearlwood, ItemID.RichMahogany, ItemID.Shadewood, ItemID.Ebonwood
							};
							for(int i = 0; i < types.Length; i++)
								owner.QuickSpawnItem(types[i], 100);
							break;
						case 4:
							InformEveryone("1 Bone Key!");

							owner.QuickSpawnItem(ItemID.BoneKey, 1);
							break;
					}
					break;
				#endregion
				#region Roll 19
				case 19:
					/*   Effects:
					 *   
					 *   - 50% max life healed
					 *   - an extra life
					 *   - 3 really good buffs
					 *   - all debuffs cleared
					 *   - potion sickness timer cleared
					 */
					rollEvent = Main.rand.Next(0, 5);

					switch(rollEvent){
						case 0:
							InformEveryone("A lot of health recovered!");

							owner.HealEffect((int)(owner.statLifeMax2 * 0.5f), broadcast: true);
							break;
						case 1:
							InformEveryone("One extra life!");

							owner.GetModPlayer<DicePlayer>().extraLives++;
							break;
						case 2:
							InformEveryone("Some very good buffs!");

							(int, int)[] buffs = new (int, int)[]{
								(BuffID.Lifeforce, 6 * 60 * 60),
								(BuffID.ShadowDodge, 1 * 60 * 60),
								(BuffID.RapidHealing, 3 * 60 * 60),
								(-1, -1), //Nebula set buffs
								(-2, -2), //Beetle armor buffs
								(BuffID.AmmoBox, 10 * 60 * 60),
								(BuffID.Clairvoyance, 10 * 60 * 60),
								(BuffID.Sharpened, 10 * 60 * 60)
							};
							int added = 0;
							for(int i = 0; i < 3; i++){
								(int, int) buff = Main.rand.Next(buffs);
								if(buff.Item1 == -1){
									owner.AddBuff(BuffID.NebulaUpDmg3, 3 * 60 * 60);
									owner.AddBuff(BuffID.NebulaUpLife3, 3 * 60 * 60);
									owner.AddBuff(BuffID.NebulaUpMana3, 3 * 60 * 60);
									added += 3;
								}else if(buff.Item1 == -2){
									owner.AddBuff(BuffID.BeetleEndurance3, 3 * 60 * 60);
									owner.AddBuff(BuffID.BeetleMight3, 3 * 60 * 60);
									added += 2;
								}else{
									owner.AddBuff(buff.Item1, buff.Item2);
									added++;
								}

								if(added >= 3)
									return;
							}
							break;
						case 3:
							InformEveryone("All debuffs cleared!");

							for(int i = 0; i < Player.MaxBuffs; i++){
								if(Main.debuff[owner.buffType[i]]){
									owner.DelBuff(i);
									i--;
								}
							}

							break;
						case 4:
							InformEveryone("Healing potion sickness cured!");

							owner.ClearBuff(BuffID.PotionSickness);
							owner.potionDelay = 0;
							break;
					}
					break;
				#endregion
				#region Roll 20
				case 20:
					/*   Effects:
					 *   
					 *   - player is given 100 platinum coins
					 *   - godmode for 1 minute
					 *   - 3 extra lives
					 *   - no Stamina decay for 4 minutes
					 *   - all damage dealt is doubled for 2 minutes
					 *   - infinite ammo for 3 minutes
					 *   - infinite mana for 3 minutes
					 */
					rollEvent = Main.rand.Next(0, 7);

					switch(rollEvent){
						case 0:
							InformEveryone("100 Platinum coins!");

							owner.QuickSpawnItem(ItemID.PlatinumCoin, 100);
							break;
						case 1:
							InformEveryone("Godmode for 1 minute!");

							owner.GetModPlayer<DicePlayer>().godmodeTimer = 1 * 60 * 60;
							break;
						case 2:
							InformEveryone("Three extra lives!");

							owner.GetModPlayer<DicePlayer>().extraLives += 3;
							break;
						case 3:
							InformEveryone("No Stamina Decay for 4 minutes!");

							owner.GetModPlayer<DicePlayer>().SetNSD(4 * 60 * 60);
							break;
						case 4:
							InformEveryone("Doubled weapon damage!");

							owner.GetModPlayer<DicePlayer>().buffDamageTimer = 2 * 60 * 60;
							break;
						case 5:
							InformEveryone("Infinite ammo for 3 minutes!");

							owner.GetModPlayer<DicePlayer>().endlessClipTimer = 3 * 60 * 60;
							break;
						case 6:
							InformEveryone("Infinite mana for 3 minutes!");

							owner.GetModPlayer<DicePlayer>().endlessManaTimer = 3 * 60 * 60;
							break;
					}
					break;
				#endregion
			}
		}

		private static bool TryTeleportPlayerToDungeon(out int x, out int y){
			List<(int, int)> dungeonWalls = new List<(int, int)>();

			bool dungeonLeft = Main.dungeonX < Main.maxTilesX / 2;
			for(y = (int)Main.rockLayer + 100; y < Main.maxTilesY - 200; y++){
				for(x = dungeonLeft ? 0 : Main.maxTilesX - 1; dungeonLeft ? x < Main.maxTilesX / 3 : x > Main.maxTilesX * 2 / 3; x = dungeonLeft ? x + 1 : x - 1){
					Tile tile = Framing.GetTileSafely(x, y);
					if(Main.wallDungeon[tile.wall])
						dungeonWalls.Add((x, y));
				}

				if(dungeonWalls.Count > 0)
					break;
			}

			if(dungeonWalls.Count == 0){
				x = 0;
				y = 0;
				return false;
			}

			(int, int) coord = dungeonWalls[dungeonWalls.Count / 2];
			x = coord.Item1;
			y = coord.Item2;
			return true;
		}

		private static bool TryTeleportPlayerToBrazil(out int x, out int y){
			Tile tile;
			int tries = 4000;
			do{
				x = Main.rand.Next(0, Main.maxTilesX);
				y = Main.rand.Next(Main.maxTilesY - 200, Main.maxTilesY);

				tile = Framing.GetTileSafely(x, y);

				tries--;

				if(tries <= 0){
					x = 0;
					y = 0;
					return false;
				}
			}while(tile.active() || !tile.lava() || tile.liquid < 200);

			return true;
		}
	}
}
