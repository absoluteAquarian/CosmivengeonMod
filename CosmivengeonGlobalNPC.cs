using System;
using CosmivengeonMod.Buffs;
using CosmivengeonMod.Items.Frostbite;
using CosmivengeonMod.NPCs.Draek;
using CosmivengeonMod.Projectiles.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod{
	public class CosmivengeonGlobalNPC : GlobalNPC{
		public override bool InstancePerEntity => true;

		public bool primordialWrath;

		public float endurance;
		public float baseEndurance = 0f;

		public override void ResetEffects(NPC npc){
			primordialWrath = false;
			endurance = baseEndurance;
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage){
			if(primordialWrath){
				if(npc.lifeRegen > 0)
					npc.lifeRegen = 0;
				npc.defense -= 10;
				endurance -= 0.1f;
				npc.lifeRegen -= 15 * 2;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor){
			if(primordialWrath)
				drawColor = CosmivengeonUtils.Blend(drawColor, Color.DarkRed);
		}

		public override void ModifyHitByItem(NPC npc, Player player, Item item, ref int damage, ref float knockback, ref bool crit){
			damage = (int)(Math.Max(1 - endurance, 0.01) * damage);
		}

		public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection){
			damage = (int)(Math.Max(1 - endurance, 0.01) * damage);
		}

		public override void OnHitByItem(NPC npc, Player player, Item item, int damage, float knockback, bool crit){
			if(npc.life <= 0 && item.modItem is SubZero){
				//Spawn some projectiles
				int numProjs = 20;
				for(int i = 0; i < 20; i++){
					Projectile.NewProjectile(npc.Center, new Vector2(-9, 0).RotateDegrees(360f / numProjs * i, 180f / numProjs), ModContent.ProjectileType<FrostbiteBreath>(), item.damage, 3f, player.whoAmI, 0f, 4f);
				}
			}
		}
	}
}
