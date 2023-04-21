using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Rockserpent{
	[AutoloadEquip(EquipType.Legs)]
	public class RockserpentLeggings : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Leggings");
			Tooltip.SetDefault("Movement speed increased by 10%");
		}

		public override void SetDefaults(){
			Item.width = 22;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.defense = 6;
			Item.value = Item.sellPrice(gold: 1, silver: 85);
		}

		public override void UpdateEquip(Player player){
			player.moveSpeed *= 1.1f;
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilDrops, 4);
			recipe.AddRecipeGroup("IronBar", 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
