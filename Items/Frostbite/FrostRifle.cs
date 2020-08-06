using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class FrostRifle : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("I-SKL");
			Tooltip.SetDefault("Fires fast, piercing icicles which inflict [c/00ffff:Frostburn] on hit" +
				"\nUses [c/00ffff:Ice Blocks] for ammo");
		}

		public override void SetDefaults(){
			item.width = 60;
			item.height = 16;
			item.ranged = true;
			item.noMelee = true;
			item.useTime = 45;
			item.useAnimation = 45;
			item.shoot = ModContent.ProjectileType<Projectiles.Weapons.Frostbite.FrostRifleProjectile>();
			item.useAmmo = ItemID.IceBlock;
			item.UseSound = SoundID.Item40;
			item.shootSpeed = 14f;	//actual speed: 28px/frame
			item.useStyle = ItemUseStyleID.HoldingOut;
			item.damage = 32;
			item.knockBack = 8.7f;
			item.value = Item.sellPrice(silver: 5);
			item.rare = ItemRarityID.Blue;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
	}
}
