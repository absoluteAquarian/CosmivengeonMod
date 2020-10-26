using CosmivengeonMod.Items.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Body)]
	public class RockserpentChestplate : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Chestplate");
			Tooltip.SetDefault("Makes the wearer immune to [c/274e13:Poisoned]");
		}

		public override void SetDefaults(){
			item.width = 22;
			item.height = 20;
			item.rare = ItemRarityID.Orange;
			item.defense = 7;
			item.value = Item.sellPrice(gold: 2, silver: 15);
		}

		public override void UpdateEquip(Player player){
			player.buffImmune[BuffID.Poisoned] = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 20);
			recipe.AddRecipeGroup("Cosmivengeon: Evil Drops", 5);
			recipe.AddRecipeGroup("IronBar", 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
