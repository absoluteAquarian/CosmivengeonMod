using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSwordExtra : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Draek/DraekSword";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek's Sword");
		}
		public override void SetDefaults(){
			projectile.height = 30;
			projectile.width = 30;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 5 * 60;
			projectile.aiStyle = 0;
			projectile.alpha = 255;
		}

		private bool hasSpawned = false;

		public override void AI(){
			Projectile owner = Main.projectile[(int)projectile.ai[0]];
			
			if(!hasSpawned){
				hasSpawned = true;
			}
			
			if(!owner.active || owner.type != ModContent.ProjectileType<DraekSword>()){
				projectile.Kill();
				return;
			}

			float rotation = CosmivengeonUtils.ToActualAngle(owner.rotation);

			Vector2 center = owner.Center;
			center.X += projectile.ai[1] * CosmivengeonUtils.fSin(rotation) * 48;
			center.Y += projectile.ai[1] * -CosmivengeonUtils.fCos(rotation) * 48;
			projectile.Center = center;
		}
	}
}
