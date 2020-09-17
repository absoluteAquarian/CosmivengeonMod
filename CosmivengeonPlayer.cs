using CosmivengeonMod.Buffs.Stamina;
using CosmivengeonMod.Projectiles.Summons;
using CosmivengeonMod.Items.Armor;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameInput;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using CosmivengeonMod.Items.DebugOrTogglers;

namespace CosmivengeonMod{
	public class CosmivengeonPlayer : ModPlayer{
		public List<int> BossesKilled;

		//Core of Desolation check
		public bool hasGottenCore;

		//Debuffs
		public bool primordialWrath;
		public bool rotting;

		//Custom double jump booleans
		public bool doubleJump_JewelOfOronitus;
		public bool dJumpEffect_JewelOfOronitus = false;
		public bool jumpAgain_JewelOfOronitus = false;
		public bool flag_JewelOfOronitus = false;

		//Summon buffs
		public bool babySnek;
		public bool babyProwler;

		//Pet buffs
		public bool cloudPet;

		//Special damage increase thing
		public Stamina stamina;

		//Accessory flags
		public bool equipped_SnowscaleCoat;
		public bool equipped_FrostHorn;
		public bool frostHorn_Broken;
		public bool equipped_EyeOfTheBlizzard;
		public bool abilityActive_EyeOfTheBlizzard;
		public bool DoubleTapUp => player.doubleTapCardinalTimer[1] < 15 && player.controlUp && player.releaseUp;

		//Set bonus flags
		public bool setBonus_Rockserpent;
		public bool setBonus_Crystalice;

		private List<EnergizedParticle> energizedParticles;

		public override void Initialize(){
			stamina = new Stamina(player);
			energizedParticles = new List<EnergizedParticle>();
			BossesKilled = new List<int>();
		}

		public override void ResetEffects(){
			primordialWrath = false;
			rotting = false;
			
			doubleJump_JewelOfOronitus = false;
			
			babySnek = false;
			babyProwler = false;

			cloudPet = false;
			
			equipped_SnowscaleCoat = false;
			equipped_FrostHorn = false;
			equipped_EyeOfTheBlizzard = false;
			
			frostHorn_Broken = false;

			setBonus_Rockserpent = false;
			setBonus_Crystalice = false;

			stamina.Reset();
		}

		public override TagCompound Save() => new TagCompound(){
			["bosses"] = BossesKilled,
			["stamina"] = stamina.GetTagCompound()
		};

		public override void Load(TagCompound tag){
			BossesKilled = tag.GetList<int>("bosses").ToList();
			stamina.ParseCompound(tag.GetCompound("stamina"));
		}

		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath){
			Item item = new Item();
			item.SetDefaults(ModContent.ItemType<CoreOfDesolation>());
			items.Add(item);
		}

		public override void UpdateBadLifeRegen(){
			if(rotting){
				if(player.lifeRegen > 0){
					player.lifeRegen -= 4 * 2;
					if(player.lifeRegen < 0)
						player.lifeRegen = 0;
				}

				player.statDefense -= 10;
				player.endurance -= 0.05f;
			}
			if(primordialWrath){
				if(player.lifeRegen > 0)
					player.lifeRegen = 0;
				player.statDefense -= 10;
				player.endurance -= 0.1f;
				player.lifeRegen -= 15 * 2;
			}
		}

		public override void PostUpdateRunSpeeds(){
			stamina.RunSpeedChange();
		}

		public override void PreUpdate(){
			stamina.FallSpeedDebuff();
		}

		public override void PostUpdateEquips(){
			List<Projectile> crystals = Main.projectile.Where(p => p?.active == true && p.modProjectile is EyeOfTheBlizzardCrystal && p.owner == player.whoAmI).ToList();
			if(!equipped_EyeOfTheBlizzard && crystals.Count >= 1){
				for(int i = 0; i < crystals.Count; i++)
					crystals[i].active = false;
			}

			stamina.Update();
		}

		public override void ProcessTriggers(TriggersSet triggersSet){
			if(CosmivengeonMod.StaminaHotKey.JustPressed && !stamina.Exhaustion && CosmivengeonWorld.desoMode){
				stamina.Active = !stamina.Active;
			}
		}

		public override void clientClone(ModPlayer clientClone){
			CosmivengeonPlayer clone = clientClone as CosmivengeonPlayer;

			clone.stamina.Clone(stamina);
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer){
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)CosmivengeonModMessageType.SyncPlayer);
			packet.Write(player.whoAmI);
			stamina.SendData(packet);
			
			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer){
			CosmivengeonPlayer clone = clientPlayer as CosmivengeonPlayer;

			if(clone.stamina.Value != stamina.Value){
				ModPacket packet = mod.GetPacket();
				packet.Write((byte)CosmivengeonModMessageType.StaminaChanged);
				packet.Write(player.whoAmI);
				stamina.SendData(packet);
				packet.Send();
			}
		}

		public override void UpdateDead(){
			primordialWrath = false;
			doubleJump_JewelOfOronitus = false;
			frostHorn_Broken = false;
			babySnek = false;
			stamina.Active = false;
			abilityActive_EyeOfTheBlizzard = false;
			stamina.ResetValue();
		}

		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit){
			if(equipped_SnowscaleCoat && Main.rand.NextFloat() < 0.075f)
				target.AddBuff(BuffID.Frostburn, 5 * 60);

			OnHitNPCWithAnything(target);
		}

		public override void PostItemCheck(){
			if(player.HeldItem.IsAir || player.HeldItem.damage <= 0 || player.itemAnimation != player.itemAnimationMax - 1)
				return;

			if(setBonus_Rockserpent && Main.rand.NextFloat() < 0.1f){
				Projectile.NewProjectile(player.Center, player.DirectionTo(Main.MouseWorld) * ForsakenOronobladeProjectile.ShootVelocity, ModContent.ProjectileType<ForsakenOronobladeProjectile>(), (int)(player.HeldItem.damage * 0.75f), 5f, player.whoAmI);
				Main.PlaySound(SoundID.Item43.WithVolume(0.5f), player.Center);
			}

			//If the stamina is recharging, punish the player for attacking
			if(stamina.Recharging && stamina.Value < stamina.MaxValue)
				stamina.AddAttackPunishment(1);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit){
			if(equipped_SnowscaleCoat && Main.rand.NextFloat() < 0.075f)
				target.AddBuff(BuffID.Frostburn, 5 * 60);

			OnHitNPCWithAnything(target);
		}

		private void OnHitNPCWithAnything(NPC target){
			if(setBonus_Rockserpent)
				target.AddBuff(BuffID.Poisoned, 8 * 60);
			if(setBonus_Crystalice)
				target.AddBuff(BuffID.Frostburn, 6 * 60);
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			if(player.HasBuff(ModContent.BuffType<Buffs.PrimordialWrath>()))
				damageSource = PlayerDeathReason.ByCustomReason($"{player.name}'s flesh was melted off.");
			return true;
		}

		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit){
			if(equipped_FrostHorn && !frostHorn_Broken && damage >= player.statLifeMax2 * 0.25f){
				player.AddBuff(ModContent.BuffType<Buffs.FrostHornBroken>(), 10 * 60);

				Main.PlaySound(SoundID.Item27, player.Top);
				for(int i = 0; i < 60; i++){
					Dust dust = Dust.NewDustDirect(player.Top - new Vector2(8, 8), 16, 16, 74, Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					Dust dust2 = Dust.NewDustDirect(player.Top - new Vector2(8, 8), 16, 16, 107, Main.rand.NextFloat(-2.5f, 2.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					dust.noGravity = true;
					dust2.noGravity = true;
				}
			}
		}

		public override float UseTimeMultiplier(Item item) => stamina.UseTimeMultiplier() * (abilityActive_EyeOfTheBlizzard ? 1.05f : 1f);

		private int particleTimer = 0;

		public override void DrawEffects(PlayerDrawInfo drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright){
			particleTimer--;

			if(energizedParticles.Count > 0){
				foreach(EnergizedParticle particle in energizedParticles)
					particle.Update();

				int index = 0;
				while(index < energizedParticles.Count){
					if(!energizedParticles[index].Active)
						energizedParticles.RemoveAt(index);
					else
						index++;
				}
			}

			if(drawInfo.shadow != 0f)
				return;

			if(stamina.Active){
				//Draw some dust on the player and make them
				// turn slightly green
				if(particleTimer < 0){
					particleTimer = Main.rand.Next(5, 15 + 1);
					energizedParticles.Add(new EnergizedParticle(
						player,
						new Vector2(Main.rand.NextFloat(-6, player.width + 6), Main.rand.NextFloat(4, player.height - 4)),
						new Vector2(0, -1.5f * 16f / 60f))
					);
				}

				Color currentColor = new Color(r, g, b, a);
				Color newColor = CosmivengeonUtils.Blend(currentColor, Stamina.EnergizedColor);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = newColor.A / 255f;

				Lighting.AddLight(player.Center, (Stamina.EnergizedColor * 0.85f).ToVector3());
			}else if(energizedParticles.Count > 0){
				foreach(EnergizedParticle particle in energizedParticles)
					particle.Delete();

				energizedParticles.Clear();
			}else if(stamina.Exhaustion){
				Color currentColor = new Color(r, g, b, a);
				Color newColor = Color.Gray;
				newColor = CosmivengeonUtils.FadeBetween(currentColor, newColor, 1f - stamina.Value / (float)stamina.MaxValue);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = 1f;
			}

			//Rotting visual effect
			if(rotting){
				Color current = new Color(r, g, b, a);
				Color newColor = Color.DarkGreen;
				newColor = CosmivengeonUtils.FadeBetween(current, newColor, 0.375f);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = 1f;

				if(Main.rand.NextFloat() < 0.18f){
					int index = Dust.NewDust(drawInfo.drawPlayer.position, drawInfo.drawPlayer.width, drawInfo.drawPlayer.height, 18);
					Main.dust[index].velocity = new Vector2(0, Main.rand.NextFloat(1.75f, 3.25f) * 0.2f);
					Main.playerDrawDust.Add(index);
				}
			}
		}

		public static readonly PlayerLayer EnergizedParticles = new PlayerLayer("CosmivengeonMod", "Energized Buff Particles", PlayerLayer.MiscEffectsFront, delegate(PlayerDrawInfo drawInfo){
			if(drawInfo.shadow != 0 || !CosmivengeonWorld.desoMode)
				return;

			CosmivengeonPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<CosmivengeonPlayer>();

			if(modPlayer.stamina.Active){
				foreach(EnergizedParticle particle in modPlayer.energizedParticles){
					if(particle.Active){
						DrawData data = particle.GetDrawData();
						Main.playerDrawData.Add(data);
					}
				}
			}
		});
		
		public int exhaustionAnimationTimer = 0;
		public int shakeTimer = 0;

		public override void ModifyDrawLayers(List<PlayerLayer> layers){
			EnergizedParticles.visible = true;
			layers.Add(EnergizedParticles);
		}
	}
}
