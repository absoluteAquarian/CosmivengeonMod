using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class CrystaliceBow : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Bow");
			Tooltip.SetDefault("A sleek, frozen bow." +
				"\nCauses all arrows fired from it to inflict Frostburn.");
		}

		public override void SetDefaults(){
			item.ranged = true;
			item.damage = 18;
			item.knockBack = 4f;
			item.useTime = 25;
			item.useAnimation = 25;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.width = 28;
			item.height = 58;
			item.scale = 0.8f;
			item.noMelee = true;
			item.useAmmo = AmmoID.Arrow;
			item.shoot = 10;
			item.shootSpeed = 10f;
			item.UseSound = SoundID.Item5;
			item.value = Item.sellPrice(silver: 1, copper: 75);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
