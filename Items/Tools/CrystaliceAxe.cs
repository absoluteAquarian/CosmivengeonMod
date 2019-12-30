using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class CrystaliceAxe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Axe");
		}

		public override void SetDefaults(){
			//Halfway between Gold Pickaxe and Platinum Axe for most stats
			Item goldAxe = new Item();
			goldAxe.CloneDefaults(ItemID.GoldAxe);
			Item platinumAxe = new Item();
			platinumAxe.CloneDefaults(ItemID.PlatinumAxe);

			item.damage = CosmivengeonUtils.Average(goldAxe.damage, platinumAxe.damage);
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = CosmivengeonUtils.Average(goldAxe.useTime, platinumAxe.useTime);
			item.useAnimation = CosmivengeonUtils.Average(goldAxe.useAnimation, platinumAxe.useAnimation);
			item.axe = CosmivengeonUtils.Average(goldAxe.axe, platinumAxe.axe);
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = CosmivengeonUtils.Average(goldAxe.knockBack, platinumAxe.knockBack);
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
