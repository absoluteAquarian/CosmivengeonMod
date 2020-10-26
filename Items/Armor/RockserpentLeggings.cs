using CosmivengeonMod.Items.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Legs)]
	public class RockserpentLeggings : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Leggings");
			Tooltip.SetDefault("Movement speed increased by 10%");
		}

		public override void SetDefaults(){
			item.width = 22;
			item.height = 18;
			item.rare = ItemRarityID.Orange;
			item.defense = 6;
			item.value = Item.sellPrice(gold: 1, silver: 85);
		}

		public override void UpdateEquip(Player player){
			player.moveSpeed *= 1.1f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddRecipeGroup("Cosmivengeon: Evil Drops", 4);
			recipe.AddRecipeGroup("IronBar", 8);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}