using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class SubZero : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sub-Zero");
			Tooltip.SetDefault("An insanely fast, short-ranged attack" +
				"\nTarget explodes into a burst of ice sparks when they die");
		}

		public override void SetDefaults(){
			item.melee = true;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.damage = 17;
			item.knockBack = 3.1f;
			item.useTime = 6;
			item.useAnimation = 6;
			item.width = 22;
			item.height = 20;
			item.scale = 1.15f;
			item.useTurn = true;
			item.autoReuse = true;
			item.value = Item.sellPrice(silver: 5, copper: 40);
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit){
			target.AddBuff(BuffID.Frostburn, 4 * 60);
		}
	}
}
