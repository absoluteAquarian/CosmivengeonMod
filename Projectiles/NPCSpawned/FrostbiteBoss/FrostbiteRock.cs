using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss {
	public class FrostbiteRock : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Giant Snowball");
		}

		public override void SetDefaults() {
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.timeLeft = 2 * 60;
		}

		private bool spawned = false;
		private Player Target;
		private float targetSpeed;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				Target = Main.player[(int)Projectile.ai[0]];
				targetSpeed = Projectile.ai[1];
			}

			//Get the push vector towards the target
			Projectile.velocity += Projectile.DirectionTo(Target.Center) * 0.2713f;

			//Cap the projectile's velocity to the intended speed
			if (Projectile.velocity.Length() != targetSpeed)
				Projectile.velocity = Vector2.Normalize(Projectile.velocity) * targetSpeed;

			Projectile.rotation += (Projectile.velocity.X >= 0 ? 1 : -1) * MathHelper.ToRadians(2.5f * 360f / 60f);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			target.AddBuff(BuffID.Chilled, (WorldEvents.desoMode ? 3 : 5) * 60);
		}
	}
}
