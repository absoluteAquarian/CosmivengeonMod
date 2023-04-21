using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class SnowballFlail : ModItem {
		protected override bool CloneNewInstances => true;

		public const float ProjectileVelocity = 12f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("The Snowball");
			Tooltip.SetDefault("Almost certainly banned in an elementary school playground");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.BallOHurt);
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.knockBack = 5f;
			Item.width = 40;
			Item.height = 40;
			Item.damage = 17;
			Item.scale = 1f;
			Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = ProjectileVelocity;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 30);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			TooltipLine nameLine = tooltips.Find(t => t.Name == "ItemName");

			//If we've got a prefix, move it to between the "The" and "Snowball"
			if (Item.prefix > 0) {
				List<string> text = Item.Name.Split(' ').ToList();
				text.Insert(1, Lang.prefix[Item.prefix].Value);
				nameLine.Text = string.Join(" ", text.ToArray());
			}
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			type = ModContent.ProjectileType<SnowballFlailProjectile>();
			damage = (int)(damage * 0.72f);
			return true;
		}
	}
}
