using CosmivengeonMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Mechanics {
	public class FrostHornBroken : ModBuff {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frost Demon's Horn: Broken");
			Description.SetDefault("Movement speed is decreased.");
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			player.GetModPlayer<AccessoriesPlayer>().brokenFrostHorn = true;
		}
	}
}
