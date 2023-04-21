using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Draek{
	[AutoloadEquip(EquipType.Shield)]
	public class SnakeShield : ModItem{
		public static int BaseDamage = 65;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Shield of the Serpent");
			Tooltip.SetDefault("Allows user to perform serpentine dashes" +
				"\nDashes travel twice as far as the Shield of Cthulhu’s dash" +
				"\nIf an enemy is bashed, they will be inflicted the [c/274e13:Poisoned] and [c/aa3300:Primordial Wrath]" +
				"\n  debuffs for several seconds");
		}

		public override void SetDefaults(){
			Item.rare = ItemRarityID.Expert;
			Item.width = 30;
			Item.height = 32;
			Item.accessory = true;
			Item.expert = true;
			Item.damage = BaseDamage;
			Item.defense = 6;
			Item.DamageType = DamageClass.Melee/* tModPorter Suggestion: Consider MeleeNoSpeed for no attack speed scaling */;
			Item.knockBack = 10.2f;
			Item.value = Item.sellPrice(gold: 2, silver: 15);
		}

		public override bool AllowPrefix(int pre)
			=> pre == PrefixID.Hard || pre == PrefixID.Guarding || pre == PrefixID.Armored || pre == PrefixID.Warding
				|| pre == PrefixID.Precise || pre == PrefixID.Lucky
				|| pre == PrefixID.Jagged || pre == PrefixID.Spiked || pre == PrefixID.Angry || pre == PrefixID.Menacing
				|| pre == PrefixID.Brisk || pre == PrefixID.Hasty || pre == PrefixID.Fleeting || pre == PrefixID.Quick2
				|| pre == PrefixID.Wild || pre == PrefixID.Rash || pre == PrefixID.Intrepid || pre == PrefixID.Violent
				|| pre == PrefixID.Arcane;

		public override void UpdateAccessory(Player player, bool hideVisual){
			SnakeShieldPlayer mp = player.GetModPlayer<SnakeShieldPlayer>();

			//If the dash is not active, immediately return so we don't do any of the logic for it
			if(!mp.DashActive)
				return;

			//This is where we set the afterimage effect.  You can replace these two lines with whatever you want to happen during the dash
			//Some examples include:  spawning dust where the player is, adding buffs, making the player immune, etc.
			//Here we take advantage of "player.eocDash" and "player.armorEffectDrawShadowEOCShield" to get the Shield of Cthulhu's afterimage effect
			player.eocDash = mp.DashTimer;
			player.armorEffectDrawShadowEOCShield = true;

			player.dash = 0;

			mp.CheckSoCHitEffect();

			//If the dash has just started, apply the dash velocity in whatever direction we wanted to dash towards
			if(mp.DashTimer == SnakeShieldPlayer.MAX_DASH_TIMER){
				Vector2 newVelocity = player.velocity;
					
				//Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
				if((mp.DashDir == SnakeShieldPlayer.DashLeft && player.velocity.X > -mp.DashVelocity) || (mp.DashDir == SnakeShieldPlayer.DashRight && player.velocity.X < mp.DashVelocity)){
					//X-velocity is set here
					int dashDirection = mp.DashDir == SnakeShieldPlayer.DashRight ? 1 : -1;
					newVelocity.X = dashDirection * mp.DashVelocity;
				}

				player.velocity = newVelocity;
			}

			//Decrement the timers
			mp.DashTimer--;
			mp.DashDelay--;

			if(mp.DashDelay == 0){
				//The dash has ended.  Reset the fields
				mp.DashDelay = SnakeShieldPlayer.MAX_DASH_DELAY;
				mp.DashTimer = SnakeShieldPlayer.MAX_DASH_TIMER;
				mp.DashActive = false;
				mp.HitNPCIndex = -1;
			}
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<JewelOfOronitus>());
			recipe.AddIngredient(ItemID.EoCShield);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 30);
			recipe.AddRecipeGroup(CoreMod.RecipeGroups.EvilDrops, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}
