using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.SwordUpgrade{
	public class CrystaliceSwordProjectile : ModProjectile{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.FrostBoltSword;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Shard");
		}

		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.FrostBoltSword);
			projectile.aiStyle = 28;
			aiType = ProjectileID.FrostBoltSword;
			projectile.penetrate = 1;
			projectile.timeLeft = 3 * 60;
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			if(Main.rand.NextFloat() < Items.SwordUpgrade.CrystaliceSword.SlowChance)
				target.AddBuff(BuffID.Frostburn, 2 * 60);
		}
	}
}
