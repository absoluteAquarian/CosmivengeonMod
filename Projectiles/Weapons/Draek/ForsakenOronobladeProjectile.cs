using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class ForsakenOronobladeProjectile : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Draek/DraekLaser";

		public static readonly float ShootVelocity = 9f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Poisonous Energy Blast");
			Main.projFrames[projectile.type] = 5;
		}

		public override void SetDefaults(){
			projectile.height = 14;
			projectile.width = 14;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.tileCollide = true;
			projectile.ignoreWater = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 2 * 60;
			projectile.alpha = 255;
			projectile.scale = 0.8f;

			projectile.melee = true;

			projectile.aiStyle = 0;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			if(Main.rand.NextFloat() <= 0.16667)
				target.AddBuff(BuffID.Poisoned, 5 * 60);
		}

		public override void AI(){
			//Fade the projectile in
			if(projectile.alpha > 0)
				projectile.alpha -= 25;
			else if(projectile.alpha < 0)
				projectile.alpha = 0;

			//Set the rotation to the projectile's velocity vector + PI
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			//Change the animation frame every 5 frames
			if (++projectile.frameCounter >= 5){
				projectile.frameCounter = 0;
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
			}

			//Add a green light from the projectile
			Lighting.AddLight(projectile.Center, 0f, 1f, 0f);

			if(Main.rand.NextFloat() < 0.125f){
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 74);
				dust.noGravity = true;
				dust.velocity = Vector2.Zero;
			}
		}
	}
}
