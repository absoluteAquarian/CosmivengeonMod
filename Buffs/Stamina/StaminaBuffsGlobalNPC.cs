using CosmivengeonMod.Commands;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Stamina{
	public class StaminaBuffsGlobalNPC : GlobalNPC{
		public static List<int> BossIDs;
		public static Dictionary<int, Action<Stamina>> BuffActions;
		public static Dictionary<int, string> OnKillMessages;

		public static Dictionary<int, List<string>> BossNames;

		public override void NPCLoot(NPC npc){
			//If we're not in Desolation mode, don't do anything
			if(!Main.expertMode || !CosmivengeonWorld.desoMode)
				return;

			//If this NPC is a boss and it's a separating segment of a worm boss, force the type to be the head segment
			int type = npc.type;
			if(type == NPCID.EaterofWorldsBody || type == NPCID.EaterofWorldsTail)
				type = NPCID.EaterofWorldsHead;

			string key = ClearBossKilledCommand.GetNPCKeyFromID(type);
			var words = key.Split('.');
			StaminaBuffData data = new StaminaBuffData(words[0], words[1]);

			//If this game is a multiplayer server, handle this effect differently
			if(Main.netMode == NetmodeID.Server){
				for(int i = 0; i < Main.maxNetPlayers; i++){
					if(!Main.player[i].active)
						continue;

					CosmivengeonPlayer mp = Main.player[i].GetModPlayer<CosmivengeonPlayer>();
					//Only do the stamina buff checks on bosses that exist in the predefined Dictionaries
					if(npc.boss && !npc.friendly && npc.playerInteraction[i] && !mp.BossesKilled.HasKey(words[0], words[1]) && BossIDs.Contains(type)){
						//It's a boss and it's dead; add it to the list if it's not there already and print the message for it
						mp.BossesKilled.Add(data);
						var lines = OnKillMessages[type].Split('\n');
						foreach(string line in lines)
							NetMessage.SendChatMessageToClient(NetworkText.FromLiteral(line), Color.HotPink, i);
					}
				}
			}else{
				//This hook only runs for the server and in singleplayer, so we can be certain than this game isn't a multiplayer client here
				CosmivengeonPlayer mp = Main.LocalPlayer.GetModPlayer<CosmivengeonPlayer>();
				//Only do the stamina buff checks on bosses that exist in the predefined Dictionaries
				if(npc.boss && !npc.friendly && npc.playerInteraction[Main.myPlayer] && !mp.BossesKilled.HasKey(words[0], words[1]) && BossIDs.Contains(type)){
					//It's a boss and it's dead; add it to the list if it's not there already and print the message for it
					mp.BossesKilled.Add(data);
					var lines = OnKillMessages[type].Split('\n');
					foreach(string line in lines)
						Main.NewText(line, Color.HotPink);
				}
			}
		}
	}
}
