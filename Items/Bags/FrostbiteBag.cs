using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Bags {
	public class FrostbiteBag : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Treasure Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
		}

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 40;
			Item.height = 36;
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override bool CanRightClick() => true;

		public override void OpenBossBag(Player player) {
			if (Main.hardMode)
				player.TryGettingDevArmor();

			player.QuickSpawnItem(ModContent.ItemType<EyeOfTheBlizzard>());

			Frostbite.NormalModeDrops(player: player, quickSpawn: true);
		}

		public override int BossBagNPC => ModContent.NPCType<Frostbite>();
	}
}
