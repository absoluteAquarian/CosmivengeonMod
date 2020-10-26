using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class Sticky : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Honeyed");
			Description.SetDefault("You are covered in honey");
			Main.debuff[Type] = true;
			Main.pvpBuff[Type] = true;	
			Main.buffNoSave[Type] = true;
		}
	}
}
