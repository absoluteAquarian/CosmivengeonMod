using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss {
	public class DraekLaser : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Poisonous Laser");
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults() {
			Projectile.height = 14;
			Projectile.width = 14;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			target.AddBuff(BuffID.Poisoned, 600);   //When hit, apply "Poisoned" debuff for 10s
		}

		private bool hasSpawned = false;

		public override void AI() {
			//If the projectile just spawned, set its velocity vector
			if (!hasSpawned) {
				hasSpawned = true;

				Vector2 normalized = Vector2.Normalize(new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center);
				Vector2 speedOffset = normalized * Main.rand.NextFloat(-2, 2);

				Projectile.velocity = speedOffset + normalized * 12f;
			}

			//Set the rotation to the projectile's velocity vector + PI
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			//Change the animation frame every 5 frames
			if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
			}

			//Add a green light from the projectile
			Lighting.AddLight(Projectile.Center, 0f, 1f, 0f);

			if (Main.rand.NextFloat() < 0.125f) {
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 74);
				dust.velocity = Projectile.velocity * 0.3f;
			}
		}
	}
}
