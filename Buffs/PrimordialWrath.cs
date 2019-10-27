using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class PrimordialWrath : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Primordial Wrath");
			Description.SetDefault("Your skin is melting off.");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;	
			Main.buffNoSave[Type] = true;
			longerExpertDebuff = true;
		}

		public override void Update(NPC npc, ref int buffIndex){
			npc.GetGlobalNPC<CosmivengeonGlobalNPC>().primordialWrath = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<CosmivengeonPlayer>().primordialWrath = true;
		}
	}
}
