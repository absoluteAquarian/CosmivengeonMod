﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class TerraBolt : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Draek/Terra_Bolt";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terra Bolt");
			Tooltip.SetDefault("[c/6a00aa:Desolator]" +
				"\nLaunches a devastating barrage of earthen stones. Rocks will curse" +
				"\nenemies with Primordial Wrath, lowering their defense and damaging" +
				"\nthem over time." +
				"\nScales in damage with each boss defeated" +
				"\nOne of the legendary 10 Desolator weapons, an ancient stone capable" +
				"\nof ripping apart the planet itself if used in the right hands.  How" +
				"\nDraek managed to stumble across something this powerful is unknown," +
				"\nbut if it were to ever be combined with the others of its class..." +
				"\nDesolation Mode Item");
		}

		public override void SetDefaults(){
			item.damage = 10;
			item.width = 42;
			item.height = 24;
			item.useTime = 6;
			item.useAnimation = 6;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 2f;
			item.value = Item.sellPrice(0, 5, 0, 0);
			item.rare = 3;
			item.UseSound = SoundID.Item11;
			item.autoReuse = true;
			item.shoot = ModContent.ProjectileType<TerraBoltProjectile>();
			item.shootSpeed = 30f;
		}
	}
}