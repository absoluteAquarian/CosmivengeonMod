using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Items.Equippable.Accessories.Draek;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players {
	//Used with CosmivengeonMod.Items.Equippable.Accessories.Draek.SnakeShield
	public class SnakeShieldPlayer : ModPlayer {
		//These indicate what direction is what in the timer arrays used
		public const int DashUp = 0;
		public const int DashDown = 1;
		public const int DashRight = 2;
		public const int DashLeft = 3;

		//The direction the player is currently dashing towards.  Defaults to -1 if no dash is ocurring.
		public int DashDir = -1;

		//The fields related to the dash accessory
		public bool DashActive = false;
		public int DashDelay = MAX_DASH_DELAY;
		public int DashTimer = MAX_DASH_TIMER;
		//The initial velocity.  SoC dash has 14.5f velocity, but this dash lasts longer so it appears to be "faster"
		public readonly float DashVelocity = 11.5f;
		//These two fields are the max values for the delay between dashes and the length of the dash in that order
		//The time is measured in frames
		public const int MAX_DASH_DELAY = 50;
		public const int MAX_DASH_TIMER = 35;

		public int HitNPCIndex = -1;

		public override void ResetEffects() {
			//ResetEffects() is called not long after player.doubleTapCardinalTimer's values have been set

			//Check if the ExampleDashAccessory is equipped and also check against this priority:
			// If the Shield of Cthulhu, Master Ninja Gear, Tabi and/or Solar Armour set is equipped, prevent this accessory from doing its dash effect
			//The priority is used to prevent undesirable effects.
			//Without it, the player is able to use the ExampleDashAccessory's dash as well as the vanilla ones
			bool dashAccessoryEquipped = false;

			//This is the loop used in vanilla to update/check the not-vanity accessories
			for (int i = 3; i < 8 + Player.extraAccessorySlots; i++) {
				Item item = Player.armor[i];

				//Set the flag for the ExampleDashAccessory being equipped if we have it equipped OR immediately return if any of the accessories are
				// one of the higher-priority ones
				if (item.type == ModContent.ItemType<SnakeShield>())
					dashAccessoryEquipped = true;
				else if (item.type == ItemID.EoCShield || item.type == ItemID.MasterNinjaGear || item.type == ItemID.Tabi)
					return;
			}

			//If we don't have the ExampleDashAccessory equipped or the player has the Solor armor set equipped, return immediately
			//Also return if the player is currently on a mount, since dashes on a mount look weird, or if the dash was already activated
			if (!dashAccessoryEquipped || Player.setSolar || Player.mount.Active || DashActive)
				return;

			//When a directional key is pressed and released, vanilla starts a 15 tick (1/4 second) timer during which a second press activates a dash
			//If the timers are set to 15, then this is the first press just processed by the vanilla logic.  Otherwise, it's a double-tap
			if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < 15)
				DashDir = DashRight;
			else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < 15)
				DashDir = DashLeft;
			else {
				DashDir = 0;
				return;  //No dash was activated, return
			}

			DashActive = true;

			//Here you'd be able to set an effect that happens when the dash first activates
			//Some examples include:  the larger smoke effect from the Master Ninja Gear and Tabi
			Player.dash = 0;
			Player.ChangeDir(DashDir == DashRight ? 1 : -1);
		}

		//Copy of the SoC's code in Player.DashMovement(), but changed to fit this accessory
		public void CheckSoCHitEffect() {
			if (HitNPCIndex < 0) {
				//Vanilla uses the player's hitbox and adds 4px to each direction, then offsets it by half of the player's velocity.
				//We'll a larger component of the velocity since the dash is faster
				Rectangle collisionCheck = new Rectangle((int)(Player.position.X + Player.velocity.X * 2f - 4), (int)(Player.position.Y + Player.velocity.Y * 2f - 4), Player.width + 8, Player.height + 8);
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.active && !npc.dontTakeDamage && !npc.friendly && collisionCheck.Intersects(npc.getRect()) && (npc.noTileCollide || Player.CanHit(npc))) {
						float damageWithMultiplier = SnakeShield.BaseDamage * Player.GetDamage(DamageClass.Melee);
						float knockback = 9f;
						bool crit = Main.rand.Next(100) < Player.GetCritChance(DamageClass.Generic);

						if (Player.kbGlove)
							knockback *= 2f;
						if (Player.kbBuff)
							knockback *= 1.5f;

						int direction = Player.velocity.X == 0 ? Player.direction : Math.Sign(Player.velocity.X);

						if (Player.whoAmI == Main.myPlayer) {
							Player.ApplyDamageToNPC(npc, (int)damageWithMultiplier, knockback, direction, crit);
							npc.AddBuff(BuffID.Poisoned, 8 * 60);
							npc.AddBuff(ModContent.BuffType<PrimordialWrath>(), (int)(4.5f * 60));
						}

						DashTimer = 10;
						DashDelay = 30;
						Player.velocity.X = -direction * 9;
						Player.velocity.Y = -4f;
						Player.immune = true;
						Player.immuneNoBlink = true;
						//Longer immune because the dash is faster
						Player.immuneTime = 8;
						HitNPCIndex = i;
					}
				}
			} else if ((!Player.controlLeft || Player.velocity.X >= 0f) && (!Player.controlRight || Player.velocity.X <= 0f))
				Player.velocity.X *= 0.95f;
		}
	}
}
