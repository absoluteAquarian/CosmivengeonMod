using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek {
	public class EarthBolt : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Earthen Tome");
			Tooltip.SetDefault("Casts a slow-moving bolt of green energy.\nBolts have a chance to poison foes, and can pierce up to 3 enemies at once");
		}

		public override void SetDefaults() {
			Item.autoReuse = true;
			Item.rare = ItemRarityID.Green;
			Item.mana = 9;
			Item.UseSound = SoundID.Item21;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.damage = 19;
			Item.useAnimation = 22;
			Item.useTime = 22;
			Item.width = 28;
			Item.height = 30;
			Item.shoot = ModContent.ProjectileType<EarthBoltProjectile>();
			Item.scale = 0.9f;
			Item.shootSpeed = 7f;
			Item.knockBack = 5f;
			Item.DamageType = DamageClass.Magic;
			Item.value = Item.sellPrice(0, 2, 50, 0);
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
