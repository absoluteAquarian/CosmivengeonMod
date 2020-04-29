using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons.Draek;

namespace CosmivengeonMod.Items.Draek{
	public class RockslideYoyo : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Draek/Rockslide";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rockslide");
			Tooltip.SetDefault("Casts out a medium-length earth yoyo that has a chance to poison foes on hit.");

			// These are all related to gamepad controls and don't seem to affect anything else
			ItemID.Sets.Yoyo[item.type] = true;
			ItemID.Sets.GamepadExtraRange[item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[item.type] = true;
		}

		public override void SetDefaults(){
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.width = 30;
			item.height = 26;
			item.useAnimation = 25;
			item.useTime = 25;
			item.shootSpeed = 16f;
			item.knockBack = 3f;
			item.damage = 20;
			item.rare = ItemRarityID.Green;

			item.melee = true;
			item.channel = true;
			item.noMelee = true;
			item.noUseGraphic = true;

			item.UseSound = SoundID.Item1;
			item.value = Item.sellPrice(0, 0, 35, 0);
			item.shoot = ModContent.ProjectileType<RockslideProjectile>();
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
