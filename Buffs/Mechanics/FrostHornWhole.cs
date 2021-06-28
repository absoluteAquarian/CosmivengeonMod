using CosmivengeonMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Mechanics{
	public class FrostHornWhole : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Frost Demon's Horn: Whole");
			Description.SetDefault("Movement speed and damage dealt are increased, but defense is lowered.");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			player.GetModPlayer<AccessoriesPlayer>().brokenFrostHorn = false;
		}
	}
}
