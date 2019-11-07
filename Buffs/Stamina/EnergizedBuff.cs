using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Stamina{
	public class EnergizedBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Energized");
			Description.SetDefault("Movement speed and attack speed increased.");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
	}
}