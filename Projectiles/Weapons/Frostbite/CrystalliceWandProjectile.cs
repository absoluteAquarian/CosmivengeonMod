using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class CrystaliceWandProjectile : ModProjectile{
		public override string Texture => "Terraria/Projectile_" + ProjectileID.FrostBoltSword;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Shard");
		}

		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.FrostBoltSword);
			projectile.aiStyle = 28;
			aiType = ProjectileID.FrostBoltSword;
			projectile.penetrate = 1;
			projectile.timeLeft = 3600;  //Default timeLeft
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			if(Main.rand.NextFloat() < 0.1111f)
				target.AddBuff(BuffID.Frostburn, 2 * 60);
		}

		public override void Kill(int timeLeft){
			Main.PlaySound(SoundID.Item27, projectile.Center);
		}
	}
}