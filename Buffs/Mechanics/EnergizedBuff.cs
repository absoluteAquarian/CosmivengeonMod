using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Mechanics{
	public class EnergizedBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Energized");
			Description.SetDefault("Movement speed and attack speed are increased");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
	}
}
