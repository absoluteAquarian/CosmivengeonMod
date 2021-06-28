using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global{
	public class BuffNPC : GlobalNPC{
		public override bool InstancePerEntity => true;

		public bool primordialWrath;

		public override void UpdateLifeRegen(NPC npc, ref int damage){
			if(primordialWrath){
				if(npc.lifeRegen > 0)
					npc.lifeRegen = 0;
				npc.defense -= 10;
				npc.GetGlobalNPC<StatsNPC>().endurance -= 0.1f;
				npc.lifeRegen -= 15 * 2;
			}
		}

		public override void DrawEffects(NPC npc, ref Color drawColor){
			if(primordialWrath)
				drawColor = MiscUtils.Blend(drawColor, Color.DarkRed);
		}
	}
}
