using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class SlitherWand : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Slither Wand");
			Tooltip.SetDefault("Calls forth a lesser wyrm, which launches from the ground at the cursor's x-position.\nWyrm will cut through anything in its path, before falling back into the ground.");
		}

		public override void SetDefaults(){
			item.autoReuse = false;
			item.rare = ItemRarityID.Green;
			item.mana = 24;
			item.UseSound = SoundID.Item70;
			item.noMelee = true;
			item.useStyle = 5;
			item.damage = 40;
			item.useAnimation = 45;
			item.useTime = 45;
			item.width = 28;
			item.height = 30;
			item.shoot = ModContent.ProjectileType<Projectiles.Weapons.SlitherWandProjectile_Head>();
			item.knockBack = 3f;
			item.magic = true;
			item.value = Item.sellPrice(0, 2, 50, 0);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override Vector2? HoldoutOffset() => new Vector2(-5, -8);
	}
}
