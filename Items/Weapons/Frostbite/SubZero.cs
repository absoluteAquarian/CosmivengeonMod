using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.Frostbite{
	public class SubZero : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Sub-Zero");
			Tooltip.SetDefault("An insanely fast, short-ranged attack" +
				"\nTargets explode into a burst of ice sparks when are killed by this weapon");
		}

		public override void SetDefaults(){
			Item.DamageType = DamageClass.Melee/* tModPorter Suggestion: Consider MeleeNoSpeed for no attack speed scaling */;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.damage = 17;
			Item.knockBack = 3.1f;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.width = 22;
			Item.height = 20;
			Item.scale = 1.15f;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(silver: 5, copper: 40);
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit){
			target.AddBuff(BuffID.Frostburn, 4 * 60);
		}
	}
}
