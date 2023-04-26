using CosmivengeonMod.Abilities;
using CosmivengeonMod.Enums;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ID;

namespace CosmivengeonMod.Players {
	public class StaminaPlayer : ModPlayer {
		public Stamina stamina;

		internal List<EnergizedParticle> energizedParticles;

		public override void Initialize() {
			stamina = new Stamina();
			energizedParticles = new List<EnergizedParticle>();
		}

		public override void ResetEffects() {
			stamina.Reset();
		}

		public override void SaveData(TagCompound tag) {
			tag["stamina"] = stamina.GetTagCompound();
		}

		public override void LoadData(TagCompound tag) {
			stamina.ParseCompound(tag.GetCompound("stamina"));
		}

		public override void PostUpdateRunSpeeds() {
			stamina.ApplyRunSpeedChanges(Player);
		}

		public override void PreUpdate() {
			stamina.ApplyFallSpeedDebuff(Player);
		}

		public override void PostUpdateEquips() {
			stamina.Update(Player);
		}

		public override void PostUpdateMiscEffects() {
			stamina.ApplyAttackSpeed(Player);
		}

		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (CoreMod.StaminaHotKey.JustPressed && !stamina.Exhaustion && WorldEvents.desoMode) {
				stamina.Active = !stamina.Active;
			}
		}

		public override void clientClone(ModPlayer clientClone) {
			StaminaPlayer clone = clientClone as StaminaPlayer;

			clone.stamina.Clone(stamina);
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			ModPacket packet = Mod.GetPacket();
			packet.Write((byte)MessageType.SyncPlayer);
			packet.Write(Player.whoAmI);
			stamina.SendData(packet);

			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer) {
			StaminaPlayer clone = clientPlayer as StaminaPlayer;

			if (clone.stamina.Value != stamina.Value) {
				ModPacket packet = Mod.GetPacket();
				packet.Write((byte)MessageType.StaminaChanged);
				packet.Write(Player.whoAmI);
				stamina.SendData(packet);
				packet.Send();
			}
		}

		public override void OnEnterWorld(Player player) {
			Main.NewText("Thank you for using Cosmivengeon Mod! Please know that this mod is not responsible for any issues that might occur when using Desolation Mode with any other mod's difficulty mode.", new Color(0xa6, 0x00, 0xcd));
		}

		private int particleTimer = 0;

		private int lastUpdate = -1;

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
			if (lastUpdate != Main.GameUpdateCount)
				particleTimer--;

			//Only update the particles once per tick
			if (energizedParticles.Count > 0 && lastUpdate != Main.GameUpdateCount) {
				foreach (EnergizedParticle particle in energizedParticles)
					particle?.Update();

				int index = 0;
				while (index < energizedParticles.Count) {
					if (energizedParticles[index] is { Active: false })
						energizedParticles[index] = null;
					else
						index++;
				}
			}

			if (drawInfo.shadow != 0f)
				return;

			if (stamina.Active) {
				//Draw some dust on the player and make them
				// turn slightly green
				if (particleTimer < 0 && lastUpdate != Main.GameUpdateCount) {
					particleTimer = Main.rand.Next(5, 15 + 1);
					
					var particle = new EnergizedParticle(
						Player,
						new Vector2(Main.rand.NextFloat(-6, Player.width + 6), Main.rand.NextFloat(4, Player.height - 4)),
						new Vector2(0, -1.5f * 16f / 60f));

					int index = -1;
					for (int i = 0; i < energizedParticles.Count; i++) {
						if (energizedParticles[i] is null) {
							energizedParticles[i] = particle;
							index = i;
							break;
						}
					}

					if (index == -1)
						energizedParticles.Add(particle);
				}

				var color = stamina.EnergizedColor;

				Color currentColor = new Color(r, g, b, a);
				Color newColor = MiscUtils.Blend(currentColor, color);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = newColor.A / 255f;

				Lighting.AddLight(Player.Center, (color * 0.85f).ToVector3());
			} else if (energizedParticles.Count > 0) {
				// Destroy all active particles
				for (int i = 0; i < energizedParticles.Count; i++) {
					energizedParticles[i]?.Delete();
					energizedParticles[i] = null;
				}
			} else if (stamina.Exhaustion) {
				Color currentColor = new Color(r, g, b, a);
				Color newColor = Color.Gray;
				newColor = MiscUtils.FadeBetween(currentColor, newColor, 1f - stamina.Value / (float)stamina.MaxValue);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = 1f;
			}

			//Rotting visual effect
			//Normally I'd put this in BuffPlayer, but i need this to override the above code if it should apply and i can't be arsed to see
			//  on what order the DrawEffects hooks are called on the ModPlayer;
			if (drawInfo.drawPlayer.GetModPlayer<BuffPlayer>().rotting) {
				Color current = new Color(r, g, b, a);
				Color newColor = Color.DarkGreen;
				newColor = MiscUtils.FadeBetween(current, newColor, 0.375f);
				r = newColor.R / 255f;
				g = newColor.G / 255f;
				b = newColor.B / 255f;
				a = 1f;

				if (Main.rand.NextFloat() < 0.18f) {
					int index = Dust.NewDust(drawInfo.drawPlayer.position, drawInfo.drawPlayer.width, drawInfo.drawPlayer.height, DustID.CorruptGibs);
					Main.dust[index].velocity = new Vector2(0, Main.rand.NextFloat(1.75f, 3.25f) * 0.2f);
					drawInfo.DustCache.Add(index);
				}
			}

			//This is used to ensure stuff only updates once per tick
			lastUpdate = (int)Main.GameUpdateCount;
		}
	}

	internal class EnergizedParticlesLayer : PlayerDrawLayer {
		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.BeetleBuff);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			if (drawInfo.shadow != 0 || !WorldEvents.desoMode)
				return;

			StaminaPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<StaminaPlayer>();

			if (modPlayer.stamina.Active) {
				foreach (EnergizedParticle particle in modPlayer.energizedParticles) {
					if (particle.Active) {
						DrawData data = particle.GetDrawData();
						drawInfo.DrawDataCache.Add(data);
					}
				}
			}
		}
	}
}
