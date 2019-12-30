﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekRockExplosion : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Draek/DraekRock";

		public override void SetStaticDefaults(){
			Main.projFrames[projectile.type] = 3;
		}
		
		public override void SetDefaults(){
			projectile.height = 8;
			projectile.width = 8;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 3 * 60;
			projectile.aiStyle = 0;
			projectile.alpha = 0;
			projectile.scale = 1f;
		}

		private bool frameChosen = false;

		public override void AI(){
			projectile.velocity.X += projectile.ai[0];
			projectile.velocity.Y += projectile.ai[1];

			projectile.rotation += MathHelper.ToRadians(3f * 360f / 60f) * ((projectile.velocity.X > 0) ? 1 : -1);
			
			//Choose a random frame to use when spawning this projectile
			if(!frameChosen){
				frameChosen = true;
				projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = Main.projectileTexture[projectile.type];
			Rectangle frame = texture.Frame(1, 3, 0, projectile.frame);

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, frame, lightColor, projectile.rotation, frame.Size() / 2f, projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}