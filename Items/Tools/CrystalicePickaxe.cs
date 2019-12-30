using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class CrystalicePickaxe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Pickaxe");
		}

		public override void SetDefaults(){
			//Halfway between Gold Pickaxe and Platinum Pickaxe for most stats
			Item goldPickaxe = new Item();
			goldPickaxe.CloneDefaults(ItemID.GoldPickaxe);
			Item platinumPickaxe = new Item();
			platinumPickaxe.CloneDefaults(ItemID.PlatinumPickaxe);

			item.damage = CosmivengeonUtils.Average(goldPickaxe.damage, platinumPickaxe.damage);
			item.melee = true;
			item.width = 38;
			item.height = 38;
			item.useTime = CosmivengeonUtils.Average(goldPickaxe.useTime, platinumPickaxe.useTime);
			item.useAnimation = CosmivengeonUtils.Average(goldPickaxe.useAnimation, platinumPickaxe.useAnimation);
			item.pick = CosmivengeonUtils.Average(goldPickaxe.pick, platinumPickaxe.pick);
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = CosmivengeonUtils.Average(goldPickaxe.knockBack, platinumPickaxe.knockBack);
			item.value = Item.sellPrice(silver: 25);
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Frostbite.FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
