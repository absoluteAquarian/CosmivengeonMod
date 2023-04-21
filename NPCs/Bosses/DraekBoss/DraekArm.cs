using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Bosses.DraekBoss{
	public class DraekArm : ModNPC{
		private bool spawned;
		private Draek Parent;

		public ref float Timer => ref NPC.ai[0];

		public ref float State => ref NPC.ai[1];

		public ref float ShootTimer => ref NPC.ai[2];

		public ref float PunchTarget => ref NPC.ai[3];

		public const int State_Idle = 0;
		public const int State_Prime = 1;
		public const int State_Punch = 2;
		public const int State_Punched = 3;
		public const int State_Return = 4;

		public bool drawAboveBoss;

		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/DraekArm_Punched";

		public override bool CheckActive() => false;

		public override void SetDefaults(){
			NPC.width = 30;
			NPC.height = 30;

			NPC.friendly = false;

			NPC.dontTakeDamage = true;

			NPC.lifeMax = 1;

			NPC.noGravity = true;
			NPC.noTileCollide = true;
		}

		public override void AI(){
			if(!spawned){
				spawned = true;
				Parent = Main.npc[(int)NPC.ai[0]].ModNPC as Draek;

				if(Parent is null || !Parent.NPC.active){
					NPC.active = false;
					return;
				}

				NPC.ai[0] = 0f;
			}

			//kill this NPC if the boss it's attached to has died or despawned
			if(Parent?.NPC.active != true || Parent.NPC.type != ModContent.NPCType<Draek>()){
				NPC.active = false;
				return;
			}

			Timer++;

			NPC.scale = Parent.NPC.scale;
			NPC.defDefense = Parent.NPC.defDefense;
			NPC.lifeMax = Parent.NPC.lifeMax;
			NPC.life = Parent.NPC.lifeMax;
			NPC.realLife = Parent.NPC.whoAmI;
			NPC.immune = Parent.NPC.immune;

			NPC.GetGlobalNPC<StatsNPC>().baseEndurance = Parent.NPC.GetGlobalNPC<StatsNPC>().baseEndurance;
			NPC.GetGlobalNPC<StatsNPC>().endurance = Parent.NPC.GetGlobalNPC<StatsNPC>().endurance;

			Player target = Main.player[(int)PunchTarget];

			if(Parent.AI_Attack != Draek.Attack_Punch)
				NPC.alpha = 255;

			if(State == State_Punched){
				NPC.width = (int)(30 * NPC.scale);
				NPC.height = (int)(30 * NPC.scale);

				NPC.dontTakeDamage = false;
				NPC.alpha = 0;

				//Point towards the target player and shoot blasts of energy towards them
				var direction = NPC.DirectionTo(target.Center);
				NPC.rotation = direction.ToRotation() - MathHelper.Pi;

				if(Timer > 50){
					//Stay a certain distance away from the target player
					const float dist = 10 * 16;
					if(NPC.DistanceSQ(target.Center) < dist * dist)
						NPC.Center = target.Center + NPC.DirectionFrom(target.Center) * dist;
					else{
						NPC.velocity += direction * MiscUtils.GetModeChoice(10f, 15f, 20f) / 60f;

						if(Math.Sign(direction.X) != Math.Sign(NPC.velocity.X))
							NPC.velocity.X *= 1f - 8f / 60f;
						if(Math.Sign(direction.Y) != Math.Sign(NPC.velocity.Y))
							NPC.velocity.Y *= 1f - 8f / 60f;
					}

					const float velCap = 20f;
					if(NPC.velocity.LengthSquared() > velCap * velCap)
						NPC.velocity = Vector2.Normalize(NPC.velocity) * velCap;

					ShootTimer++;

					if(ShootTimer > MiscUtils.GetModeChoice(2f, 1.25f, 0.75f) * 60){
						ShootTimer = 0;

						//Recoil
						NPC.velocity += -direction * 7f;

						Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 4f * 16;

						MiscUtils.SpawnProjectileSynced(NPC.Bottom - new Vector2(0, 0.667f * NPC.height),
							Vector2.Zero,
							ModContent.ProjectileType<DraekLaser>(),
							NPC.damage,
							6f,
							target.Center.X + positionOffset.X,
							target.Center.Y + positionOffset.Y
						);

						//Play "boss laser" sound effect
						SoundEngine.PlaySound(SoundID.Item33, NPC.Center);
					}
				}
			}else if(State == State_Return && NPC.alpha < 255){
				//Not used
				//Draek just "kills" the old arms and creates new ones
				NPC.dontTakeDamage = true;
				NPC.alpha = 255;

				SoundEngine.PlaySound(SoundID.Tink, NPC.Center);

				//Spawn gores
				Vector2 dir = NPC.rotation.ToRotationVector2();
				Vector2 spawn = NPC.Center;
				int gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, Mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
				spawn += dir * 24;
				gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, Mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
				spawn += dir * 24;
				gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, Mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);

				State = State_Idle;
			}else if(NPC.alpha >= 255){
				//Hide the NPC; it doesn't need to be drawn anymore
				NPC.dontTakeDamage = true;
				NPC.alpha = 255;

				NPC.rotation = 0;

				NPC.velocity = Vector2.Zero;
				NPC.Center = Parent.NPC.Center;
			}

			if(NPC.rotation > MathHelper.PiOver2 || NPC.rotation < -MathHelper.PiOver2)
				NPC.spriteDirection = -1;
			else
				NPC.spriteDirection = 1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor){
			//Draek draws the arms if they're not punched
			if(State != State_Punched && State != State_Return)
				return false;

			if(NPC.alpha == 255)
				return false;

			Texture2D texture = Mod.GetTexture("NPCs/Draek/DraekArm_Punched");
			Vector2 drawOrigin = new Vector2(12, 16);
			SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, null, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
			
			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
			=> Parent?.OnHitPlayer(target, damage, crit);

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

		//Thanks jopojelly for helping me figure out how to make the Parent-Child link work for the two hooks below:
		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit){
			if(State == State_Punched)
				return;

			if(Parent != null)
				Parent.NPC.immune[player.whoAmI] = player.itemAnimation;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			if(State == State_Punched)
				return;

			if(Parent != null){
				Parent.NPC.immune[projectile.owner] = NPC.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[Parent.NPC.whoAmI] = projectile.localNPCHitCooldown;
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => !NPC.dontTakeDamage;
	}
}
