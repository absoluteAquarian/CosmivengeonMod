﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Trophies{
	public class FrostbiteTrophy : ModItem{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frostbite Trophy");
		}

		public override void SetDefaults() {
			item.width = 30;
			item.height = 30;
			item.maxStack = 99;
			item.useTurn = true;
			item.autoReuse = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.consumable = true;
			item.value = Item.buyPrice(gold: 5);
			item.rare = ItemRarityID.Blue;
			item.createTile = ModContent.TileType<Tiles.FrostbiteTrophy>();
		}
	}
}