using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Crystalice {
	public class CrystaliceShard : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Shard");
		}

		public override void SetDefaults() {
			Item.shootSpeed = CrystaliceShardProjectile.MAX_VELOCITY;
			Item.damage = 10;
			Item.knockBack = 3.1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useAnimation = 18;
			Item.useTime = 18;
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 999;
			Item.rare = ItemRarityID.Blue;

			Item.consumable = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
			Item.DamageType = DamageClass.Throwing;

			Item.UseSound = SoundID.Item1;
			Item.value = Item.sellPrice(0, 0, 0, 50);

			Item.shoot = ModContent.ProjectileType<CrystaliceShardProjectile>();
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe(100);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
