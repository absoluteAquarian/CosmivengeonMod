using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Frostbite{
	public class FrostbiteRock : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Giant Snowball");
		}

		public override void SetDefaults(){
			projectile.width = 32;
			projectile.height = 32;
			projectile.hostile = true;
			projectile.friendly = false;
			projectile.timeLeft = 2 * 60;
		}

		private bool spawned = false;
		private Player Target;
		private float targetSpeed;

		public override void AI(){
			if(!spawned){
				spawned = true;
				Target = Main.player[(int)projectile.ai[0]];
				targetSpeed = projectile.ai[1];
			}

			//Get the push vector towards the target
			projectile.velocity += projectile.DirectionTo(Target.Center) * 0.2713f;

			//Cap the projectile's velocity to the intended speed
			if(projectile.velocity.Length() != targetSpeed)
				projectile.velocity = Vector2.Normalize(projectile.velocity) * targetSpeed;

			projectile.rotation += (projectile.velocity.X >= 0 ? 1 : -1) * MathHelper.ToRadians(2.5f * 360f / 60f);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			target.AddBuff(BuffID.Chilled, (CosmivengeonWorld.desoMode ? 3 : 5) * 60);
		}
	}
}
