using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Stamina{
	public class ExhaustedDebuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Exhausted");
			Description.SetDefault("Movement speed, jump height and attack speed are lowered.");
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
	}
}
