using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Harmful{
	public class PrimordialWrath : ModBuff{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Primordial Wrath");
			Description.SetDefault("Your skin is melting off.");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;	
			Main.buffNoSave[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex){
			npc.GetGlobalNPC<BuffNPC>().primordialWrath = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<BuffPlayer>().primordialWrath = true;
		}
	}
}
