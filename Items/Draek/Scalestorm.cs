﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class Scalestorm : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Scalestorm");
			Tooltip.SetDefault("Description to be added.");
		}

		public override void SetDefaults() {
			item.damage = 30;
			item.ranged = true;
			item.width = 31;
			item.height = 81;
			item.scale = 0.8f;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 4;
			item.value = Item.sellPrice(0, 2, 50, 0);
			item.rare = 2;
			item.UseSound = SoundID.Item15;
			item.autoReuse = false;
			item.shoot = 10;		//Needed to shoot projectiles
			item.shootSpeed = 24f;
			item.useAmmo = AmmoID.Arrow;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(mod.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
