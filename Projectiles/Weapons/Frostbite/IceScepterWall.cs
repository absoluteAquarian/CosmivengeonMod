using CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss;
using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class IceScepterWall : ModProjectile {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/FrostbiteBoss/Summons/FrostbiteWall";

		public static readonly float Scale = 0.87f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frozen Totem");
		}

		public override void SetDefaults() {
			Projectile.sentry = true;
			Projectile.width = 30;
			Projectile.height = 116;
			Projectile.timeLeft = 60 * 60;
			Projectile.scale = Scale;
			Projectile.tileCollide = true;
		}

		private int AI_Timer = -1;

		public override void AI() {
			Projectile.TryDecrementAlpha(10);

			//copied from FrostbiteWall AI
			if (AI_Timer < 0)
				AI_Timer = Main.rand.Next(48, 90);
			else if (AI_Timer == 0) {
				//Spawn some Frostbite ice projectiles (the breath ones)
				for (int i = 0; i < 6; i++) {
					Projectile.NewProjectile(
						Projectile.Top + new Vector2(0, 16),
						new Vector2(0, -12).RotatedByRandom(MathHelper.ToRadians(40)),
						ModContent.ProjectileType<FrostbiteBreath>(),
						Projectile.damage,
						2f,
						Main.myPlayer,
						1f,
						1f
					);

					SoundEngine.PlaySound(SoundID.Item28.WithVolume(0.6f), Projectile.Top);
				}
			}

			//Make this sentry fall if we couldn't find a tile to place it on beforehand
			Projectile.velocity.Y += 8f / 60f;

			AI_Timer--;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => false;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return base.TileCollideStyle(ref width, ref height, ref fallThrough);
		}
	}
}
