using CosmivengeonMod.Items.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class IceDiskProjectile : ModProjectile {
		public override string Texture => "CosmivengeonMod/Items/Weapons/Frostbite/IceDisk";

		public static float MaxVelocity = 8.72f;
		public static float HomingFactor = 2.1153f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Abominable's Animosity");
		}

		public override void SetDefaults() {
			Projectile.width = 42;
			Projectile.height = 42;
			Projectile.scale = 0.8f;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Throwing;
			Projectile.penetrate = -1;

			Projectile.gfxOffY = -1;
		}

		private int timer = 0;
		private Player Parent = null;

		private float oldDist = -1f;

		public override void AI() {
			if (Parent is null)
				Parent = Main.player[Projectile.owner];

			if (Projectile.ai[0] == 0f) {
				MoveTowardsCursor();

				if (Main.rand.NextFloat() < 0.35f) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 92, 0f, 0f, 50, Scale: 1.2f);
					dust.noGravity = true;
					dust.velocity = Vector2.Zero;
				}

				if (!Parent.channel || !(Parent.HeldItem.ModItem is IceDisk) || Projectile.Distance(Main.MouseWorld) < 2.5f * 16)
					Projectile.ai[0] = 1f;
			} else if (Projectile.ai[0] == 1f) {
				int minTime = 35;

				if (timer == 0)
					Projectile.timeLeft = 4 * minTime + 1;

				timer++;

				Projectile.velocity *= 0.9217f;

				if (timer % minTime == 0) {
					float startAngle = Main.rand.NextFloat(MathHelper.PiOver2);

					for (int i = 0; i < 4; i++)
						Projectile.NewProjectile(Projectile.Center, new Vector2(5f, 0f).RotatedBy(startAngle + MathHelper.PiOver2 * i), ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(), (int)(Projectile.damage * 0.32f), 3f, Projectile.owner, 1f, 0f);
				}
			}

			float rotationsPerSecond = Projectile.ai[0] == 1f ? 16f : 8f;
			Projectile.rotation += MathHelper.ToRadians(rotationsPerSecond * 6f) * (Projectile.velocity.X >= 0 ? 1 : -1);

			oldDist = Projectile.Distance(Main.MouseWorld);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X != oldVelocity.X)
				Projectile.velocity.X = -Math.Sign(oldVelocity.X) * (HomingFactor * 1.75f);
			if (Projectile.velocity.Y != oldVelocity.Y)
				Projectile.velocity.Y = -Math.Sign(oldVelocity.Y) * (HomingFactor * 1.75f);

			return Projectile.ai[0] == 2f;
		}

		public override void Kill(int timeLeft) {
			if (Projectile.ai[0] == 2f)
				return;

			float angle = Main.rand.NextFloat(MathHelper.Pi);

			for (int i = 0; i < 2; i++) {
				Projectile proj = Projectile.NewProjectileDirect(Projectile.Center, new Vector2(MaxVelocity, 0f).RotatedBy(angle + MathHelper.Pi * i), Projectile.type, (int)(Projectile.damage * 0.5f), Projectile.knockBack, Projectile.owner, 2f);
				proj.timeLeft = 37;
			}

			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
		}

		private void MoveTowardsCursor() {
			Projectile.velocity += Projectile.DirectionTo(Main.MouseWorld) * HomingFactor;

			if (Projectile.Distance(Main.MouseWorld) > oldDist)
				Projectile.velocity *= 0.891f;
			else
				Projectile.velocity *= 0.975f;

			if (Projectile.velocity.Length() != MaxVelocity)
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxVelocity;
		}
	}
}