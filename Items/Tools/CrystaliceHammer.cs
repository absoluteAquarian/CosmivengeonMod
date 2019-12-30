using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class CrystaliceHammer : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Hammer");
		}

		public override void SetDefaults(){
			//Halfway between Gold Pickaxe and Platinum Hammer for most stats
			Item goldHammer = new Item();
			goldHammer.CloneDefaults(ItemID.GoldHammer);
			Item platinumHammer = new Item();
			platinumHammer.CloneDefaults(ItemID.PlatinumHammer);

			item.damage = CosmivengeonUtils.Average(goldHammer.damage, platinumHammer.damage);
			item.melee = true;
			item.width = 40;
			item.height = 40;
			item.useTime = CosmivengeonUtils.Average(goldHammer.useTime, platinumHammer.useTime);
			item.useAnimation = CosmivengeonUtils.Average(goldHammer.useAnimation, platinumHammer.useAnimation);
			item.hammer = CosmivengeonUtils.Average(goldHammer.hammer, platinumHammer.hammer);
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = CosmivengeonUtils.Average(goldHammer.knockBack, platinumHammer.knockBack);
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
