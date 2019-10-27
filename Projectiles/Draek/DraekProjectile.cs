using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Poisonous Laser");
			Main.projFrames[projectile.type] = 5;
		}
		
		public override void SetDefaults(){
			projectile.height = 14;
			projectile.width = 14;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 300;
			projectile.aiStyle = 0;
			projectile.alpha = 0;
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit){
			target.AddBuff(BuffID.Poisoned, 600);	//When hit, apply "Poisoned" debuff for 10s
		}

		private bool hasSpawned = false;

		public override void AI(){
			//If the projectile just spawned, set its velocity vector
			if(!hasSpawned){
				hasSpawned = true;

				Vector2 normalized = Vector2.Normalize(new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center);
				Vector2 speedOffset = normalized * Main.rand.NextFloat(-2, 2);

				projectile.velocity = speedOffset + normalized * 12f;
			}
			
			//Set the rotation to the projectile's velocity vector + PI
			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			//Change the animation frame every 5 frames
			if(++projectile.frameCounter >= 5){
				projectile.frameCounter = 0;
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
			}

			//Add a green light from the projectile
			Lighting.AddLight(projectile.Center, 0f, 1f, 0f);

			if(Main.rand.Next(8) == 0)
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 74);
		}
	}
}