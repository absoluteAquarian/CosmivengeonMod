﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSwordExtra : ModProjectile{
		public override string Texture => "CosmivengeonMod/NPCs/Empty";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
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

		Projectile owner = null;

		public override void AI(){
			owner = Main.projectile[(int)projectile.ai[0]];

			projectile.damage = owner.damage;
			
			if(!owner.active || owner.type != ModContent.ProjectileType<DraekSword>()){
				projectile.Kill();
				return;
			}

			projectile.timeLeft = owner.timeLeft;

			float rotation = CosmivengeonUtils.ToActualAngle(owner.rotation);

			Vector2 center = owner.Center;
			center.X += projectile.ai[1] * CosmivengeonUtils.fSin(rotation) * 48;
			center.Y += projectile.ai[1] * -CosmivengeonUtils.fCos(rotation) * 48;
			projectile.Center = center;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			(owner?.modProjectile as DraekSword)?.OnHitPlayer(target, damage, crit);
		}
	}
}
