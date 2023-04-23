using CosmivengeonMod.API.Commands;
using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Players;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Dice {
	public class DiceOfFateD20Dice : ModProjectile {
		public override string Texture => "CosmivengeonMod/Items/Tools/Dice/DiceOfFateD20";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("D20");
		}

		public override void SetDefaults() {
			Projectile.tileCollide = true;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.scale = 8f / Projectile.width;
		}

		internal bool chooseNumber;
		private SpriteEffects effects;

		internal int random = -1;

		private float realAlpha = 0;

		private int GetMaxLuck(int unluckyFactor) {
			if (unluckyFactor == 0)
				return 20;
			else if (unluckyFactor < 3)
				return 15;
			else if (unluckyFactor < 5)
				return 10;
			else
				return 8;
		}

		public override void AI() {
			float oldYVel = Projectile.velocity.Y;
			Projectile.velocity.Y += 21f / 60f;

			Projectile.velocity.X *= 1f - 1.5f / 60f;

			if (Math.Abs(Projectile.velocity.X) < 0.08f)
				Projectile.velocity.X = 0;

			//Keep it alive as long as it hasn't stopped yet
			if (!chooseNumber) {
				Projectile.timeLeft = 60;
				Projectile.netUpdate = true;
			}

			//Only the server should update the projectile
			if (Main.myPlayer != Projectile.owner || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (Projectile.velocity.LengthSquared() > 10 * 10)
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 10f;

			if (!chooseNumber) {
				if (Main.rand.NextFloat() * 5f * (Projectile.velocity.Length() / 10f) >= 0.5f) {
					effects = Main.rand.NextBool() ? SpriteEffects.None : SpriteEffects.FlipVertically;
					Projectile.rotation = Main.rand.NextFloat(0, MathHelper.TwoPi);
				}
			} else {
				realAlpha += 255f / 90f;
				Projectile.alpha = (int)realAlpha;
			}

			if (Projectile.oldVelocity.X == 0 && Projectile.velocity.X == 0f && oldYVel == 0 && !chooseNumber) {
				chooseNumber = true;
				Projectile.timeLeft = 90;

				//Roll was set by a command if "random" isn't -1
				if (random == -1) {
					//Events for a 1 and 20 are very extreme.  Make sure they only happen after two random calls
					random = Main.rand.Next(1, 21);
					if ((random == 1 || random == 20) && Main.rand.NextFloat() > 0.2f)
						random = Main.rand.Next(1, 21);

					//If the unlucky factor is > 0, then make bad rolls happen more often that good rolls
					int unlucky = (int)Projectile.ai[0];
					int maxLuck = GetMaxLuck(unlucky);
					if (random > maxLuck)
						random = Main.rand.Next(1, maxLuck + 1);
				}

				Color color = random < 5
					? CombatText.LifeRegenNegative
					: (random < 10
						? CombatText.DamagedHostileCrit
						: (random < 15
							? CombatText.DamagedHostile
							: CombatText.LifeRegen));

				CombatText.NewText(new Rectangle((int)Projectile.Center.X - 8, (int)Projectile.Center.Y - 40, 16, 16), color, random, dramatic: true);
			}
		}

		public override bool? CanCutTiles() => true;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = (int)(-oldVelocity.Y * (oldVelocity.Y > 0 ? 0.78f : 1f));

				//Stop bouncing if the bounce up was too slow
				if (-0.3f < Projectile.velocity.Y && Projectile.velocity.Y < 0)
					Projectile.velocity.Y = 0;
			}
			if (Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = (int)(-oldVelocity.X * 0.78f);

			return false;
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2f, Projectile.scale, effects, 0);
			return false;
		}

		private void InformEveryoneLiteral(string result) {
			Player owner = Main.player[Projectile.owner];

			string dice = (random == 1 || random == 20) ? $"NAT {random}" : random.ToString();

			if (Main.netMode == NetmodeID.SinglePlayer)
				Main.NewText(Language.GetTextValue("Mods.CosmivengeonMod.DiceText.PlayerRolledMessage", owner.name, dice, result));
			else if (Main.netMode == NetmodeID.Server)
				ChatHelper.BroadcastChatMessage(NetworkText.FromKey("Mods.CosmivengeonMod.DiceText.PlayerRolledMessage", owner.name, dice, result), Color.White);
		}

		private void InformLootDrop(int type, int stack, string lootMsgType = "", int? prefix = null) {
			string msg = Language.GetTextValue("Mods.CosmivengeonMod.DiceText.Loot" + lootMsgType);

			if (prefix is { } p && p > 0 && p < PrefixLoader.PrefixCount)
				InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText.LootWithPrefixDrop", msg, type, stack, p));
			else
				InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText.LootDrop", msg, type, stack));
		}

		private void InformEveryone(string key) => InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText." + key));

		private void InformEveryone(string key, object arg0) => InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText." + key, arg0));

		private void InformEveryone(string key, object arg0, object arg1) => InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText." + key, arg0, arg1));

		private void InformEveryone(string key, object arg0, object arg1, object arg2) => InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText." + key, arg0, arg1, arg2));

		private void InformEveryone(string key, params object[] args) => InformEveryoneLiteral(Language.GetTextValue("Mods.CosmivengeonMod.DiceText." + key, args));

		public override void Kill(int timeLeft) {
			Player owner = Main.player[Projectile.owner];

			//Stuff should not run on clients other than the one for the owner (or the server)
			if (Main.myPlayer != owner.whoAmI || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			//Effects go from really bad (1) to really good (20), with 10 and 11 being "neutral" effects
			//Only one effect is chosen per roll
			int rollEvent;
			bool validEvent;
			int type, stack;

			var lootSource = owner.GetSource_Loot();
			switch (random) {
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
					do {
						rollEvent = Main.rand.Next(0, 5);

						if (rollEvent == 3) {
							if (owner.difficulty == 0) {
								if (!dungeonChecked && !NPC.downedBoss3) {
									dungeonChecked = true;
									validEvent = TryTeleportPlayerToDungeon(out teleXDungeon, out teleYDungeon);
								} else
									validEvent = false;
							} else
								validEvent = false;
						} else if (rollEvent == 4) {
							if (!hellChecked) {
								hellChecked = true;
								validEvent = TryTeleportPlayerToBrazil(out teleXHell, out teleYHell);
							} else
								validEvent = false;
						} else
							validEvent = true;
					} while (!validEvent);

					switch (rollEvent) {
						case 0:
							//Ouch!
							InformEveryone(owner.difficulty == 0 ? "BadLuck.Kill" : "BadLuck.Pain");

							var deathReason = PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.CosmivengeonMod.KillReason.Unlucky", owner.name));

							if (owner.difficulty == 0)
								owner.KillMe(deathReason, 9999, 0);
							else {
								int life = owner.statLife;
								int newLife = owner.statLife / (owner.difficulty == 1 ? 2 : 4);

								if (owner.statLife < 1)
									owner.statLife = 1;

								owner.Hurt(deathReason, life - newLife, 0, Crit: true);
							}
							break;
						case 1:
							InformEveryone("BadLuck.NoMoney");

							for (int i = 0; i < 54; i++) {
								Item item = owner.inventory[i];
								if (item.type == ItemID.CopperCoin || item.type == ItemID.SilverCoin || item.type == ItemID.GoldCoin || item.type == ItemID.PlatinumCoin) {
									int index = Item.NewItem(owner.GetSource_DropAsItem(), owner.Center, item.type, item.stack);
									Main.item[index].noGrabDelay = 5 * 60;

									item.TurnToAir();
								}
							}
							break;
						case 2:
							InformEveryone("BadLuck.TwoNastyDebuffs");

							for (int i = 0; i < 2; i++) {
								(int, int) debuff = Main.rand.Next(new (int, int)[] {
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
							InformEveryone("BadLuck.BoneZone");

							owner.Teleport(new Vector2(teleXDungeon, teleYDungeon) * 16, Style: 1);
							break;
						case 4:
							InformEveryone("BadLuck.Brazil");

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

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Horde");

							for (int i = 0; i < 20; i++) {
								int randX, randY;
								int tries = 20;
								do {
									randX = Main.rand.Next(-5 * 16, 5 * 16 + 1) + (int)owner.Bottom.X;
									randY = Main.rand.Next(-2 * 16, 1) + (int)owner.Bottom.Y;

									tries--;
									if (tries <= 0)
										break;
								} while (Framing.GetTileSafely(randX / 16, randY / 16).HasTile);

								int hordeType;
								if (!Main.hardMode)
									hordeType = Main.rand.NextFloat() > 0.2f ? NPCID.Zombie : NPCID.UndeadMiner;
								else if (Main.hardMode && !NPC.downedPlantBoss)
									hordeType = NPCID.PossessedArmor;
								else if (Main.hardMode && NPC.downedPlantBoss && !NPC.downedMoonlord)
									hordeType = WorldGen.crimson ? NPCID.Herpling : NPCID.Corruptor;
								else
									hordeType = Main.rand.Next(new int[] {
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

								NPC npc = NPC.NewNPCDirect(new EntitySource_WorldEvent("Cosmivengeon:D20 Roll"), randX, randY, hordeType);
								npc.Bottom = new Vector2(randX, randY);
							}
							break;
						case 1:
							InformEveryone("BadLuck.JunkSpam");

							for (int i = 0; i < 50; i++) {
								Item item = owner.inventory[i];
								if (item.IsAir) {
									item = owner.inventory[i] = new Item();
									item.SetDefaults(Main.rand.Next(new int[] {
										ItemID.OldShoe, ItemID.TinCan, ItemID.FishingSeaweed
									}));
									item.stack = 1;
								}
							}
							break;
						case 2:
							if (Main.hardMode || Main.rand.NextBool()) {
								InformEveryone("BadLuck.TerribleDay");

								//Set the world to a solar eclipse
								Main.SkipToTime(0, true);

								Main.eclipse = true;
							} else {
								InformEveryone("BadLuck.TerribleNight");

								//Set the world to a blood moon
								Main.SkipToTime(0, false);

								Main.bloodMoon = true;
							}

							//Let everyone know that something wack happened
							if (Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendData(MessageID.WorldData);
							break;
						case 3:
							InformEveryone(WorldEvents.desoMode ? "BadLuck.NoManaDeso" : "BadLuck.NoMana");

							if (WorldEvents.desoMode) {
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

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.BadShopPrices", 10);

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: false, duration: 10 * 60 * 60);
							break;
						case 1:
							InformEveryone("BadLuck.OneNastyDebuff");

							(int, int) debuff = Main.rand.Next(new (int, int)[] {
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
							InformEveryone("BadLuck.OneAnnoyingDebuff");

							debuff = Main.rand.Next(new (int, int)[] {
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
							InformEveryone("BadLuck.NoBuffs");

							for (int i = 0; i < Player.MaxBuffs; i++) {
								if (owner.buffType[i] == 0 || owner.buffTime[i] == 0)
									continue;

								if (!Main.debuff[owner.buffType[i]]) {
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

					if (rollEvent == 0 && owner.SpawnX == Main.spawnTileX && owner.SpawnY == Main.spawnTileY)
						while ((rollEvent = Main.rand.Next(0, 3)) == 0) ;

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.NoSpawnpoint");

							owner.RemoveSpawn();
							break;
						case 1:
							//Teleportation potion effect
							InformEveryone("BadLuck.Teleport");

							if (Main.netMode == NetmodeID.SinglePlayer)
								owner.TeleportationPotion();
							else if (Main.netMode == NetmodeID.MultiplayerClient)
								NetMessage.SendData(MessageID.RequestTeleportationByServer);
							break;
						case 2:
							InformEveryone("BadLuck.ReversedGravity");

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

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.KillTownie");

							List<int> townies = new List<int>();

							for (int i = 0; i < Main.maxNPCs; i++) {
								NPC npc = Main.npc[i];
								if (npc.active && npc.townNPC)
									townies.Add(i);
							}

							if (townies.Count > 0) {
								NPC victim = Main.npc[Main.rand.Next(townies)];

								victim.StrikeNPCNoInteraction(victim.lifeMax * 4, 0f, 0, crit: true);
							}
							break;
						case 1:
							InformEveryone("BadLuck.BadFish");

							owner.GetModPlayer<DicePlayer>().SetFishModifier(detriment: true);
							break;
						case 2:
							InformEveryone("BadLuck.OneAnnoyingDebuff");

							(int, int) debuff = Main.rand.Next(new (int, int)[] {
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
					do {
						rollEvent = Main.rand.Next(0, 3);
					} while (rollEvent == 1 && !WorldEvents.desoMode);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.BadShopPrices", 4);

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: false, 4 * 60 * 60);
							break;
						case 1:
							InformEveryone("BadLuck.NoStamina");

							owner.GetModPlayer<StaminaPlayer>().stamina.ForceExhaustion();
							break;
						case 2:
							InformEveryone("BadLuck.NoMana");

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

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Slapped");

							owner.Hurt(PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.CosmivengeonMod.KillReason.DesoModeInstaKill", owner.name)), 20, 0, Crit: true);

							owner.velocity = new Vector2(0, -8f);
							break;
						case 1:
							InformEveryone("BadLuck.BadFish");

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
					do {
						rollEvent = Main.rand.Next(0, 6);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.1f);

					switch (rollEvent) {
						case 0:
							InformEveryone("Nothing");
							break;
						case 1:
							InformEveryone("Dirt");

							owner.QuickSpawnItem(lootSource, ItemID.DirtBlock, 1);
							break;
						case 2:
							InformEveryone("BadLuck.ManaSickness", 4);

							owner.AddBuff(BuffID.ManaSickness, 4 * 60 * 60);
							break;
						case 3:
							InformEveryone("BadLuck.PotionSickness", 2);

							owner.AddBuff(BuffID.PotionSickness, 2 * 60 * 60);
							owner.potionDelay = 2 * 60;
							break;
						case 4:
							InformEveryone("BadLuck.Frozen");

							owner.AddBuff(BuffID.Frozen, 5 * 60);
							break;
						case 5:
							InformEveryone("BadLuck.Stoned");

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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.25f);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Nothing");
							break;
						case 1:
							owner.QuickSpawnItem(lootSource, ItemID.CopperCoin, 1);

							InformLootDrop(ItemID.CopperCoin, 1, "Bad");
							break;
						case 2:
							owner.QuickSpawnItem(lootSource, ItemID.DirtBlock, 50);

							InformLootDrop(ItemID.DirtBlock, 50, "Bad");
							break;
						case 3:
							int drop = Main.rand.Next(new int[] {
								ItemID.CopperPickaxe,
								ItemID.CopperAxe,
								ItemID.CopperHammer,
								ItemID.CopperShortsword,
								ItemID.CopperBroadsword,
								ItemID.CopperBow
							});

							Item.NewItem(lootSource, owner.Center, drop, 1, prefixGiven: PrefixID.Broken, noGrabDelay: true);

							InformLootDrop(drop, 1, "Bad", PrefixID.Broken);
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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.5f);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLoot.Nothing");
							break;
						case 1:
							owner.QuickSpawnItem(lootSource, ItemID.SilverCoin, 1);

							InformLootDrop(ItemID.SilverCoin, 1);
							break;
						case 2:
							(type, stack) = Main.rand.Next(new (int, int)[] {
								(WorldGen.SavedOreTiers.Copper == TileID.Copper ? ItemID.CopperBar : ItemID.TinBar,
									Main.rand.Next(1, 6)),
								(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(1, 5)),
								(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(1, 4)),
								(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(1, 3))
							});

							Item item = new Item(type, stack);

							owner.QuickSpawnClonedItem(lootSource, item, item.stack);

							InformLootDrop(type, stack);
							break;
						case 3:
							type = Main.rand.Next(new int[] {
								ItemID.Amethyst,
								ItemID.Topaz,
								ItemID.Sapphire
							});

							owner.QuickSpawnItem(lootSource, type, 1);

							InformLootDrop(type, 1);
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
					do {
						rollEvent = Main.rand.Next(0, 3);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.5f);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Nothing");
							break;
						case 1:
							owner.QuickSpawnItem(lootSource, ItemID.SilverCoin, 20);

							InformLootDrop(ItemID.SilverCoin, 20);
							break;
						case 2:
							type = Main.rand.NextFloat() < 0.6f ? ItemID.JourneymanBait : ItemID.ApprenticeBait;
							stack = Main.rand.Next(1, 6);

							owner.QuickSpawnItem(lootSource, type, stack);

							InformLootDrop(type, stack);
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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.25f);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Nothing");
							break;
						case 1:
							(type, stack) = Main.rand.Next(new (int, int)[] {
								(WorldGen.SavedOreTiers.Copper == TileID.Copper ? ItemID.CopperBar : ItemID.TinBar,
									Main.rand.Next(3, 11)),
								(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(2, 8)),
								(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(1, 6)),
								(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(1, 5))
							});

							Item item = new Item(type, stack);

							owner.QuickSpawnClonedItem(lootSource, item, item.stack);

							InformLootDrop(type, stack, "Great");
							break;
						case 2:
							(type, stack) = Main.rand.Next(new (int, int)[] {
								(ItemID.Amethyst, 3),
								(ItemID.Topaz, 2),
								(ItemID.Sapphire, 2),
								(ItemID.Emerald, 1),
								(ItemID.Ruby, 1),
								(ItemID.Diamond, 1)
							});

							item = new Item(type, stack);

							owner.QuickSpawnClonedItem(lootSource, item, stack);

							InformLootDrop(type, stack, "Great");
							break;
						case 3:
							owner.QuickSpawnItem(lootSource, ItemID.GoldCoin, 1);

							InformLootDrop(ItemID.GoldCoin, 1, "Great");
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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while (rollEvent == 0 && Main.rand.NextFloat() >= 0.1f);

					switch (rollEvent) {
						case 0:
							InformEveryone("BadLuck.Nothing");
							break;
						case 1:
							InformEveryone("GoodLuck.StarterKit");

							owner.QuickSpawnItem(lootSource, ItemID.Wood, 100);
							owner.QuickSpawnItem(lootSource, ItemID.StoneBlock, 50);
							owner.QuickSpawnItem(lootSource, ItemID.Gel, 60);
							owner.QuickSpawnItem(lootSource, ItemID.LifeCrystal, 2);
							owner.QuickSpawnItem(lootSource, ItemID.ManaCrystal, 2);
							break;
						case 2:
							InformEveryone("GoodLuck.GoodFish");

							owner.GetModPlayer<DicePlayer>().SetFishModifier(detriment: false, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 3:
							int drop = Main.rand.Next(new int[] {
								ItemID.Aglet,
								ItemID.AnkletoftheWind,
								ItemID.CloudinaBottle,
								ItemID.HermesBoots
							});

							int i = Item.NewItem(lootSource, owner.Center, drop, 1, prefixGiven: -1, noGrabDelay: true);
							Item item = Main.item[i];

							InformLootDrop(drop, 1, "Great", item.prefix);
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
					do {
						rollEvent = Main.rand.Next(0, 5);
					} while ((rollEvent == 3 && !NPC.downedBoss2) || (rollEvent == 4 && !Main.hardMode));

					switch (rollEvent) {
						case 0:
							owner.QuickSpawnItem(lootSource, ItemID.GoldCoin, 5);

							InformLootDrop(ItemID.GoldCoin, 5, "Great");
							break;
						case 1:
							(type, stack) = Main.rand.Next(new (int, int)[] {
								(WorldGen.SavedOreTiers.Iron == TileID.Iron ? ItemID.IronBar : ItemID.LeadBar,
									Main.rand.Next(6, 21)),
								(WorldGen.SavedOreTiers.Silver == TileID.Silver ? ItemID.SilverBar : ItemID.TungstenBar,
									Main.rand.Next(4, 13)),
								(WorldGen.SavedOreTiers.Gold == TileID.Gold ? ItemID.GoldBar : ItemID.PlatinumBar,
									Main.rand.Next(4, 9))
							});

							Item item = new Item(type, stack);

							owner.QuickSpawnClonedItem(lootSource, item, stack);

							InformLootDrop(type, stack, "Great");
							break;
						case 2:
							(type, stack) = Main.rand.Next(new (int, int)[] {
								(ItemID.Amethyst, 10),
								(ItemID.Topaz, 8),
								(ItemID.Sapphire, 7),
								(ItemID.Emerald, 5),
								(ItemID.Ruby, 3),
								(ItemID.Diamond, 1)
							});

							item = new Item(type, stack);

							owner.QuickSpawnClonedItem(lootSource, item, stack);

							InformLootDrop(type, stack, "Great");
							break;
						case 3:
							(type, stack) = (WorldGen.crimson ? ItemID.CrimtaneBar : ItemID.DemoniteBar, Main.rand.Next(8, 16));
							
							owner.QuickSpawnItem(lootSource, type, stack);

							(int type2, int stack2) = (WorldGen.crimson ? ItemID.TissueSample : ItemID.ShadowScale, Main.rand.Next(18, 36));

							owner.QuickSpawnItem(lootSource, type2, stack2);

							InformEveryone("GoodLuck.EvilItems_" + (WorldGen.crimson ? "Crimson" : "Corruption"), stack, stack2);
							break;
						case 4:
							owner.QuickSpawnItem(lootSource, ItemID.HellstoneBar, stack = Main.rand.Next(8, 13));

							InformLootDrop(ItemID.HellstoneBar, stack);
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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while (rollEvent == 2 && noAngler);

					switch (rollEvent) {
						case 0:
							InformEveryone("GoodLuck.GoodShopPrices");
							break;
						case 1:
							int healAmount = (int)(owner.statLifeMax2 * 0.2f);
							
							owner.statLife += healAmount;
							if (owner.statLife > owner.statLifeMax2)
								owner.statLife = owner.statLifeMax2;

							owner.HealEffect(healAmount, broadcast: false);

							NetMessage.SendData(MessageID.SpiritHeal, number: owner.whoAmI, number2: healAmount);  // Heals on other clients

							InformEveryone("GoodLuck.Health");
							break;
						case 2:
							Item quest = new Item(Main.rand.Next(Main.anglerQuestItemNetIDs));

							owner.QuickSpawnItem(lootSource, quest, 1);

							InformLootDrop(quest.type, 1);
							break;
						case 3:
							InformEveryone("GoodLuck.OneNormalBuff");

							(int, int) buff = Main.rand.Next(new (int, int)[] {
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
					do {
						rollEvent = Main.rand.Next(0, 7);
					} while ((rollEvent == 1 && !Main.hardMode)
						|| (rollEvent == 4 && !NPC.downedMechBossAny)
						|| (rollEvent == 6 && !Main.hardMode));

					switch (rollEvent) {
						case 0:
							owner.QuickSpawnItem(lootSource, ItemID.GoldCoin, 20);

							InformLootDrop(ItemID.GoldCoin, 20, "Amazing");
							break;
						case 1:
							int drop = Main.rand.Next(new int[] {
								ItemID.CobaltBar, ItemID.PalladiumBar, ItemID.MythrilBar, ItemID.OrichalcumBar, ItemID.AdamantiteBar, ItemID.TitaniumBar
							});
							int count = Main.rand.Next(1, 11);

							Item item = new Item(drop, count);

							owner.QuickSpawnClonedItem(lootSource, item, count);

							InformLootDrop(drop, count, "Great");
							break;
						case 2:
							if (Main.hardMode) {
								owner.QuickSpawnItem(lootSource, ItemID.LifeFruit, 6);

								InformLootDrop(ItemID.LifeFruit, 6, "Great");
							} else {
								owner.QuickSpawnItem(lootSource, ItemID.LifeCrystal, 10);

								InformLootDrop(ItemID.LifeCrystal, 10, "Great");
							}
							break;
						case 3:
							InformEveryone("GoodLuck.GoodShopPrices");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: true, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 4:
							count = Main.rand.Next(1, 9);

							owner.QuickSpawnItem(lootSource, ItemID.HallowedBar, count);

							InformLootDrop(ItemID.HallowedBar, count, "Great");
							break;
						case 5:
							count = Main.rand.Next(1, 6);

							owner.QuickSpawnItem(lootSource, ItemID.MasterBait, count);

							InformLootDrop(ItemID.MasterBait, count, "Great");
							break;
						case 6:
							owner.QuickSpawnItem(lootSource, ItemID.TruffleWorm, 1);

							InformLootDrop(ItemID.TruffleWorm, 1, "Great");
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
					do {
						rollEvent = Main.rand.Next(0, 4);
					} while ((rollEvent == 2 && !NPC.downedPlantBoss) || (rollEvent == 3 && !NPC.downedAncientCultist) || (rollEvent == 4 && !NPC.downedMoonlord));

					switch (rollEvent) {
						case 0:
							InformEveryone("GoodLuck.OnePowerfulBuff");

							(int, int) buff = Main.rand.Next(new (int, int)[] {
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
							InformEveryone("GoodLuck.LongerIFrames");

							owner.GetModPlayer<DicePlayer>().SetMoreIFrames(5 * 60 * 60);
							break;
						case 2:
							int count = Main.rand.Next(1, 7);

							owner.QuickSpawnItem(lootSource, ItemID.ChlorophyteBar, count);

							InformLootDrop(ItemID.ChlorophyteBar, count, "Great");
							break;
						case 3:
							int[] fragments = new int[] { ItemID.FragmentSolar, ItemID.FragmentNebula, ItemID.FragmentVortex, ItemID.FragmentStardust };
							int[] fragmentCount = new int[4];
							for (int i = 0; i < 4; i++)
								owner.QuickSpawnItem(lootSource, fragments[i], fragmentCount[i] = Main.rand.Next(1, 5));

							InformEveryone("GoodLuck.FragmentLoot", fragmentCount[0], fragmentCount[1], fragmentCount[2], fragmentCount[3]);
							break;
						case 4:
							count = Main.rand.Next(1, 6);

							owner.QuickSpawnItem(lootSource, ItemID.LunarBar, count);

							InformLootDrop(ItemID.LunarBar, count, "Amazing");
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

					switch (rollEvent) {
						case 0:
							owner.QuickSpawnItem(lootSource, ItemID.PlatinumCoin, 1);

							InformLootDrop(ItemID.PlatinumCoin, 1, "Amazing");
							break;
						case 1:
							InformEveryone("GoodLuck.GoodShopPrices");

							owner.GetModPlayer<DicePlayer>().SetShopModifier(good: true, duration: TimeSetter._7_30PM_day + TimeSetter._4_30AM_night);
							break;
						case 2:
							InformEveryone("GoodLuck.InfiniteStamina", 2);

							owner.GetModPlayer<DicePlayer>().SetNSD(2 * 60 * 60);
							break;
						case 3:
							InformEveryone("GoodLuck.LotsaWood");

							int[] types = new int[] {
								ItemID.Wood, ItemID.BorealWood, ItemID.PalmWood, ItemID.Pearlwood, ItemID.RichMahogany, ItemID.Shadewood, ItemID.Ebonwood
							};
							for (int i = 0; i < types.Length; i++)
								owner.QuickSpawnItem(lootSource, types[i], 100);
							break;
						case 4:
							InformEveryone("GoodLuck.BoneKey");

							owner.QuickSpawnItem(lootSource, ItemID.BoneKey, 1);
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

					switch (rollEvent) {
						case 0:
							int healAmount = (int)(owner.statLifeMax2 * 0.5f);
							
							owner.statLife += healAmount;
							if (owner.statLife > owner.statLifeMax2)
								owner.statLife = owner.statLifeMax2;

							owner.HealEffect(healAmount, broadcast: false);

							NetMessage.SendData(MessageID.SpiritHeal, number: owner.whoAmI, number2: healAmount);  // Heals on other clients

							InformEveryone("GoodLuck.HealthMore");
							break;
						case 1:
							InformEveryone("GoodLuck.ExtraLife");

							owner.GetModPlayer<DicePlayer>().extraLives++;
							break;
						case 2:
							InformEveryone("GoodLuck.ManyPowerfulBuffs");

							(int, int)[] buffs = new (int, int)[] {
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
							for (int i = 0; i < 3; i++) {
								(int, int) buff = Main.rand.Next(buffs);
								if (buff.Item1 == -1) {
									owner.GetModPlayer<DicePlayer>().nebulaBuffsTimer = 3 * 60 * 60;
									added += 3;
								} else if (buff.Item1 == -2) {
									owner.GetModPlayer<DicePlayer>().beetleBuffsTimer = 3 * 60 * 60;
									added += 2;
								} else {
									owner.AddBuff(buff.Item1, buff.Item2);
									added++;
								}

								if (added >= 3)
									return;
							}
							break;
						case 3:
							InformEveryone("GoodLuck.NoDebuffs");

							for (int i = 0; i < Player.MaxBuffs; i++) {
								if (Main.debuff[owner.buffType[i]]) {
									owner.DelBuff(i);
									i--;
								}
							}

							break;
						case 4:
							InformEveryone("GoodLuck.NoPotionSickness");

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

					switch (rollEvent) {
						case 0:
							owner.QuickSpawnItem(lootSource, ItemID.PlatinumCoin, 100);

							InformLootDrop(ItemID.PlatinumCoin, 100, "Amazing");
							break;
						case 1:
							InformEveryone("GoodLuck.Iddqd");

							owner.GetModPlayer<DicePlayer>().godmodeTimer = 1 * 60 * 60;
							break;
						case 2:
							InformEveryone("GoodLuck.ExtraLifeThree");

							owner.GetModPlayer<DicePlayer>().extraLives += 3;
							break;
						case 3:
							InformEveryone("GoodLuck.InfiniteStamina", 4);

							owner.GetModPlayer<DicePlayer>().SetNSD(4 * 60 * 60);
							break;
						case 4:
							InformEveryone("GoodLuck.MoreDamage");

							owner.GetModPlayer<DicePlayer>().buffDamageTimer = 2 * 60 * 60;
							break;
						case 5:
							InformEveryone("GoodLuck.InfiniteAmmo");

							owner.GetModPlayer<DicePlayer>().endlessClipTimer = 3 * 60 * 60;
							break;
						case 6:
							InformEveryone("GoodLuck.InfiniteMana");

							owner.GetModPlayer<DicePlayer>().endlessManaTimer = 3 * 60 * 60;
							break;
					}
					break;
					#endregion
			}
		}

		private static bool TryTeleportPlayerToDungeon(out int x, out int y) {
			List<(int, int)> dungeonWalls = new List<(int, int)>();

			bool dungeonLeft = Main.dungeonX < Main.maxTilesX / 2;
			for (y = (int)Main.rockLayer + 100; y < Main.maxTilesY - 200; y++) {
				for (x = dungeonLeft ? 0 : Main.maxTilesX - 1; dungeonLeft ? x < Main.maxTilesX / 3 : x > Main.maxTilesX * 2 / 3; x = dungeonLeft ? x + 1 : x - 1) {
					Tile tile = Framing.GetTileSafely(x, y);
					if (Main.wallDungeon[tile.WallType])
						dungeonWalls.Add((x, y));
				}

				if (dungeonWalls.Count > 0)
					break;
			}

			if (dungeonWalls.Count == 0) {
				x = 0;
				y = 0;
				return false;
			}

			(int, int) coord = dungeonWalls[dungeonWalls.Count / 2];
			x = coord.Item1;
			y = coord.Item2;
			return true;
		}

		private static bool TryTeleportPlayerToBrazil(out int x, out int y) {
			Tile tile;
			int tries = 4000;
			do {
				x = Main.rand.Next(0, Main.maxTilesX);
				y = Main.rand.Next(Main.maxTilesY - 200, Main.maxTilesY);

				tile = Main.tile[x, y];

				tries--;

				if (tries <= 0) {
					x = 0;
					y = 0;
					return false;
				}
			} while (tile.HasTile || !(tile.LiquidType == LiquidID.Lava) || tile.LiquidAmount < 200);

			return true;
		}
	}
}
