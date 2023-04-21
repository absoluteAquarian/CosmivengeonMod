using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss{
	public class DraekSwordExtra : ModProjectile{
		public override string Texture => "CosmivengeonMod/Assets/Empty";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
		}
		public override void SetDefaults(){
			Projectile.height = 30;
			Projectile.width = 30;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5 * 60;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
		}

		Projectile owner = null;

		public override void AI(){
			owner = Main.projectile[(int)Projectile.ai[0]];

			Projectile.damage = owner.damage;
			
			if(!owner.active || owner.type != ModContent.ProjectileType<DraekSword>()){
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = owner.timeLeft;

			float rotation = MiscUtils.ToActualAngle(owner.rotation);

			Vector2 center = owner.Center;
			center.X += Projectile.ai[1] * MiscUtils.fSin(rotation) * 48;
			center.Y += Projectile.ai[1] * -MiscUtils.fCos(rotation) * 48;
			Projectile.Center = center;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			(owner?.ModProjectile as DraekSword)?.OnHitPlayer(target, damage, crit);
		}
	}
}
