using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.UpgradeTree{
	public class BreadSword : ModItem{
		public override bool OnlyShootOnSwing/* tModPorter Note: Removed. If you returned true, set Item.useTime to a multiple of Item.useAnimation */ => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terracrust Blade");
			Tooltip.SetDefault("Attacks have a guaranteed chance to [c/274e13:Poison] enemies and only a" +
				"\nslight chance to inflict [c/00dddd:Frostburn] and [c/aa3300:Primordial Wrath].");
		}

		public override void SetDefaults(){
			Item.damage = 30;
			Item.DamageType = DamageClass.Melee/* tModPorter Suggestion: Consider MeleeNoSpeed for no attack speed scaling */;
			Item.useTurn = false;
			Item.width = 58;
			Item.height = 58;
			Item.useTime = 36;
			Item.useAnimation = 26;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5.5f;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<CrystaliceSword>());
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ItemID.Emerald, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}

		public override void OnHitNPC(Player player, NPC target, int damage, float knockBack, bool crit) {
			target.AddBuff(BuffID.Poisoned, 8 * 60);

			if(Main.rand.NextFloat() < 0.125f)
				target.AddBuff(BuffID.Frostburn, 3 * 60);

			if(Main.rand.NextFloat() < 0.05f)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 2 * 60);

			var stamina = player.GetModPlayer<StaminaPlayer>().stamina;
			if(!stamina.Active)
				stamina.Add(200, true);
		}
	}
}
