using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod{
	public class CosmivengeonGlobalProjectile : GlobalProjectile{
		public override bool InstancePerEntity => true;

		public bool shotFromCrystaliceBow = false;
		private bool crystaliceBowCheck = false;

		public override void AI(Projectile projectile){
			if(!shotFromCrystaliceBow && !crystaliceBowCheck){
				crystaliceBowCheck = true;
				if(Main.player[projectile.owner].HeldItem.modItem is Items.Frostbite.CrystaliceBow)
					shotFromCrystaliceBow = true;
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit){
			if(shotFromCrystaliceBow)
				target.AddBuff(BuffID.Frostburn, 5 * 60);
		}
	}
}
