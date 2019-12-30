using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.SwordUpgrade{
	public class BreadSwordProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Poisonous Orb");
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.penetrate = 1;
			projectile.alpha = 0;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.ignoreWater = true;
			projectile.timeLeft = 4 * 60;
			projectile.melee = true;
			projectile.scale = 0.75f;
		}

		public override void OnHitNPC(NPC target, int damage, float knockBack, bool crit){
			target.AddBuff(BuffID.Poisoned, 8 * 60);
		}

		public override void AI(){
			//Spawn dust
			for(int i = 0; i < 12; i++)
				if(Main.rand.NextFloat() < 0.125f){
					Dust dust = Dust.NewDustDirect(projectile.Center - new Vector2(2, 2), projectile.width - 4, projectile.height - 4, 74, 0, 0);
					dust.velocity = Vector2.Zero;
				}

			projectile.rotation += MathHelper.ToRadians(2.5f * 360f / 60f);

			//Target the closest hostile NPC
			List<NPC> activeHostileNPCs = Main.npc.Where(n => n.active && (!n.friendly || n.boss) && !n.immortal && !n.dontTakeDamage).ToList();
			if(activeHostileNPCs.Any()){
				//Generate a vector to the hostile NPC and nudge this projectile towards it
				// only if the NPC is within 20 tiles of the projectile
				List<NPC> closestHostiles = activeHostileNPCs.OrderBy(n => Vector2.Distance(projectile.Center, n.Center)).Where(n => Vector2.Distance(projectile.Center, n.Center) < 20 * 16).ToList();

				if(closestHostiles.Any()){
					NPC closestHostile = closestHostiles.First();
					Vector2 direction = Vector2.Normalize(closestHostile.Center - projectile.Center);
					projectile.velocity += direction * 0.8793f;
				}
			}

			//Finally, make sure that this projectile is always moving at max speed
			if(projectile.velocity.Length() != projectile.ai[0])
				projectile.velocity = Vector2.Normalize(projectile.velocity) * projectile.ai[0];
		}
	}
}
