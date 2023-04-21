using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Rockserpent{
	[AutoloadEquip(EquipType.Body)]
	public class RockserpentChestplate : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockserpent Chestplate");
			Tooltip.SetDefault("Makes the wearer immune to [c/274e13:Poisoned]");
		}

		public override void SetDefaults(){
			Item.width = 22;
			Item.height = 20;
			Item.rare = ItemRarityID.Orange;
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 2, silver: 15);
		}

		public override void UpdateEquip(Player player){
			player.buffImmune[BuffID.Poisoned] = true;
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 20);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilDrops, 5);
			recipe.AddRecipeGroup("IronBar", 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
