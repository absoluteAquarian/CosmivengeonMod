using CosmivengeonMod.Items.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class IceDiskProjectile : ModProjectile{
		public override string Texture => "CosmivengeonMod/Items/Weapons/Frostbite/IceDisk";

		public static float MaxVelocity = 8.72f;
		public static float HomingFactor = 2.1153f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Abominable's Animosity");
		}

		public override void SetDefaults(){
			projectile.width = 42;
			projectile.height = 42;
			projectile.scale = 0.8f;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.thrown = true;
			projectile.penetrate = -1;

			projectile.gfxOffY = -1;
		}

		private int timer = 0;
		private Player Parent = null;

		private float oldDist = -1f;

		public override void AI(){
			if(Parent is null)
				Parent = Main.player[projectile.owner];

			if(projectile.ai[0] == 0f){
				MoveTowardsCursor();

				if(Main.rand.NextFloat() < 0.35f){
					Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 92, 0f, 0f, 50, Scale: 1.2f);
					dust.noGravity = true;
					dust.velocity = Vector2.Zero;
				}

				if(!Parent.channel || !(Parent.HeldItem.modItem is IceDisk) || projectile.Distance(Main.MouseWorld) < 2.5f * 16)
					projectile.ai[0] = 1f;
			}else if(projectile.ai[0] == 1f){
				int minTime = 35;

				if(timer == 0)
					projectile.timeLeft = 4 * minTime + 1;

				timer++;

				projectile.velocity *= 0.9217f;

				if(timer % minTime == 0){
					float startAngle = Main.rand.NextFloat(MathHelper.PiOver2);

					for(int i = 0; i < 4; i++)
						Projectile.NewProjectile(projectile.Center, new Vector2(5f, 0f).RotatedBy(startAngle + MathHelper.PiOver2 * i), ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(), (int)(projectile.damage * 0.32f), 3f, projectile.owner, 1f, 0f);
				}
			}

			float rotationsPerSecond = projectile.ai[0] == 1f ? 16f : 8f;
			projectile.rotation += MathHelper.ToRadians(rotationsPerSecond * 6f) * (projectile.velocity.X >= 0 ? 1 : -1);

			oldDist = projectile.Distance(Main.MouseWorld);
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			if(projectile.velocity.X != oldVelocity.X)
				projectile.velocity.X = -Math.Sign(oldVelocity.X) * (HomingFactor * 1.75f);
			if(projectile.velocity.Y != oldVelocity.Y)
				projectile.velocity.Y = -Math.Sign(oldVelocity.Y) * (HomingFactor * 1.75f);

			return projectile.ai[0] == 2f;
		}

		public override void Kill(int timeLeft){
			if(projectile.ai[0] == 2f)
				return;

			float angle = Main.rand.NextFloat(MathHelper.Pi);

			for(int i = 0; i < 2; i++){
				Projectile proj = Projectile.NewProjectileDirect(projectile.Center, new Vector2(MaxVelocity, 0f).RotatedBy(angle + MathHelper.Pi * i), projectile.type, (int)(projectile.damage * 0.5f), projectile.knockBack, projectile.owner, 2f);
				proj.timeLeft = 37;
			}

			Main.PlaySound(SoundID.Item27, projectile.Center);
		}

		private void MoveTowardsCursor(){
			projectile.velocity += projectile.DirectionTo(Main.MouseWorld) * HomingFactor;

			if(projectile.Distance(Main.MouseWorld) > oldDist)
				projectile.velocity *= 0.891f;
			else
				projectile.velocity *= 0.975f;

			if(projectile.velocity.Length() != MaxVelocity)
				projectile.velocity = Vector2.Normalize(projectile.velocity) * MaxVelocity;
		}
	}
}