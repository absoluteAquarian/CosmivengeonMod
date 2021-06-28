using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite{
	public class SnowballFlail : ModItem{
		public override bool CloneNewInstances => true;

		public const float ProjectileVelocity = 12f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("The Snowball");
			Tooltip.SetDefault("Almost certainly banned in an elementary school playground");
		}

		public override void SetDefaults(){
			item.CloneDefaults(ItemID.BallOHurt);
			item.useAnimation = 45;
			item.useTime = 45;
			item.knockBack = 5f;
			item.width = 40;
			item.height = 40;
			item.damage = 17;
			item.scale = 1f;
			item.shoot = ProjectileID.PurificationPowder;
			item.shootSpeed = ProjectileVelocity;
			item.rare = ItemRarityID.Blue;
			item.value = Item.sellPrice(silver: 30);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			TooltipLine nameLine = tooltips.Find(t => t.Name == "ItemName");

			//If we've got a prefix, move it to between the "The" and "Snowball"
			if(item.prefix > 0){
				List<string> text = item.Name.Split(' ').ToList();
				text.Insert(1, Lang.prefix[item.prefix].Value);
				nameLine.text = string.Join(" ", text.ToArray());
			}
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			type = ModContent.ProjectileType<SnowballFlailProjectile>();
			damage = (int)(damage * 0.72f);
			return true;
		}
	}
}
