using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class DesomodeProjectile : GlobalProjectile{
		public override void ModifyHitNPC(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			//Desomode Skeletron takes 33% less damage from bee projectiles
			if(CosmivengeonWorld.desoMode && (target.type == NPCID.SkeletronHead || target.type == NPCID.SkeletronHand) && (projectile.type == ProjectileID.Bee || projectile.type == ProjectileID.BeeArrow || projectile.type == ProjectileID.Beenade || projectile.type == ProjectileID.GiantBee))
				damage = (int)(damage * 0.6667f);
		}
	}
}
