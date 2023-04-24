using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class CrystaliceWandProjectile : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.FrostBoltSword;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frost Shard");
		}

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.FrostBoltSword);
			Projectile.aiStyle = 28;
			AIType = ProjectileID.FrostBoltSword;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 3600;  //Default timeLeft
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Main.rand.NextFloat() < 0.1111f)
				target.AddBuff(BuffID.Frostburn, 2 * 60);
		}

		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
		}
	}
}