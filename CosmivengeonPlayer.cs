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

namespace CosmivengeonMod{
	public class CosmivengeonPlayer : ModPlayer{
		public List<int> BossesKilled;

		//Debuffs
		public bool primordialWrath;

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

		public override void UpdateBadLifeRegen(){
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
		
		private int exhaustionAnimationTimer = 0;
		private int shakeTimer = 0;

		private const int ExhaustionAnimation1 = 15;
		private const int ExhaustionAnimation2 = ExhaustionAnimation1 + 30;
		private const int ExhaustionAnimation3 = ExhaustionAnimation2 + 15;
		private const int ExhaustionAnimation4 = ExhaustionAnimation3 + 30;
		private const int ExhaustionAnimation5 = ExhaustionAnimation4 + 15;
		private const int ExhaustionAnimation6 = ExhaustionAnimation5 + 30;
		private const int ExhaustionAnimation7 = ExhaustionAnimation6 + 15;
		private const int ExhaustionAnimation8 = ExhaustionAnimation7 + 20;
		private const int ExhaustionAnimation9 = ExhaustionAnimation8 + 20;
		private const int ExhaustionAnimation10 = ExhaustionAnimation9 + 20;
		private const int ExhaustionAnimation11 = ExhaustionAnimation10 + 20;

		public static readonly PlayerLayer ExhaustionAnimation = new PlayerLayer("CosmivengeonMod", "Exhaustion Animation", PlayerLayer.MiscEffectsFront, delegate(PlayerDrawInfo drawInfo){
			if(drawInfo.shadow != 0 || !CosmivengeonWorld.desoMode)
				return;

			CosmivengeonPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<CosmivengeonPlayer>();

			if(modPlayer.stamina.Exhaustion){
				DrawData data;
				Vector2 shakeOffset = Vector2.Zero;
				int frame = 0;
				float colorMult = 1f;
				
				if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation1)
					frame = 0;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation2){
					modPlayer.shakeTimer++;
					frame = 1;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation2 - 10)
						shakeOffset = new Vector2(5f * CosmivengeonUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation3){
					modPlayer.shakeTimer = 0;
					frame = 1;
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation4){
					modPlayer.shakeTimer++;
					frame = 2;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation4 - 10)
						shakeOffset = new Vector2(5f * CosmivengeonUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation5){
					modPlayer.shakeTimer = 0;
					frame = 2;
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation6){
					modPlayer.shakeTimer++;
					frame = 3;

					if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation6 - 10)
						shakeOffset = new Vector2(5f * CosmivengeonUtils.fSin(modPlayer.shakeTimer / 5f * MathHelper.TwoPi), 0f);
				}else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation7)
					frame = 3;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation8)
					frame = 4;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation9)
					frame = 5;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation10)
					frame = 6;
				else if(modPlayer.exhaustionAnimationTimer < ExhaustionAnimation11)
					frame = 7;
				else
					colorMult = 0f;

				Texture2D animationTexture = ModContent.GetTexture("CosmivengeonMod/Buffs/Stamina/ExhaustionAnimation");

				data = new DrawData(
					animationTexture,
					modPlayer.player.position - new Vector2(10, 60) - Main.screenPosition + shakeOffset,
					new Rectangle(0, frame * animationTexture.Height / 8, animationTexture.Width, animationTexture.Height / 8),
					Color.White * colorMult,
					0f,
					Vector2.Zero,
					1f,
					SpriteEffects.None,
					0
				);

				Main.playerDrawData.Add(data);
				modPlayer.exhaustionAnimationTimer++;
			}else
				modPlayer.exhaustionAnimationTimer = 0;
		});

		public override void ModifyDrawLayers(List<PlayerLayer> layers){
			EnergizedParticles.visible = true;
			layers.Add(EnergizedParticles);
			ExhaustionAnimation.visible = true;
			layers.Add(ExhaustionAnimation);
		}
	}
}
