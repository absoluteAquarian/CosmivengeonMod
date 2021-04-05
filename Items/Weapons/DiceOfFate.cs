using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Weapons;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Items.Weapons{
	public class DiceOfFate : HidableTooltip{
		public override TagCompound Save()
			=> new TagCompound(){
				["recharge"] = rechargeTimer,
				["pushLuck"] = dontPushYourLuck,
				["unlucky"] = unluckyFactor
			};

		public override void Load(TagCompound tag){
			rechargeTimer = tag.GetInt("recharge");
			dontPushYourLuck = tag.GetInt("pushLuck");
			unluckyFactor = tag.GetInt("unlucky");
		}

		public override bool CloneNewInstances => true;

		public static int rechargeTimer;
		public static int dontPushYourLuck;

		public static int unluckyFactor;

		public override string ItemName => "Dice of Fate";

		private string TimeString(float timeTicks){
			string ret = "";
			const float oneHour = 60 * 60 * 60;
			const float oneMinute = 60 * 60;
			
			bool hasHour = false, hasMinute = false;
			if(timeTicks >= oneHour){
				hasHour = true;

				ret += $"{(int)(timeTicks / oneHour)}h";
				timeTicks %= oneHour;
			}

			if(timeTicks >= oneMinute){
				if(hasHour)
					ret += " ";

				hasMinute = true;

				ret += $"{(int)(timeTicks / oneMinute)}m";
				timeTicks %= oneMinute;
			}

			if(hasHour || hasMinute)
				ret += " ";

			ret += $"{timeTicks / 60 :0.#}s";

			return ret;
		}

		public override string AlwaysDisplayText => "'Are ya feelin' lucky?'" +
			(rechargeTimer > 0 ? $"\n[c/bc0000:You need to wait {TimeString(rechargeTimer)} to throw another dice.]" : "");

		public override string FlavourText => "~!~";

		public override void SetDefaults(){
			item.useTime = item.useAnimation = 18;
			item.useTurn = true;
			item.noUseGraphic = true;
			item.noMelee = true;

			item.useStyle = ItemUseStyleID.SwingThrow;
			item.UseSound = SoundID.Item1;

			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(gold: 10, silver: 15);

			item.width = 34;
			item.height = 34;
			item.scale = 8f / 34;

			item.shoot = ModContent.ProjectileType<DiceOfFateDice>();
			item.shootSpeed = 6f;

			item.consumable = false;
			item.ammo = item.type;
			item.useAmmo = item.type;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips){
			int line = FindCustomTooltipIndex(tooltips);

			if(line < 0)
				return;

			TooltipLine tooltip = tooltips[line];
			DicePlayer mp = Main.LocalPlayer.GetModPlayer<DicePlayer>();

			string replacement;
			if(mp.badShop || mp.goodShop || mp.forceReversedGravity || mp.fishDontWantMe || (!mp.fishDontWantMe && mp.fishTimer > 0) || mp.moreIFrames || mp.noStaminaDecay || mp.extraLives > 0 || mp.godmodeTimer > 0 || mp.buffDamageTimer > 0 || mp.endlessClipTimer > 0 || mp.endlessManaTimer > 0){
				replacement = "Active effects:";

				if(mp.badShop)
					replacement += $"\n - Increased shop prices ({TimeString(mp.shopModifierTimer)})";
				if(mp.goodShop)
					replacement += $"\n - Decreased shop prices ({TimeString(mp.shopModifierTimer)})";
				if(mp.forceReversedGravity)
					replacement += $"\n - Forced reverse gravity ({TimeString(mp.forcedGravityTimer)})";
				if(mp.fishDontWantMe)
					replacement += $"\n - Decreased fishing skill ({TimeString(mp.fishTimer)})";
				else if(!mp.fishDontWantMe && mp.fishTimer > 0)
					replacement += $"\n - Increased fishing skill ({TimeString(mp.fishTimer)})";
				if(mp.moreIFrames)
					replacement += $"\n - Increased immunity frames on hit ({TimeString(mp.moreIFrameTimer)})";
				if(mp.noStaminaDecay)
					replacement += $"\n - No Stamina Decay ({TimeString(mp.nsdTimer)})";
				if(mp.extraLives > 0)
					replacement += $"\n - Extra Lives: {mp.extraLives}";
				if(mp.godmodeTimer > 0)
					replacement += $"\n - Godmode ({TimeString(mp.godmodeTimer)})";
				if(mp.buffDamageTimer > 0)
					replacement += $"\n - Double damage ({TimeString(mp.buffDamageTimer)})";
				if(mp.endlessClipTimer > 0)
					replacement += $"\n - Infinite ammo ({TimeString(mp.endlessClipTimer)})";
				if(mp.endlessManaTimer > 0)
					replacement += $"\n - Infinite mana ({TimeString(mp.endlessManaTimer)})";
			}else
				replacement = "Active effects:\n - none";

			tooltip.text = tooltip.text.Replace("~!~", replacement);
		}

		public override void UpdateInventory(Player player){
			UpdateRecharge();
		}

		public override void PostUpdate(){
			UpdateRecharge();
		}

		private void UpdateRecharge(){
			if(rechargeTimer > 0){
				rechargeTimer--;

				if(item.useAmmo == item.type)
					item.useAmmo = ModContent.ItemType<DiceOfFateFakeAmmo>();
			}

			if(rechargeTimer == 0 || CosmivengeonMod.debug_fastDiceOfFateRecharge){
				item.useAmmo = item.type;

				if(dontPushYourLuck == 0){
					unluckyFactor = 0;
				}else
					dontPushYourLuck--;
			}
		}

		public override bool CanUseItem(Player player) => rechargeTimer <= 0 || CosmivengeonMod.debug_fastDiceOfFateRecharge;

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack){
			if(dontPushYourLuck > 0)
				unluckyFactor++;

			//Gotta wait 10 minutes
			rechargeTimer = CosmivengeonMod.debug_fastDiceOfFateRecharge ? 0 : 10 * 60 * 60;
			dontPushYourLuck = 10 * 60;

			Projectile.NewProjectile(position, new Vector2(speedX, speedY), type, damage, knockBack, player.whoAmI, unluckyFactor);

			return false;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 8);
			recipe.AddRecipeGroup(CosmivengeonMod.RecipeGroup_PreHM_Tier4, 20);
			recipe.AddRecipeGroup(CosmivengeonMod.RecipeGroup_EvilBars, 12);
			recipe.AddRecipeGroup(CosmivengeonMod.RecipeGroup_EvilDrops, 10);
			recipe.AddRecipeGroup(CosmivengeonMod.RecipeGroup_WeirdPlant, 4);
			recipe.AddIngredient(ItemID.LifeCrystal, 5);
			recipe.AddIngredient(ItemID.ManaCrystal, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
