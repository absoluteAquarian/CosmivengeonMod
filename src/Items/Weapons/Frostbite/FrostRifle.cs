using CosmivengeonMod.DamageClasses.Desolate;
using CosmivengeonMod.Projectiles.Weapons.Frostbite;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite {
	public class FrostRifle : DesolatorItem {
		public override string ItemName => "I-SKL";

		public override string FlavourText =>
			"Fires fast, piercing icicles which inflict [c/00ffff:Frostburn] on hit" +
			"\nUses [c/00ffff:Ice Blocks] for ammo" +
			"\nOne of the legendary 10 Desolator weapons, an ancient rifle capable" +
			"\nof the same monstrous power that transformed the frozen Tundra into" +
			"\nwhat it is today.  It is unknown how Frostbite managed to discover" +
			"\nthis relic, but if it were to to ever be combined with the others of" +
			"\nits class...";

		public override void SafeSetDefaults() {
			Item.width = 60;
			Item.height = 16;
			Item.noMelee = true;
			Item.useTime = 72;
			Item.useAnimation = 72;
			Item.shoot = ModContent.ProjectileType<FrostRifleProjectile>();
			Item.useAmmo = ItemID.IceBlock;
			Item.UseSound = SoundID.Item40;
			Item.shootSpeed = 14f;  //actual speed: 28px/frame
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.damage = 60;
			Item.knockBack = 8.7f;
			Item.value = Item.sellPrice(silver: 5);
			Item.rare = ItemRarityID.Blue;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-10, 0);
	}
}
