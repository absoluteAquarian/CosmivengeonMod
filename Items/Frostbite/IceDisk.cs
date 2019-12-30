using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons.Frostbite;

namespace CosmivengeonMod.Items.Frostbite{
	public class IceDisk : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Abominable's Animosity");
			Tooltip.SetDefault("Launches a frozen disk that homes in on" +
				"\nthe cursor's position.  Once it reaches" +
				"\nthe cursor, it shoots out several frozen" +
				"\nshards then splits into two smaller disks.");
		}

		public override void SetDefaults(){
			item.width = 42;
			item.height = 38;
			item.thrown = true;
			item.damage = 13;
			item.knockBack = 5.1f;
			item.rare = ItemRarityID.Blue;
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.useTime = 20;
			item.useAnimation = 20;
			item.noUseGraphic = true;
			item.autoReuse = true;
			item.noMelee = true;
			item.UseSound = SoundID.Item1;
			item.value = Item.sellPrice(silver: 1, copper: 50);
			item.shootSpeed = IceDiskProjectile.MaxVelocity;
			item.shoot = ModContent.ProjectileType<IceDiskProjectile>();
			item.channel = true;
		}

		public override bool CanUseItem(Player player)
			=> Main.projectile.Count(p => p?.active == true && p.type == ModContent.ProjectileType<IceDiskProjectile>() && p.ai[0] == 0) == 0 && Main.projectile.Count(p => p?.active == true && p.type == ModContent.ProjectileType<IceDiskProjectile>() && p.ai[0] == 1) < 3;
	}
}
