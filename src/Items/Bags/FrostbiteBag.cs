using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.Items.Weapons.Frostbite;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Utility;
using Terraria;
using Terraria.GameContent.ItemDropRules;
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

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.CoinsBasedOnNPCValue(ModContent.NPCType<Frostbite>()));

			Frostbite.AddDrops(itemLoot, restrictNormalDrops: false);

			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<EyeOfTheBlizzard>()));
			itemLoot.Add(ItemDropRule.ByCondition(new LootConditions.DesolationMode(), ModContent.ItemType<FrostRifle>()));
		}
	}
}
