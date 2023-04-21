using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Crystalice {
	public class CrystaliceWand : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Wand");
			Tooltip.SetDefault("Casts a slow-moving ice bolt that explodes" +
				"\ninto shards upon contact. Attacks have" +
				"\na chance to inflict frostburn.");
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults() {
			Item.DamageType = DamageClass.Magic;
			Item.damage = 16;
			Item.knockBack = 5f;
			Item.value = Item.sellPrice(silver: 5, copper: 25);
			Item.width = 42;
			Item.height = 42;
			Item.scale = 0.8f;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.autoReuse = false;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.UseSound = SoundID.Item28;
			Item.rare = ItemRarityID.Blue;
			Item.mana = 15;
			Item.shootSpeed = 9.1f;
			Item.shoot = ModContent.ProjectileType<CrystaliceWandProjectile>();
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>());
			recipe.AddIngredient(ItemID.SnowBlock, 10);
			recipe.AddIngredient(ItemID.IceBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
