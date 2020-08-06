using CosmivengeonMod.Items.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod{
	public class CosmivengeonGlobalProjectile : GlobalProjectile{
		public override bool InstancePerEntity => true;

		public bool shotFromCrystaliceBow = false;

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit){
			if(shotFromCrystaliceBow)
				target.AddBuff(BuffID.Frostburn, 5 * 60);
		}
	}
}
