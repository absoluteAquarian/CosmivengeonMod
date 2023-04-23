using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek {
	public class ForsakenOronobladeProjectile : ModProjectile {
		public override string Texture => "CosmivengeonMod/Projectiles/NPCSpawned/DraekBoss/DraekLaser";

		public static readonly float ShootVelocity = 8.21f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Poisonous Energy Blast");
			Main.projFrames[Projectile.type] = 5;
		}

		public override void SetDefaults() {
			Projectile.height = 14;
			Projectile.width = 14;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 2 * 60;
			Projectile.alpha = 255;
			Projectile.scale = 0.8f;

			Projectile.DamageType = DamageClass.Melee;

			Projectile.aiStyle = 0;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (Main.rand.NextFloat() <= 0.16667)
				target.AddBuff(BuffID.Poisoned, 5 * 60);
		}

		public override void AI() {
			//Fade the projectile in
			if (Projectile.alpha > 0)
				Projectile.alpha -= 25;
			else if (Projectile.alpha < 0)
				Projectile.alpha = 0;

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
				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GreenFairy);
				dust.noGravity = true;
				dust.velocity = Vector2.Zero;
			}
		}
	}
}
