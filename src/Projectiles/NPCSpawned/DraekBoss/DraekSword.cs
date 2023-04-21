using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss {
	public class DraekSword : ModProjectile {
		public int DesoDebuffTime = 2 * 60;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Forsaken Oronoblade");
		}
		public override void SetDefaults() {
			Projectile.height = 30;
			Projectile.width = 30;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2 * 60;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;

			DrawOffsetX = -44;
			DrawOriginOffsetY = -52;
		}

		private bool hasSpawned = false;

		private int npcOwner = -1;

		public ref float Phase => ref Projectile.ai[1];

		private float expert_revolutionAngle;

		private Vector2 expert_revolutionOffset;

		private int timer;

		private int extra, extra2;

		private Player target;

		public override void AI() {
			//If the projectile just spawned, spawn the "extra" hitboxes
			//Also set the velocity
			if (!hasSpawned) {
				hasSpawned = true;

				target = Main.player[(int)Projectile.ai[0]];
				npcOwner = (int)Projectile.ai[1];

				//These two will be used later for swinging at the target player in Desolation Mode
				Projectile.ai[0] = 0f;
				Projectile.ai[1] = 0f;

				//All AI types have the same initial movement
				Projectile.velocity = Vector2.Normalize(target.Center - Projectile.Center) * 20f;

				extra = Projectile.NewProjectile(Projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, Projectile.whoAmI, -1);
				extra2 = Projectile.NewProjectile(Projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), Projectile.damage, Projectile.knockBack, Main.myPlayer, Projectile.whoAmI, 1);

				if (extra == Main.maxProjectiles || extra2 == Main.maxProjectiles) {
					Projectile.Kill();
					return;
				}
			}

			NPC parent = Main.npc[npcOwner];

			//Check if the NPC has died or if it's not the boss
			if (!parent.active || parent.type != ModContent.NPCType<Draek>() || (parent.ModNPC as Draek).AI_Attack == Draek.Attack_Retrieve_Sword) {
				Projectile.Kill();
				return;
			}

			//Normal mode AI just flies in a straight line
			if (!Main.expertMode && !WorldEvents.desoMode) {
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				return;
			}

			//Both mode AIs start out the same
			if (Phase == 0) {
				if (timer == 26) {
					Phase = 1;
					timer = -1;
				}

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}

			if (!WorldEvents.desoMode) {
				//Expert mode: sword flies forward a bit, then it spins around draek for a few seconds.
				//Then it points at the player and yeets itself toward the player
				if (Phase > 0 && Phase < 3)
					expert_revolutionAngle += MathHelper.ToRadians(6f * 1.65f);  //1.65 revolutions per second

				expert_revolutionOffset = Vector2.UnitX.RotatedBy(expert_revolutionAngle) * (parent.height + 5 * 16);

				Vector2 dirFromOwner = Projectile.DirectionFrom(parent.Center);

				if (Phase == 1) {
					//Trying to get back in range of the boss
					Projectile.velocity += -dirFromOwner * 13f / 60f;

					if (Math.Sign(-dirFromOwner.X) != Math.Sign(Projectile.velocity.X))
						Projectile.velocity.X *= 1f - 12f / 60f;
					if (Math.Sign(-dirFromOwner.Y) != Math.Sign(Projectile.velocity.Y))
						Projectile.velocity.Y *= 1f - 12f / 60f;

					SpawnMoveDust(0.19f);

					if (Projectile.WithinDistance(parent.Center, parent.height + 5 * 16)) {
						Phase = 2;
						timer = -1;

						expert_revolutionAngle = Projectile.DirectionFrom(parent.Center).ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-30, 30));
					}
				} else if (Phase == 2) {
					//Revolving around the parent NPC
					Projectile.Center = parent.Center + expert_revolutionOffset;

					if (timer > 3f * 60) {
						Phase = 3;
						timer = -1;
					}

					SpawnMoveDust(0.3f);
				} else if (Phase == 3) {
					if (timer > 25) {
						Phase = 4;
						Projectile.velocity = Projectile.DirectionTo(target.Center) * 20f;
					}
				}

				if (Phase < 4)
					Projectile.timeLeft = 2 * 60;

				if (Phase > 0 && Phase < 3)
					Projectile.rotation = dirFromOwner.ToRotation() + MathHelper.PiOver2;
				else if (Phase == 3) {
					float targetRotation = Projectile.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;

					//Ease the targetting rotation
					Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.25f);
				} else
					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			} else {
				//Desolation Mode: sword acts like the Terraprisma summon, but with a twist
				Projectile.timeLeft = 60;

				if (!target.WithinDistance(parent.Center, 40 * 16) && Phase < 2)
					Phase = -1;
				else if (Phase < 2 && timer >= 0) {
					Phase = 2;

					timer = 0;
				}

				if (Phase == -1) {
					//Try to stay behind the boss
					Vector2 pos = parent.Bottom - new Vector2(0, 0.667f * parent.height);
					pos += parent.spriteDirection == 1 ? new Vector2(0, 150) : new Vector2(0, -150);

					//Only try to change movement if the projectile is far enough away
					if (!Projectile.WithinDistance(pos, 2 * 16)) {
						Vector2 dir = Projectile.DirectionTo(pos);

						if (Math.Sign(Projectile.velocity.X) != Math.Sign(dir.X))
							Projectile.velocity.X *= 1f - 5f / 60f;
						if (Math.Sign(Projectile.velocity.Y) != Math.Sign(dir.Y))
							Projectile.velocity.Y *= 1f - 5f / 60f;

						Projectile.velocity += dir * 12f / 60f;

						const float velCap = 18.5f;
						if (Projectile.velocity.LengthSquared() > velCap * velCap)
							Projectile.velocity = Vector2.Normalize(Projectile.velocity) * velCap;
					}

					float factorX = Projectile.velocity.X / 12f;
					factorX.Clamp(-1, 1);
					Projectile.rotation = MathHelper.Pi + MathHelper.ToRadians(30 * factorX);

					//If any swords are nearby, move both away from each other
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile proj = Main.projectile[i];
						const float dist = 4 * 16;
						if (i != Projectile.whoAmI && proj.active && proj.type == Projectile.type && Projectile.DistanceSQ(proj.Center) < dist * dist) {
							Vector2 dir = Projectile.DirectionTo(proj.Center);
							Vector2 midpoint = Projectile.Center + (proj.Center - Projectile.Center) / 2;

							Projectile.Center = midpoint + -dir * dist / 2;
							proj.Center = midpoint + dir * dist / 2;
						}
					}

					SpawnMoveDust(0.3f);
				} else if (Phase == 2) {
					//Player is too close!  Stab them
					var dir = Projectile.DirectionTo(target.Center);
					Projectile.rotation = MathHelper.PiOver2 + MathHelper.Lerp(Projectile.rotation, dir.ToRotation() + MathHelper.PiOver2, 0.25f);

					Projectile.velocity = Vector2.Zero;

					if (timer == 45) {
						timer = -1;
						Phase = 3;

						Projectile.velocity = dir * 12f;
					}
				} else if (Phase == 3) {
					SpawnMoveDust(0.3f);

					Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

					if (timer == 80) {
						Phase = -1;

						timer = -151;
					}
				}
			}

			timer++;
		}

		private void SpawnMoveDust(float chance) {
			Projectile projExtra = Main.projectile[extra];
			Projectile projExtra2 = Main.projectile[extra2];

			Vector2 start = projExtra.Center;
			Vector2 end = projExtra2.Center;

			int count = 20;
			Vector2 step = (end - start) / count;

			for (int i = 0; i < count; i++) {
				Vector2 offset = start + step * i;
				SpawnDustInner(Projectile.Center + offset, new Vector2(4), chance);
			}
		}

		private void SpawnDustInner(Vector2 position, Vector2 box, float chance) {
			if (Main.rand.NextFloat() < chance) {
				Dust dust = Dust.NewDustDirect(position - box / 2f, (int)box.X, (int)box.Y, 74);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			//Only apply the Primordial Wrath debuff if the world is in Desolation mode
			if (WorldEvents.desoMode) {
				//Apply a shorter debuff time while not spinning
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), DesoDebuffTime);
			}
		}
	}
}
