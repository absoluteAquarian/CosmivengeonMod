using CosmivengeonMod.API.Commands;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Players{
	public class DicePlayer : ModPlayer{
		public bool badShop;
		public bool goodShop;
		internal int shopModifierTimer;

		public bool forceReversedGravity;
		internal int forcedGravityTimer;

		public bool fishDontWantMe;
		internal int fishTimer;

		public bool moreIFrames;
		internal int moreIFrameTimer;

		public bool noStaminaDecay;
		internal int nsdTimer;

		public int extraLives;

		public int godmodeTimer;

		public int buffDamageTimer;

		public int endlessClipTimer;
		public int endlessManaTimer;

		public override void SaveData(TagCompound tag)/* tModPorter Suggestion: Edit tag parameter instead of returning new TagCompound */
			=> new TagCompound(){
				["badShop"] = badShop,
				["goodShop"] = goodShop,
				["shopTimer"] = shopModifierTimer,
				["forceGravity"] = forceReversedGravity,
				["forceGravityTimer"] = forcedGravityTimer,
				["fish"] = fishDontWantMe,
				["fishTimer"] = fishTimer,
				["moreIF"] = moreIFrames,
				["moreIFTimer"] = moreIFrameTimer,
				["noDecay"] = noStaminaDecay,
				["noDecayTimer"] = nsdTimer,
				["lives"] = extraLives,
				["godmode"] = godmodeTimer,
				["damage"] = buffDamageTimer,
				["ammo"] = endlessClipTimer,
				["mana"] = endlessManaTimer
			};

		public override void LoadData(TagCompound tag){
			badShop = tag.GetBool("badShop");
			goodShop = tag.GetBool("goodShop");
			shopModifierTimer = tag.GetInt("shopTimer");

			forceReversedGravity = tag.GetBool("forceGravity");
			forcedGravityTimer = tag.GetInt("forceGravityTimer");

			fishDontWantMe = tag.GetBool("fish");
			fishTimer = tag.GetInt("fishTimer");

			moreIFrames = tag.GetBool("moreIF");
			moreIFrameTimer = tag.GetInt("moreIFTimer");

			noStaminaDecay = tag.GetBool("noDecay");
			nsdTimer = tag.GetInt("noDecayTimer");

			extraLives = tag.GetInt("lives");

			godmodeTimer = tag.GetInt("godmode");

			buffDamageTimer = tag.GetInt("damage");

			endlessClipTimer = tag.GetInt("ammo");
			endlessManaTimer = tag.GetInt("mana");
		}

		public void SetShopModifier(bool good, int duration = 10 * 60 * 60){
			badShop = !good;
			goodShop = good;

			shopModifierTimer = duration;
		}

		public void SetForcedGravity(int duration = 2 * 60 * 60){
			forceReversedGravity = true;
			forcedGravityTimer = duration;

			Player.gravDir = -1;
		}

		public void SetFishModifier(bool detriment, int duration = TimeSetter._7_30PM_day * 2 + TimeSetter._4_30AM_night * 2){
			fishDontWantMe = detriment;

			//Two ingame days' worth of time by default
			fishTimer = duration;
		}

		public void SetMoreIFrames(int duration = 10 * 60 * 60){
			moreIFrames = true;
			moreIFrameTimer = duration;
		}

		public void SetNSD(int duration = 2 * 60 * 60){
			noStaminaDecay = true;
			nsdTimer = duration;
		}

		public override void ResetEffects(){
			if(shopModifierTimer <= 0){
				badShop = false;
				goodShop = false;
			}

			if(forcedGravityTimer <= 0)
				forceReversedGravity = false;

			if(fishTimer <= 0)
				fishDontWantMe = false;

			if(moreIFrameTimer <= 0)
				moreIFrames = false;

			if(nsdTimer <= 0)
				noStaminaDecay = false;
		}

		public override void PostUpdateRunSpeeds(){
			if(forceReversedGravity)
				Player.gravControl = true;

			//This hook is called just before the gravity flipping code is executed
			if(forceReversedGravity && !Main.gameMenu){
				if(Player.controlUp && Player.releaseUp){
					//Set the gravity to the opposite so that the vanilla code reverses back to the intended value
					Player.gravDir = 1;
				}else
					Player.gravDir = -1;
			}
		}

		public override void PostUpdateMiscEffects(){
			if(moreIFrames)
				Player.longInvince = true;

			if(godmodeTimer > 0){
				Player.eocDash = 30;
				Player.armorEffectDrawShadowEOCShield = true;
			}
		}

		public override void GetFishingLevel(Item fishingRod, Item bait, ref int fishingLevel){
			if(fishDontWantMe)
				fishingLevel -= 50;
			else if(!fishDontWantMe && fishTimer > 0)
				fishingLevel += 50;
		}

		public override void PostUpdate(){
			if(godmodeTimer > 0 && Player.statLife < Player.statLifeMax2)
				Player.statLife = Player.statLifeMax2;

			if(shopModifierTimer > 0)
				shopModifierTimer--;

			if(forcedGravityTimer > 0)
				forcedGravityTimer--;

			if(fishTimer > 0)
				fishTimer--;

			if(moreIFrameTimer > 0)
				moreIFrameTimer--;

			if(nsdTimer > 0)
				nsdTimer--;

			if(godmodeTimer > 0)
				godmodeTimer--;

			if(buffDamageTimer > 0)
				buffDamageTimer--;

			if(endlessClipTimer > 0)
				endlessClipTimer--;

			if(endlessManaTimer > 0)
				endlessManaTimer--;
		}

		public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price){
			if(badShop)
				price = (int)(price * 1.5f);
			else if(goodShop)
				price = (int)(price * 0.7f);
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage){
			if(buffDamageTimer > 0)
				mult += 2f;
		}

		public override bool CanConsumeAmmo(Item weapon, Item ammo)
			=> endlessClipTimer == 0;

		public override void ModifyManaCost(Item item, ref float reduce, ref float mult){
			if(endlessManaTimer > 0)
				mult = 0;
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			//DoT debuffs can circumvent the godmode... Prevent that from happening
			if(godmodeTimer > 0){
				Player.statLife = Player.statLifeMax2;

				return false;
			}

			if(extraLives > 0){
				Rectangle r = new Rectangle((int)Player.position.X, (int)Player.position.Y - 3 * 16, 0, 0);
				CombatText.NewText(r, CombatText.LifeRegenNegative, "-1 EXTRA LIFE", dramatic: true);

				extraLives--;

				Player.immuneTime = 8 * 60;
				Player.immuneNoBlink = false;

				Player.statLife = Math.Min(100, Player.statLifeMax2);

				return false;
			}

			return true;
		}

		public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
			=> godmodeTimer <= 0;

		public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright){
			if(drawInfo.drawPlayer.GetModPlayer<DicePlayer>().godmodeTimer > 0){
				r = Main.DiscoR;
				g = Main.DiscoG;
				b = Main.DiscoB;
			}
		}
	}
}
