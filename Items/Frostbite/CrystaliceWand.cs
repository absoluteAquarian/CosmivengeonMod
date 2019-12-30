using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class CrystaliceWand : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Wand");
			Tooltip.SetDefault("Casts a slow-moving ice bolt that explodes" +
				"\ninto shards upon contact. Attacks have" +
				"\na chance to inflict frostburn.");
			Item.staff[item.type] = true;
		}

		public override void SetDefaults(){
			item.magic = true;
			item.damage = 15;
			item.knockBack = 5f;
			item.value = Item.sellPrice(silver: 5, copper: 25);
			item.width = 42;
			item.height = 42;
			item.scale = 0.8f;
			item.useTime = 18;
			item.useAnimation = 18;
			item.autoReuse = false;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.noMelee = true;
			item.UseSound = SoundID.Item28;
			item.rare = ItemRarityID.Blue;
			item.mana = 13;
			item.shootSpeed = 7f;
			item.shoot = ModContent.ProjectileType<Projectiles.Weapons.Frostbite.CrystaliceWandProjectile>();
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
