using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class SnowballFlailProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Giant Snowball");
		}

		public override void SetDefaults(){
			projectile.width = 32;
			projectile.height = 32;
			projectile.melee = true;
			projectile.penetrate = -1;
			projectile.friendly = true;
			projectile.hostile = false;
		}

		private int AI_Timer = 0;

		public override void AI(){
			//Copying AI from Flairon projectile
			Vector2 directionToOwner = Main.player[projectile.owner].Center - projectile.Center;
			projectile.rotation = directionToOwner.ToRotation() - MathHelper.PiOver2;

			if (Main.player[projectile.owner].dead){
				projectile.Kill();
			}else{
				Main.player[projectile.owner].itemAnimation = 10;
				Main.player[projectile.owner].itemTime = 10;

				if(directionToOwner.X < 0f){
					Main.player[projectile.owner].ChangeDir(1);
					projectile.direction = 1;
				}else{
					Main.player[projectile.owner].ChangeDir(-1);
					projectile.direction = -1;
				}

				Main.player[projectile.owner].itemRotation = (directionToOwner * -1f * projectile.direction).ToRotation();
				projectile.spriteDirection = directionToOwner.X <= 0f ? 1 : -1;

				if(projectile.ai[0] == 0f && directionToOwner.Length() > 400f)
					projectile.ai[0] = 1f;

				if(projectile.ai[0] == 1f || projectile.ai[0] == 2f){
					float projectileLength = directionToOwner.Length();
					if(projectileLength > 1500f){
						projectile.Kill();
						return;
					}

					if(projectileLength > 600f)
						projectile.ai[0] = 2f;

					projectile.tileCollide = false;
					float num698 = 20f;

					if(projectile.ai[0] == 2f)
						num698 = 40f;

					projectile.velocity = Vector2.Normalize(directionToOwner) * num698;

					if(directionToOwner.Length() < num698){
						projectile.Kill();
						return;
					}
				}

				projectile.ai[1] += 1f;

				if(projectile.ai[1] > 5f){
					projectile.alpha = 0;
				}

				if(AI_Timer % 20 == 19 && projectile.owner == Main.myPlayer){
					for(int i = 0; i < 8; i++){
						float rangeStart = MathHelper.ToRadians(360f / 8f);
						Vector2 direction = Main.rand.NextFloat(rangeStart * i, rangeStart * (i + 1)).ToRotationVector2();
						float speed = Main.rand.NextFloat(3f, 5f);
						direction *= speed;

						Projectile.NewProjectile(
							projectile.Center,
							direction,
							ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(),
							(int)(projectile.damage * 0.6667f),
							3f,
							Main.myPlayer,
							0f,
							1f
						);
					}
				}
			}

			AI_Timer++;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Vector2 mountedCenter = Main.player[projectile.owner].MountedCenter;
			Vector2 projCenter = projectile.Center;
			Vector2 direction = mountedCenter - projectile.Center;
			float rotation = direction.ToRotation() - MathHelper.PiOver2;

			Texture2D chain = ModContent.GetTexture("CosmivengeonMod/Chains/SnowballFlail");

			if (projectile.alpha == 0){
				int num128 = -1;
				if (projectile.position.X + projectile.width / 2 < mountedCenter.X)
					num128 = 1;
				if (Main.player[projectile.owner].direction == 1)
					Main.player[projectile.owner].itemRotation = (num128 * direction).ToRotation();
				else
					Main.player[projectile.owner].itemRotation = (num128 * direction).ToRotation();
			}

			bool flag20 = true;
			while (flag20){
				float num129 = direction.Length();
				if (num129 < 25f)
					flag20 = false;
				else if (float.IsNaN(num129))
					flag20 = false;
				else{
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
