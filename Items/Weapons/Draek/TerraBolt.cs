using CosmivengeonMod.DamageClasses.Desolate;
using CosmivengeonMod.Projectiles.Weapons.Draek;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Draek{
	public class TerraBolt : DesolatorItem{
		public override string ItemName => "Terra Bolt";

		public override string FlavourText =>
			"Launches a devastating barrage of earthen stones. Rocks will curse" +
			"\nenemies with Primordial Wrath, lowering their defense and damaging" +
			"\nthem over time." +
			"\nRight click to shoot a shotgun burst of rocks." +
			"\nOne of the legendary 10 Desolator weapons, an ancient stone capable" +
			"\nof ripping apart the planet itself if used in the right hands.  How" +
			"\nDraek managed to stumble across something this powerful is unknown," +
			"\nbut if it were to ever be combined with the others of its class...";

		public override void SafeSetDefaults(){
			item.damage = 18;
			item.width = 42;
			item.height = 24;
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = 6;  //Custom use style
			item.channel = true;
			item.mana = 3;
			item.noMelee = true;
			item.noUseGraphic = true;
			item.knockBack = 2f;
			item.value = Item.sellPrice(0, 5, 0, 0);
			item.rare = ItemRarityID.Orange;
			item.shoot = ModContent.ProjectileType<TerraBoltCharge>();
			item.shootSpeed = 5f;

			//charge sound: Item15
			//shoot sound: Item84
		}

		public override bool UseItemFrame(Player player){
			//Only one Charge projectile should be out at once
			//Therefore, we can just check for the first active one and use it
			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile proj = Main.projectile[i];
				if(proj.active && proj.owner == player.whoAmI && proj.modProjectile is TerraBoltCharge charge){
					player.bodyFrame.Y = charge.BodyIndex() * player.bodyFrame.Height;
					return true;
				}
			}

			return false;
		}

		public override bool HoldItemFrame(Player player)
			=> UseItemFrame(player);

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			speedX = 0;
			speedY = 0;
			return true;
		}

		public override bool SafeCanUseItem(Player player) => player.ownedProjectileCounts[item.shoot] < 1;
	}
}
