using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Frostbite{
	public class FrostbiteWall : ModNPC{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frozen Totem");
		}

		private bool shouldShootBolts = false;
		private int activeTime = 1;

		public override void SetDefaults(){
			npc.noTileCollide = false;
			npc.noGravity = false;
			npc.dontTakeDamage = true;
			npc.lifeMax = 1;
			npc.knockBackResist = 0f;
			npc.width = 30;
			npc.height = 116;
			npc.alpha = 255;
			npc.friendly = false;
		}

		private bool spawned = false;
		private int AI_Timer = -1;

		public override void AI(){
			if(!spawned){
				spawned = true;
				npc.TargetClosest(false);
				activeTime = (int)npc.ai[0];
				npc.damage = (int)npc.ai[1];
				shouldShootBolts = npc.ai[2] == 1;
			}

			activeTime--;
			if(activeTime < 0){
				npc.life = 0;
				npc.active = false;
			}

			if(npc.alpha > 0)
				npc.alpha -= 3;
			else if(npc.alpha < 0)
				npc.alpha = 0;

			if(shouldShootBolts){
				if(AI_Timer < 0)
					AI_Timer = Main.rand.Next(60, 120);
				else if(AI_Timer == 0){
					//Spawn some Frostbite ice projectiles (the breath ones)
					for(int i = 0; i < 6; i++){
						npc.SpawnProjectile(
							npc.Top + new Vector2(0, 16),
							new Vector2(0, -8).RotatedByRandom(MathHelper.ToRadians(15)),
							ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteBreath>(),
							30,
							2f,
							Main.myPlayer,
							1f
						);
					}
				}
			}

			npc.velocity.Y += 8f / 60f;

			AI_Timer--;
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit){
			if(npc.alpha > 0)
				damage = 0;
		}
	}
}
