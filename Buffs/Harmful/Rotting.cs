using CosmivengeonMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Harmful{
	public class Rotting : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Rotting");
			Description.SetDefault("The Corruption weakens your body");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<BuffPlayer>().rotting = true;
		}

		public override void Update(NPC npc, ref int buffIndex){
			//This buff shouldn't be applied to NPCs
			npc.DelBuff(npc.FindBuffIndex(Type));
			buffIndex--;
		}
	}
}
