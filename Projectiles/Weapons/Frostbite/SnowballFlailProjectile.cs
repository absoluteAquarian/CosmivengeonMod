using CosmivengeonMod.Items.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class SnowballFlailProjectile : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Snowball");
		}

		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
		}

		public override void AI() {
			//Copying AI from Flairon projectile
			Vector2 directionToOwner = Main.player[Projectile.owner].Center - Projectile.Center;
			Projectile.rotation = directionToOwner.ToRotation() - MathHelper.PiOver2;

			if (Main.player[Projectile.owner].dead) {
				Projectile.Kill();
			} else {
				Main.player[Projectile.owner].itemAnimation = 10;
				Main.player[Projectile.owner].itemTime = 10;

				if (directionToOwner.X < 0f) {
					Main.player[Projectile.owner].ChangeDir(1);
					Projectile.direction = 1;
				} else {
					Main.player[Projectile.owner].ChangeDir(-1);
					Projectile.direction = -1;
				}

				Main.player[Projectile.owner].itemRotation = (directionToOwner * -1f * Projectile.direction).ToRotation();
				Projectile.spriteDirection = directionToOwner.X <= 0f ? 1 : -1;

				if (Projectile.ai[0] == 0f && directionToOwner.Length() > 550f)
					Projectile.ai[0] = 1f;

				if (Projectile.ai[0] == 1f || Projectile.ai[0] == 2f) {
					float projectileLength = directionToOwner.Length();
					if (projectileLength > 1500f) {
						Projectile.Kill();
						return;
					}

					if (projectileLength > 750f)
						Projectile.ai[0] = 2f;

					Projectile.tileCollide = false;
					float num698 = SnowballFlail.ProjectileVelocity;

					if (Projectile.ai[0] == 2f)
						num698 = SnowballFlail.ProjectileVelocity * 2f;

					Projectile.velocity = Vector2.Normalize(directionToOwner) * num698;

					if (directionToOwner.Length() < num698) {
						Projectile.Kill();
						return;
					}
				}

				Projectile.ai[1] += 1f;

				if (Projectile.ai[1] > 5f) {
					Projectile.alpha = 0;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
			Vector2 projCenter = Projectile.Center;
			Vector2 direction = mountedCenter - Projectile.Center;
			float rotation = direction.ToRotation() - MathHelper.PiOver2;

			Texture2D chain = ModContent.GetTexture("CosmivengeonMod/Assets/Chains/SnowballFlail");

			if (Projectile.alpha == 0) {
				int num128 = -1;
				if (Projectile.position.X + Projectile.width / 2 < mountedCenter.X)
					num128 = 1;
				if (Main.player[Projectile.owner].direction == 1)
					Main.player[Projectile.owner].itemRotation = (num128 * direction).ToRotation();
				else
					Main.player[Projectile.owner].itemRotation = (num128 * direction).ToRotation();
			}

			bool flag20 = true;
			while (flag20) {
				float num129 = direction.Length();
				if (num129 < 25f)
					flag20 = false;
				else if (float.IsNaN(num129))
					flag20 = false;
				else {
					num129 = 12f / num129;
					direction *= num129;
					projCenter += direction;
					direction = mountedCenter - projCenter;

					spriteBatch.Draw(chain, projCenter - Main.screenPosition, null, lightColor, rotation, new Vector2(chain.Width * 0.5f, chain.Height * 0.5f), 1f, SpriteEffects.None, 0f);
				}
			}

			return true;
		}
	}
}
