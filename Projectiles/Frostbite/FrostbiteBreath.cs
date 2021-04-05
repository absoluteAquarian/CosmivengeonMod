using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Frostbite{
	public class FrostbiteBreath : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Frostbite/FrostbiteIcicle";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frozen Fire");
		}

		public override void SetDefaults(){
			projectile.tileCollide = true;
			projectile.width = 8;
			projectile.height = 8;
			projectile.alpha = 255;
			projectile.timeLeft = 150;
		}

		private bool spawned = false;

		public override void AI(){
			if(!spawned){
				spawned = true;
				//The projectile can be spawned by Frostbite or the Player
				//We need to set its hostility depending on who created the projectile
				projectile.hostile = projectile.ai[1] == 0f;
				projectile.friendly = projectile.ai[1] != 0f;

				//If this projectile was fired from a weapon, make it pierce some
				if(projectile.ai[1] != 4f && projectile.friendly && !projectile.hostile){
					projectile.penetrate = 3;
					projectile.usesLocalNPCImmunity = true;
					projectile.localNPCHitCooldown = -1;  //Force only one hit per NPC
				}

				//Make projectiles die quickly when spawned by Frostbite
				if(projectile.hostile && projectile.ai[0] != 1)
					projectile.timeLeft = 60;

				//Sub-Zero projectile spawn
				//Make it a melee projectile
				if(projectile.ai[1] == 4f){
					projectile.melee = true;
					projectile.magic = false;
					projectile.ranged = false;
					projectile.thrown = false;
					projectile.minion = false;

					projectile.timeLeft = 18;
				}
			}

			//Spawn 9 dusts randomly
			for(int i = 0; i < 4; i++){
				if(Main.rand.NextFloat() < 0.8f){
					Dust dust = Dust.NewDustDirect(projectile.position - new Vector2(2, 2), projectile.width + 2, projectile.height + 2, 92);
					dust.velocity = Vector2.Zero;
					dust.noGravity = true;
					dust.fadeIn = 1.15f;
				}
			}

			if(projectile.ai[0] == 1)
				projectile.velocity.Y += 16f / 60f;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			if(Main.rand.NextFloat() < 0.125f)
				target.AddBuff(BuffID.Frostburn, 7 * 60);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			if(Main.rand.NextFloat() < 0.125f)
				target.AddBuff(BuffID.Frostburn, 7 * 60);
		}
	}
}