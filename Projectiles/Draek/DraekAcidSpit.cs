using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekAcidSpit : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Draek/DraekLaser";

		public override void SetDefaults(){
			projectile.alpha = 255;
			projectile.width = 8;
			projectile.height = 8;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.friendly = false;
			projectile.hostile = true;
		}

		private bool hasSpawned = false;

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				projectile.velocity = Vector2.Normalize(new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center) * 10f;

				Main.PlaySound(SoundID.NPCDeath19.WithVolume(0.6f), projectile.Center);
			}

			projectile.velocity.Y += (8f / 60f);

			projectile.velocity.Y = Utils.Clamp(projectile.velocity.Y, -10, 10);

			//Spawn several dust randomly
			for(int i = 0; i < 4; i++)
				if(Main.rand.NextFloat() < 0.125f)
					Dust.NewDust(projectile.position, projectile.width, projectile.height, 74);

		}
	}
}
