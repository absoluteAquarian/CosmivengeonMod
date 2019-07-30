﻿using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekRock : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rock");
			Main.projFrames[projectile.type] = 3;
		}
		
		public override void SetDefaults(){
			projectile.height = 15;
			projectile.width = 15;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = true;
			projectile.ignoreWater = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 300;
			projectile.aiStyle = 0;
			projectile.alpha = 0;
			projectile.scale = 2f;

			drawOriginOffsetX = -projectile.width / 2f;
			drawOriginOffsetY = (int)(projectile.height / 2f);
		}

		public override void AI(){
			if(projectile.velocity.Y < projectile.ai[0])
				projectile.velocity.Y += projectile.ai[1];
			
			//Change the animation frame every 6 frames
			if(++projectile.frameCounter >= 6){
				projectile.frameCounter = 0;
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
			}
		}
	}
}
