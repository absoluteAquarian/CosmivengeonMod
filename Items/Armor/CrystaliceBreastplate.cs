using CosmivengeonMod.Items.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Body)]
	public class CrystaliceBreastplate : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Coat");
		}

		public override void SetDefaults(){
			item.width = 30;
			item.height = 20;
			item.defense = 4;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 16, copper: 75);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 4);
			recipe.AddIngredient(ItemID.IceBlock, 20);
			recipe.AddIngredient(ItemID.SnowBlock, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}