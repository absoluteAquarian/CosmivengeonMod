using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Dice;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Items.Tools.Dice {
	public class DiceOfFateD20 : HidableTooltip {
		public override void SaveData(TagCompound tag) {
			tag["recharge"] = rechargeTimer;
			tag["pushLuck"] = dontPushYourLuck;
			tag["unlucky"] = unluckyFactor;
		}

		public override void LoadData(TagCompound tag) {
			rechargeTimer = tag.GetInt("recharge");
			dontPushYourLuck = tag.GetInt("pushLuck");
			unluckyFactor = tag.GetInt("unlucky");
		}

		protected override bool CloneNewInstances => true;

		public static int rechargeTimer;
		public static int dontPushYourLuck;

		public static int unluckyFactor;

		public override string ItemName => "Dice of Fate";

		private string TimeString(float timeTicks) {
			string ret = "";
			const float oneHour = 60 * 60 * 60;
			const float oneMinute = 60 * 60;

			bool hasHour = false, hasMinute = false;
			if (timeTicks >= oneHour) {
				hasHour = true;

				ret += $"{(int)(timeTicks / oneHour)}h";
				timeTicks %= oneHour;
			}

			if (timeTicks >= oneMinute) {
				if (hasHour)
					ret += " ";

				hasMinute = true;

				ret += $"{(int)(timeTicks / oneMinute)}m";
				timeTicks %= oneMinute;
			}

			if (hasHour || hasMinute)
				ret += " ";

			ret += $"{timeTicks / 60:0.#}s";

			return ret;
		}

		public override string AlwaysDisplayText => "'Are ya feelin' lucky?'" +
			(rechargeTimer > 0 ? $"\n[c/bc0000:You need to wait {TimeString(rechargeTimer)} to throw another dice.]" : "");

		public override string FlavourText => "~!~";

		public override void SetDefaults() {
			Item.useTime = Item.useAnimation = 18;
			Item.useTurn = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;

			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item1;

			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(gold: 10, silver: 15);

			Item.width = 34;
			Item.height = 34;
			Item.scale = 8f / 34;

			Item.shoot = ModContent.ProjectileType<DiceOfFateD20Dice>();
			Item.shootSpeed = 6f;

			Item.consumable = false;
			Item.ammo = Item.type;
			Item.useAmmo = Item.type;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips) {
			int line = FindCustomTooltipIndex(tooltips);

			if (line < 0)
				return;

			TooltipLine tooltip = tooltips[line];
			DicePlayer mp = Main.LocalPlayer.GetModPlayer<DicePlayer>();

			StringBuilder replacement = new();
			if (mp.badShop || mp.goodShop || mp.forceReversedGravity || mp.fishDontWantMe || (!mp.fishDontWantMe && mp.fishTimer > 0) || mp.moreIFrames || mp.noStaminaDecay || mp.extraLives > 0 || mp.godmodeTimer > 0 || mp.buffDamageTimer > 0 || mp.endlessClipTimer > 0 || mp.endlessManaTimer > 0 || mp.nebulaBuffsTimer > 0 || mp.beetleBuffsTimer > 0) {
				replacement.Append("Active effects:");

				if (mp.badShop)
					replacement.Append($"\n - Increased shop prices ({TimeString(mp.shopModifierTimer)})");
				if (mp.goodShop)
					replacement.Append($"\n - Decreased shop prices ({TimeString(mp.shopModifierTimer)})");
				if (mp.forceReversedGravity)
					replacement.Append($"\n - Forced reverse gravity ({TimeString(mp.forcedGravityTimer)})");
				if (mp.fishDontWantMe)
					replacement.Append($"\n - Decreased fishing skill ({TimeString(mp.fishTimer)})");
				else if (!mp.fishDontWantMe && mp.fishTimer > 0)
					replacement.Append($"\n - Increased fishing skill ({TimeString(mp.fishTimer)})");
				if (mp.moreIFrames)
					replacement.Append($"\n - Increased immunity frames on hit ({TimeString(mp.moreIFrameTimer)})");
				if (mp.noStaminaDecay)
					replacement.Append($"\n - No Stamina Decay ({TimeString(mp.nsdTimer)})");
				if (mp.extraLives > 0)
					replacement.Append($"\n - Extra Lives: {mp.extraLives}");
				if (mp.nebulaBuffsTimer > 0)
					replacement.Append($"\n - Nebula Boosts ({TimeString(mp.nebulaBuffsTimer)})");
				if (mp.beetleBuffsTimer > 0)
					replacement.Append($"\n - Beetle Armor Buffs ({TimeString(mp.beetleBuffsTimer)})");
				if (mp.godmodeTimer > 0)
					replacement.Append($"\n - Godmode ({TimeString(mp.godmodeTimer)})");
				if (mp.buffDamageTimer > 0)
					replacement.Append($"\n - Double damage ({TimeString(mp.buffDamageTimer)})");
				if (mp.endlessClipTimer > 0)
					replacement.Append($"\n - Infinite ammo ({TimeString(mp.endlessClipTimer)})");
				if (mp.endlessManaTimer > 0)
					replacement.Append($"\n - Infinite mana ({TimeString(mp.endlessManaTimer)})");
			} else
				replacement.Append("Active effects:\n - none");

			tooltip.Text = tooltip.Text.Replace("~!~", replacement.ToString());
		}

		public override void UpdateInventory(Player player) {
			UpdateRecharge();
		}

		public override void PostUpdate() {
			UpdateRecharge();
		}

		private void UpdateRecharge() {
			if (rechargeTimer > 0) {
				rechargeTimer--;

				if (Item.useAmmo == Item.type)
					Item.useAmmo = ModContent.ItemType<DiceOfFateFakeAmmo>();
			}

			if (rechargeTimer == 0 || Debug.debug_fastDiceOfFateRecharge) {
				Item.useAmmo = Item.type;

				if (dontPushYourLuck == 0) {
					unluckyFactor = 0;
				} else
					dontPushYourLuck--;
			}
		}

		public override bool CanUseItem(Player player) => rechargeTimer <= 0 || Debug.debug_fastDiceOfFateRecharge;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (dontPushYourLuck > 0)
				unluckyFactor++;

			// Gotta wait 10 minutes
			rechargeTimer = Debug.debug_fastDiceOfFateRecharge ? 0 : 10 * 60 * 60;
			dontPushYourLuck = 10 * 60;

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, unluckyFactor);

			return false;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.Tier4Bars, 20);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilBars, 12);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilDrops, 10);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.WeirdPlant, 4);
			recipe.AddIngredient(ItemID.LifeCrystal, 5);
			recipe.AddIngredient(ItemID.ManaCrystal, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
