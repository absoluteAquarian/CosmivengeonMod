using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class FrostHornBroken : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Frost Demon's Horn: Broken");
			Description.SetDefault("Movement speed is decreased.");
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<CosmivengeonPlayer>().frostHorn_Broken = true;
		}
	}
}
