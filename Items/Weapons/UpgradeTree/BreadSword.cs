using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Weapons.UpgradeTree{
	public class BreadSword : ModItem{
		public override bool OnlyShootOnSwing => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Terracrust Blade");
			Tooltip.SetDefault("Attacks have a guaranteed chance to [c/274e13:Poison] enemies and only a" +
				"\nslight chance to inflict [c/00dddd:Frostburn] and [c/aa3300:Primordial Wrath].");
		}

		public override void SetDefaults(){
			item.damage = 30;
			item.melee = true;
			item.useTurn = false;
			item.width = 58;
			item.height = 58;
			item.useTime = 36;
			item.useAnimation = 26;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.knockBack = 5.5f;
			item.value = Item.sellPrice(gold: 3);
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
			item.autoReuse = true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<CrystaliceSword>());
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddIngredient(ItemID.Emerald, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
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
