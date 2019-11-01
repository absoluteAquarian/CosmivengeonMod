using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod{
	public class CosmivengeonGlobalNPC : GlobalNPC{
		public override bool InstancePerEntity => true;

		public bool primordialWrath;
		public float endurance;

		public override void ResetEffects(NPC npc){
			primordialWrath = false;
			endurance = 0f;
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
	}
}
