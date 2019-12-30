﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.SwordUpgrade{
	public class BreadSword : ModItem{
		public override bool OnlyShootOnSwing => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terracrust Blade");
			Tooltip.SetDefault("Fires a triple spread of earthen beams with slight homing affinities." +
				"\nAttacks have a guaranteed chance to poison enemies.");
		}

		public override void SetDefaults(){
			item.damage = 43;
			item.melee = true;
			item.useTurn = false;
			item.width = 58;
			item.height = 58;
			item.useTime = 36;
			item.useAnimation = 26;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 7f;
			item.value = Item.sellPrice(gold: 3);
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
			item.shoot = 10;
			item.shootSpeed = 12f;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<CrystaliceSword>());
			recipe.AddIngredient(ModContent.ItemType<Draek.DraekScales>(), 15);
			recipe.AddIngredient(ItemID.Emerald, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			type = ModContent.ProjectileType<Projectiles.SwordUpgrade.BreadSwordProjectile>();
			damage = (int)(item.damage * 0.6667f);

			for(int i = -1; i < 2; i++)
				Projectile.NewProjectile(position, new Vector2(speedX, speedY).RotatedBy(i * MathHelper.ToRadians(10f)), type, damage, knockBack, item.owner, item.shootSpeed);

			Main.PlaySound(SoundID.Item43.WithVolume(0.4f).WithPitchVariance(0.3f), position);

			return false;
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit){
			target.AddBuff(BuffID.Poisoned, 8 * 60);
		}
	}
}