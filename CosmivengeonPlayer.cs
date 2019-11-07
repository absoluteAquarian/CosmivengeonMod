using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Buffs.Stamina;
using Terraria.GameInput;
using System.Reflection;

namespace CosmivengeonMod{
	public class CosmivengeonPlayer : ModPlayer{
		//Debuffs
		public bool primordialWrath;

		//Custom double jump booleans
		public bool doubleJump_JewelOfOronitus;
		public bool dJumpEffect_JewelOfOronitus = false;
		public bool jumpAgain_JewelOfOronitus = false;
		public bool flag_JewelOfOronitus = false;

		//Summon buffs
		public bool babySnek;

		//Special damage increase thing
		public Stamina stamina;

		public override void Initialize(){
			stamina = new Stamina(player);
		}

		public override void ResetEffects(){
			primordialWrath = false;
			doubleJump_JewelOfOronitus = false;
			babySnek = false;
			stamina.Reset();
		}

		public override void UpdateBadLifeRegen(){
			if(primordialWrath){
				if(player.lifeRegen > 0)
					player.lifeRegen = 0;
				player.statDefense -= 10;
				player.endurance -= 0.1f;
				player.lifeRegen -= 15 * 2;
			}
		}

		public override void PostUpdateRunSpeeds(){
			stamina.RunSpeedChange();
		}

		public override void PreUpdate(){
			stamina.FallSpeedDebuff();
		}

		public override void PostUpdate(){
			stamina.Update();
		}

		public override void ProcessTriggers(TriggersSet triggersSet){
			if(CosmivengeonMod.StaminaHotKey.JustPressed && !stamina.Exhaustion && CosmivengeonWorld.desoMode){
				stamina.Active = !stamina.Active;
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer){
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)CosmivengeonModMessageType.SyncPlayer);
			packet.Write(player.whoAmI);
			packet.Write(stamina.Value);
			
			packet.Send(toWho, fromWho);
		}

		public override void UpdateDead(){
			primordialWrath = false;
			doubleJump_JewelOfOronitus = false;
			babySnek = false;
			stamina.Active = false;
			stamina.ResetValue();
		}

		public override float UseTimeMultiplier(Item item) => stamina.UseTimeMultiplier();
	}
}
