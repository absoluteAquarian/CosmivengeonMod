using CosmivengeonMod.Buffs;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
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
			item.rare = ItemRarityID.Expert;
			item.width = 30;
			item.height = 32;
			item.accessory = true;
			item.expert = true;
			item.damage = BaseDamage;
			item.defense = 6;
			item.melee = true;
			item.knockBack = 10.2f;
			item.value = Item.sellPrice(gold: 2, silver: 15);
		}

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
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<JewelOfOronitus>());
			recipe.AddIngredient(ItemID.EoCShield);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 30);
			recipe.AddRecipeGroup(CosmivengeonMod.RecipeGroup_EvilDrops, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class SnakeShieldPlayer : ModPlayer{
		//These indicate what direction is what in the timer arrays used
		public static readonly int DashRight = 2;
		public static readonly int DashLeft = 3;

		//The direction the player is currently dashing towards.  Defaults to -1 if no dash is ocurring.
		public int DashDir = -1;

		//The fields related to the dash accessory
		public bool DashActive = false;
		public int DashDelay = MAX_DASH_DELAY;
		public int DashTimer = MAX_DASH_TIMER;
		//The initial velocity.  SoC dash has 14.5f velocity, but this dash lasts longer so it appears to be faster
		public readonly float DashVelocity = 14.5f;
		//These two fields are the max values for the delay between dashes and the length of the dash in that order
		//The time is measured in frames
		public static readonly int MAX_DASH_DELAY = 50;
		public static readonly int MAX_DASH_TIMER = 35;

		public int HitNPCIndex = -1;

		public override void ResetEffects(){
			//ResetEffects() is called not long after player.doubleTapCardinalTimer's values have been set
			
			//Check if the ExampleDashAccessory is equipped and also check against this priority:
			// If the Shield of Cthulhu, Master Ninja Gear, Tabi and/or Solar Armour set is equipped, prevent this accessory from doing its dash effect
			//The priority is used to prevent undesirable effects.
			//Without it, the player is able to use the ExampleDashAccessory's dash as well as the vanilla ones
			bool dashAccessoryEquipped = false;

			//This is the loop used in vanilla to update/check the not-vanity accessories
			for(int i = 3; i < 8 + player.extraAccessorySlots; i++){
				Item item = player.armor[i];

				//Set the flag for the ExampleDashAccessory being equipped if we have it equipped OR immediately return if any of the accessories are
				// one of the higher-priority ones
				if(item.type == ModContent.ItemType<SnakeShield>())
					dashAccessoryEquipped = true;
				else if(item.type == ItemID.EoCShield || item.type == ItemID.MasterNinjaGear || item.type == ItemID.Tabi)
					return;
			}

			//If we don't have the ExampleDashAccessory equipped or the player has the Solor armor set equipped, return immediately
			//Also return if the player is currently on a mount, since dashes on a mount look weird, or if the dash was already activated
			if(!dashAccessoryEquipped || player.setSolar || player.mount.Active || DashActive)
				return;

			//When a directional key is pressed and released, vanilla starts a 15 tick (1/4 second) timer during which a second press activates a dash
			//If the timers are set to 15, then this is the first press just processed by the vanilla logic.  Otherwise, it's a double-tap
			if(player.controlRight && player.releaseRight && player.doubleTapCardinalTimer[DashRight] < 15)
				DashDir = DashRight;
			else if(player.controlLeft && player.releaseLeft && player.doubleTapCardinalTimer[DashLeft] < 15)
				DashDir = DashLeft;
			else{
				DashDir = 0;
				return;	 //No dash was activated, return
			}

			DashActive = true;

			//Here you'd be able to set an effect that happens when the dash first activates
			//Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
			player.dash = 0;
			player.ChangeDir(DashDir == DashRight ? 1 : -1);
		}

		//Copy of the SoC's code in Player.DashMovement(), but changed to fit this accessory
		public void CheckSoCHitEffect(){
			if(HitNPCIndex < 0){
				//Vanilla uses the player's hitbox and adds 4px to each direction, then offsets it by half of the player's velocity.
				//We'll a larger component of the velocity since the dash is faster
				Rectangle collisionCheck = new Rectangle((int)(player.position.X + player.velocity.X * 2f - 4), (int)(player.position.Y + player.velocity.Y * 2f - 4), player.width + 8, player.height + 8);
				for(int i = 0; i < Main.maxNPCs; i++){
					NPC npc = Main.npc[i];
					if(npc.active && !npc.dontTakeDamage && !npc.friendly && collisionCheck.Intersects(npc.getRect()) && (npc.noTileCollide || player.CanHit(npc))){
						float damageWithMultiplier = SnakeShield.BaseDamage * player.meleeDamage;
						float knockback = 9f;
						bool crit = Main.rand.Next(100) < player.meleeCrit;

						if(player.kbGlove)
							knockback *= 2f;
						if(player.kbBuff)
							knockback *= 1.5f;

						int direction = player.velocity.X == 0 ? player.direction : Math.Sign(player.velocity.X);

						if(player.whoAmI == Main.myPlayer){
							player.ApplyDamageToNPC(npc, (int)damageWithMultiplier, knockback, direction, crit);
							npc.AddBuff(BuffID.Poisoned, 8 * 60);
							npc.AddBuff(ModContent.BuffType<PrimordialWrath>(), (int)(4.5f * 60));
						}

						DashTimer = 10;
						DashDelay = 30;
						player.velocity.X = -direction * 9;
						player.velocity.Y = -4f;
						player.immune = true;
						player.immuneNoBlink = true;
						//Longer immune because the dash is faster
						player.immuneTime = 8;
						HitNPCIndex = i;
					}
				}
			}else if((!player.controlLeft || player.velocity.X >= 0f) && (!player.controlRight || player.velocity.X <= 0f))
				player.velocity.X *= 0.95f;
		}
	}
}
