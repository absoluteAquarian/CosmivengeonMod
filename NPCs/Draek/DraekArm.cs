using CosmivengeonMod.Projectiles.Draek;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public class DraekArm : ModNPC{
		private bool spawned;
		private Draek Parent;

		public ref float Timer => ref npc.ai[0];

		public ref float State => ref npc.ai[1];

		public ref float ShootTimer => ref npc.ai[2];

		public ref float PunchTarget => ref npc.ai[3];

		public const int State_Idle = 0;
		public const int State_Prime = 1;
		public const int State_Punch = 2;
		public const int State_Punched = 3;
		public const int State_Return = 4;

		public bool drawAboveBoss;

		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekArm_Punched";

		public override bool CheckActive() => false;

		public override void SetDefaults(){
			npc.width = 30;
			npc.height = 30;

			npc.friendly = false;

			npc.dontTakeDamage = true;

			npc.lifeMax = 1;

			npc.noGravity = true;
			npc.noTileCollide = true;
		}

		public override void AI(){
			if(!spawned){
				spawned = true;
				Parent = Main.npc[(int)npc.ai[0]].modNPC as Draek;

				if(Parent is null || !Parent.npc.active){
					npc.active = false;
					return;
				}

				npc.ai[0] = 0f;
			}

			//kill this NPC if the boss it's attached to has died or despawned
			if(Parent?.npc.active != true || Parent.npc.type != ModContent.NPCType<Draek>()){
				npc.active = false;
				return;
			}

			Timer++;

			npc.scale = Parent.npc.scale;
			npc.defDefense = Parent.npc.defDefense;
			npc.lifeMax = Parent.npc.lifeMax;
			npc.life = Parent.npc.lifeMax;
			npc.realLife = Parent.npc.whoAmI;
			npc.immune = Parent.npc.immune;

			npc.GetGlobalNPC<CosmivengeonGlobalNPC>().baseEndurance = Parent.npc.GetGlobalNPC<CosmivengeonGlobalNPC>().baseEndurance;
			npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance = Parent.npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance;

			Player target = Main.player[(int)PunchTarget];

			if(Parent.AI_Attack != Draek.Attack_Punch)
				npc.alpha = 255;

			if(State == State_Punched){
				npc.width = (int)(30 * npc.scale);
				npc.height = (int)(30 * npc.scale);

				npc.dontTakeDamage = false;
				npc.alpha = 0;

				//Point towards the target player and shoot blasts of energy towards them
				var direction = npc.DirectionTo(target.Center);
				npc.rotation = direction.ToRotation() - MathHelper.Pi;

				if(Timer > 50){
					//Stay a certain distance away from the target player
					const float dist = 10 * 16;
					if(npc.DistanceSQ(target.Center) < dist * dist)
						npc.Center = target.Center + npc.DirectionFrom(target.Center) * dist;
					else{
						npc.velocity += direction * CosmivengeonUtils.GetModeChoice(10f, 15f, 20f) / 60f;

						if(Math.Sign(direction.X) != Math.Sign(npc.velocity.X))
							npc.velocity.X *= 1f - 8f / 60f;
						if(Math.Sign(direction.Y) != Math.Sign(npc.velocity.Y))
							npc.velocity.Y *= 1f - 8f / 60f;
					}

					const float velCap = 20f;
					if(npc.velocity.LengthSquared() > velCap * velCap)
						npc.velocity = Vector2.Normalize(npc.velocity) * velCap;

					ShootTimer++;

					if(ShootTimer > CosmivengeonUtils.GetModeChoice(2f, 1.25f, 0.75f) * 60){
						ShootTimer = 0;

						//Recoil
						npc.velocity += -direction * 7f;

						Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 4f * 16;

						CosmivengeonUtils.SpawnProjectileSynced(npc.Bottom - new Vector2(0, 0.667f * npc.height),
							Vector2.Zero,
							ModContent.ProjectileType<DraekLaser>(),
							npc.damage,
							6f,
							target.Center.X + positionOffset.X,
							target.Center.Y + positionOffset.Y
						);

						//Play "boss laser" sound effect
						Main.PlaySound(SoundID.Item33, npc.Center);
					}
				}
			}else if(State == State_Return && npc.alpha < 255){
				//Not used
				//Draek just "kills" the old arms and creates new ones
				npc.dontTakeDamage = true;
				npc.alpha = 255;

				Main.PlaySound(SoundID.Tink, npc.Center);

				//Spawn gores
				Vector2 dir = npc.rotation.ToRotationVector2();
				Vector2 spawn = npc.Center;
				int gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
				spawn += dir * 24;
				gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
				spawn += dir * 24;
				gore = Gore.NewGore(spawn, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30)) * -5f, mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);

				State = State_Idle;
			}else if(npc.alpha >= 255){
				//Hide the NPC; it doesn't need to be drawn anymore
				npc.dontTakeDamage = true;
				npc.alpha = 255;

				npc.rotation = 0;

				npc.velocity = Vector2.Zero;
				npc.Center = Parent.npc.Center;
			}

			if(npc.rotation > MathHelper.PiOver2 || npc.rotation < -MathHelper.PiOver2)
				npc.spriteDirection = -1;
			else
				npc.spriteDirection = 1;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor){
			//Draek draws the arms if they're not punched
			if(State != State_Punched && State != State_Return)
				return false;

			if(npc.alpha == 255)
				return false;

			Texture2D texture = mod.GetTexture("NPCs/Draek/DraekArm_Punched");
			Vector2 drawOrigin = new Vector2(12, 16);
			SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;

			spriteBatch.Draw(texture, npc.Center - Main.screenPosition, null, drawColor, npc.rotation, drawOrigin, npc.scale, effects, 0);
			
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
				Parent.npc.immune[player.whoAmI] = player.itemAnimation;
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			if(State == State_Punched)
				return;

			if(Parent != null){
				Parent.npc.immune[projectile.owner] = npc.immune[projectile.owner];

				if(projectile.usesLocalNPCImmunity)
					projectile.localNPCImmunity[Parent.npc.whoAmI] = projectile.localNPCHitCooldown;
			}
		}

		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => !npc.dontTakeDamage;
	}
}
