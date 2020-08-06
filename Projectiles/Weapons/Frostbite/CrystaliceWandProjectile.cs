using CosmivengeonMod.Projectiles.SwordUpgrade;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class CrystaliceWandProjectile : CrystaliceSwordProjectile{
		public override void SetDefaults(){
			base.SetDefaults();
			projectile.timeLeft = 3600;  //Default timeLeft
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			if(Main.rand.NextFloat() < 0.1111f)
				target.AddBuff(BuffID.Frostburn, 2 * 60);
			projectile.Kill();
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			projectile.Kill();
			return true;
		}

		public override void Kill(int timeLeft){
			//Spawn 6 smaller shards
			for(int i = 0; i < 6; i++){
				Projectile.NewProjectile(
					projectile.Center,
					projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30f)).RotatedBy(MathHelper.ToRadians(60f * i)),
					ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(),
					(int)(projectile.damage * 0.8f),
					projectile.knockBack * 0.75f,
					projectile.owner,
					1f,
					2f
				);
			}

			Main.PlaySound(SoundID.Item27, projectile.Center);
		}
	}
}