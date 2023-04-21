using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss{
	public class DraekRockExplosion : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/NPCSpawned/DraekBoss/DraekRock";

		public override void SetStaticDefaults(){
			Main.projFrames[Projectile.type] = 3;
		}
		
		public override void SetDefaults(){
			Projectile.height = 8;
			Projectile.width = 8;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 3 * 60;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.scale = 1f;
		}

		private bool frameChosen = false;

		public override void AI(){
			Projectile.velocity.X += Projectile.ai[0];
			Projectile.velocity.Y += Projectile.ai[1];

			Projectile.rotation += MathHelper.ToRadians(3f * 360f / 60f) * ((Projectile.velocity.X > 0) ? 1 : -1);
			
			//Choose a random frame to use when spawning this projectile
			if(!frameChosen){
				frameChosen = true;
				Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
			}
		}

		public override bool PreDraw(ref Color lightColor){
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Rectangle frame = texture.Frame(1, 3, 0, Projectile.frame);

			spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}
}
