using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode {
	public class KingSlimeSpike : ModProjectile {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SpikedSlimeSpike}";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault(Lang.GetProjectileName(ProjectileID.SpikedSlimeSpike).Value);
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.SpikedSlimeSpike);
			Projectile.scale = 2f;
			AIType = ProjectileID.SpikedSlimeSpike;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			int duration = 10 * 60;
			if (!target.HasBuff(BuffID.Slimed))
				target.AddBuff(BuffID.Slimed, duration);
			else {
				int index = target.FindBuffIndex(BuffID.Slimed);
				target.buffTime[index] = Math.Min(target.buffTime[index] + 3 * 60, duration);
			}
		}
	}
}
