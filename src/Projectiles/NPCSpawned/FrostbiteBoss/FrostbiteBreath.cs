using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss {
	public class FrostbiteBreath : ModProjectile {
		public override string Texture => "CosmivengeonMod/Assets/Empty";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frozen Fire");
		}

		public override void SetDefaults() {
			Projectile.tileCollide = true;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.alpha = 255;
			Projectile.timeLeft = 150;
		}

		private bool spawned = false;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				//The projectile can be spawned by Frostbite or the Player
				//We need to set its hostility depending on who created the projectile
				Projectile.hostile = Projectile.ai[1] == 0f;
				Projectile.friendly = Projectile.ai[1] != 0f;

				//If this projectile was fired from a weapon, make it pierce some
				if (Projectile.ai[1] != 4f && Projectile.friendly && !Projectile.hostile) {
					Projectile.penetrate = 3;
					Projectile.usesLocalNPCImmunity = true;
					Projectile.localNPCHitCooldown = -1;  //Force only one hit per NPC
				}

				//Make projectiles die quickly when spawned by Frostbite
				if (Projectile.hostile && Projectile.ai[0] != 1)
					Projectile.timeLeft = 60;

				//Sub-Zero projectile spawn
				//Make it a melee projectile
				if (Projectile.ai[1] == 4f) {
					Projectile.DamageType = DamageClass.Melee;
					Projectile.minion = false;

					Projectile.timeLeft = 18;
				}
			}

			//Spawn 9 dusts randomly
			for (int i = 0; i < 4; i++) {
				if (Main.rand.NextFloat() < 0.8f) {
					Dust dust = Dust.NewDustDirect(Projectile.position - new Vector2(2, 2), Projectile.width + 2, Projectile.height + 2, DustID.Frost);
					dust.velocity = Vector2.Zero;
					dust.noGravity = true;
					dust.fadeIn = 1.15f;
				}
			}

			if (Projectile.ai[0] == 1)
				Projectile.velocity.Y += 16f / 60f;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			if (Main.rand.NextFloat() < 0.125f)
				target.AddBuff(BuffID.Frostburn, 7 * 60);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			if (Main.rand.NextFloat() < 0.125f)
				target.AddBuff(BuffID.Frostburn, 7 * 60);
		}
	}
}
