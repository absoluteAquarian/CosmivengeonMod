using CosmivengeonMod.Projectiles.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class IceScepterWall : ModProjectile{
		public override string Texture => "CosmivengeonMod/NPCs/Frostbite/FrostbiteWall";

		public static readonly float Scale = 0.87f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frozen Totem");
		}

		public override void SetDefaults(){
			projectile.sentry = true;
			projectile.width = 30;
			projectile.height = 116;
			projectile.timeLeft = 15 * 60;
			projectile.scale = Scale;
			projectile.tileCollide = true;
		}

		private int AI_Timer = -1;

		public override void AI(){
			projectile.TryDecrementAlpha(10);

			//copied from FrostbiteWall AI
			if(AI_Timer < 0)
				AI_Timer = Main.rand.Next(60, 120);
			else if(AI_Timer == 0){
				//Spawn some Frostbite ice projectiles (the breath ones)
				for(int i = 0; i < 6; i++){
					Projectile.NewProjectile(
						projectile.Top + new Vector2(0, 16),
						new Vector2(0, -8).RotatedByRandom(MathHelper.ToRadians(15)),
						ModContent.ProjectileType<FrostbiteBreath>(),
						projectile.damage,
						2f,
						Main.myPlayer,
						1f,
						1f
					);

					Main.PlaySound(SoundID.Item28.WithVolume(0.6f), projectile.Top);
				}
			}

			//Make this sentry fall if we couldn't find a tile to place it on beforehand
			projectile.velocity.Y += 8f / 60f;

			AI_Timer--;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => false;

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough){
			fallThrough = false;
			return base.TileCollideStyle(ref width, ref height, ref fallThrough);
		}
	}
}
