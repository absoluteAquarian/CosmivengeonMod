using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Crystalice{
	public class CrystaliceShard : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Shard");
		}

		public override void SetDefaults(){
			item.shootSpeed = CrystaliceShardProjectile.MAX_VELOCITY;
			item.damage = 10;
			item.knockBack = 3.1f;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useAnimation = 18;
			item.useTime = 18;
			item.width = 30;
			item.height = 30;
			item.maxStack = 999;
			item.rare = ItemRarityID.Blue;

			item.consumable = true;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = false;
			item.thrown = true;

			item.UseSound = SoundID.Item1;
			item.value = Item.sellPrice(0, 0, 0, 50);

			item.shoot = ModContent.ProjectileType<CrystaliceShardProjectile>();
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 100);
			recipe.AddRecipe();
		}
	}
}
