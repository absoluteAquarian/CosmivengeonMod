using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class BlizzardRodProjectile : ModProjectile{
		public override string Texture => "CosmivengeonMod/NPCs/Frostbite/FrostCloud";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Blizzard Cloud");
			Main.projFrames[projectile.type] = 4;
		}

		public override void SetDefaults(){
			projectile.width = 46;
			projectile.height = 32;
			projectile.friendly = true;
			projectile.timeLeft = 15 * 60;
			projectile.alpha = 255;
		}

		public static readonly int Delay = 25;
		private int timer = -Delay;

		public override void AI(){
			if(projectile.alpha > 0 && timer < 0)
				projectile.alpha -= 9;
			if((projectile.alpha < 0 && timer < 0) || (timer >= 0 && projectile.alpha > 0))
				projectile.alpha = 0;

			if(timer++ > 0 && timer % 20 == 0){
				Projectile.NewProjectile(
					projectile.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-4f, 0f)),
					Vector2.Zero,
					ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(),
					(int)(projectile.damage * 0.82f),
					projectile.knockBack,
					Main.myPlayer,
					1f,
					2f
				);
			}

			if(++projectile.frameCounter % 17 == 0)
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
		}
	}
}
