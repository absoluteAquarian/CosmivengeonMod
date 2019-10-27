using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

		public override void ResetEffects(){
			primordialWrath = false;
			doubleJump_JewelOfOronitus = false;
			babySnek = false;
		}

		public override void UpdateBadLifeRegen(){
			if(primordialWrath){
				if(player.lifeRegen > 0)
					player.lifeRegen = 0;
				player.statDefense -= 10;
				player.endurance -= 0.1f;
				player.lifeRegen -= 100;
			}
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer){
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)CosmivengeonModMessageType.SyncPlayer);
			packet.Write(player.whoAmI);
			
			packet.Send(toWho, fromWho);
		}

		public override void UpdateDead(){
			primordialWrath = false;
			doubleJump_JewelOfOronitus = false;
			babySnek = false;
		}
	}
}
