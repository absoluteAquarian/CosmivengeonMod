using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
	public class EarthBolt : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Earthen Tome");
			Tooltip.SetDefault("Casts a slow-moving bolt of green energy.\nBolts have a chance to poison foes, and can pierce up to 3 enemies at once");
		}

		public override void SetDefaults(){
			item.autoReuse = true;
			item.rare = ItemRarityID.Green;
			item.mana = 9;
			item.UseSound = SoundID.Item21;
			item.noMelee = true;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.damage = 19;
			item.useAnimation = 22;
			item.useTime = 22;
			item.width = 28;
			item.height = 30;
			item.shoot = ModContent.ProjectileType<EarthBoltProjectile>();
			item.scale = 0.9f;
			item.shootSpeed = 7f;
			item.knockBack = 5f;
			item.magic = true;
			item.value = Item.sellPrice(0, 2, 50, 0);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ModContent.ItemType<RaechonShell>());
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
