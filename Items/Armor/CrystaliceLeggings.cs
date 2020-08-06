using CosmivengeonMod.Items.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Armor{
	[AutoloadEquip(EquipType.Legs)]
	public class CrystaliceLeggings : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Boots");
			Tooltip.SetDefault("Slows movement speed by 15%" +
				"\nAllows for gliding on ice");
		}

		public override void SetDefaults(){
			item.width = 18;
			item.height = 12;
			item.defense = 3;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 11, copper: 90);
		}

		public override void UpdateEquip(Player player){
			player.moveSpeed *= 0.85f;
			player.iceSkate = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 3);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddIngredient(ItemID.SnowBlock, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
