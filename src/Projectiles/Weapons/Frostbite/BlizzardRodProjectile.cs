using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class BlizzardRodProjectile : ModProjectile {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/FrostbiteBoss/Summons/FrostCloud";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Blizzard Cloud");
			Main.projFrames[Projectile.type] = 4;
		}

		public override void SetDefaults() {
			Projectile.width = 46;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.timeLeft = 15 * 60;
			Projectile.alpha = 255;
		}

		public static readonly int Delay = 25;
		private int timer = -Delay;

		public override void AI() {
			if (Projectile.alpha > 0 && timer < 0)
				Projectile.alpha -= 9;
			if ((Projectile.alpha < 0 && timer < 0) || (timer >= 0 && Projectile.alpha > 0))
				Projectile.alpha = 0;

			if (timer++ > 0 && timer % 20 == 0) {
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center + new Vector2(Main.rand.NextFloat(-16, 16), Main.rand.NextFloat(-4f, 0f)),
					Vector2.Zero,
					ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(),
					(int)(Projectile.damage * 0.82f),
					Projectile.knockBack,
					Main.myPlayer,
					1f,
					2f
				);
			}

			if (++Projectile.frameCounter % 17 == 0)
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
		}
	}
}
