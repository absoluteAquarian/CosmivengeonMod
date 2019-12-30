using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class FrostHornWhole : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Frost Demon's Horn: Whole");
			Description.SetDefault("Movement speed and damage dealt are increased, but defense is lowered.");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<CosmivengeonPlayer>().frostHorn_Broken = false;
		}
	}
}
