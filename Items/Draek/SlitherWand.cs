using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class SlitherWand : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Slither Wand");
			Tooltip.SetDefault("Calls forth a lesser wyrm, which launches from the ground at the cursor's x-position.\nWyrm will cut through anything in its path, before falling back into the ground.");
		}

		public override void SetDefaults(){
			item.autoReuse = false;
			item.rare = ItemRarityID.Green;
			item.mana = 24;
			item.UseSound = SoundID.Item70;
			item.noMelee = true;
			item.useStyle = 5;
			item.damage = 40;
			item.useAnimation = 45;
			item.useTime = 45;
			item.width = 28;
			item.height = 30;
			item.shoot = ModContent.ProjectileType<Projectiles.Weapons.SlitherWandProjectile_Head>();
			item.scale = 1f;
			item.shootSpeed = 0f;
			item.knockBack = 3f;
			item.magic = true;
			item.value = Item.sellPrice(0, 2, 50, 0);
		}
	}
}
