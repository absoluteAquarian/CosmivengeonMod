using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Crystalice {
	[AutoloadEquip(EquipType.Head)]
	public class CrystaliceHelmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Hood");
			Tooltip.SetDefault("Damage increased by 4%" +
				"\n+1 minion slot");
		}

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 26;
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 8, copper: 40);
		}

		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Generic) += 0.04f;
			player.maxMinions++;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
			=> body.type == ModContent.ItemType<CrystaliceBreastplate>() && legs.type == ModContent.ItemType<CrystaliceLeggings>();

		public override void UpdateArmorSet(Player player) {
			player.setBonus = "All attacks inflict [c/6fa8dc:Frostburn]";
			player.GetModPlayer<ArmorsPlayer>().setBonus_Crystalice = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
