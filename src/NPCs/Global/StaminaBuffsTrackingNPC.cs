using CosmivengeonMod.API;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global {
	public class StaminaBuffsTrackingNPC : GlobalNPC {
		public override void OnKill(NPC npc) {
			// If we're not in Desolation mode, don't do anything
			if (!Main.expertMode || !WorldEvents.desoMode)
				return;

			if (!StaminaBossKillBuffLoader.TryFindBuffData(npc.type, out var buff))
				return;  // Does not exist

			// Handle type redirections
			int type = npc.type;
			buff.TransmuteNPCType(ref type);

			if (Main.netMode == NetmodeID.Server) {
				// If this game is a multiplayer server, handle this effect differently
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player plr = Main.player[i];

					if (!plr.active)
						continue;

					ProcessPlayer(npc, type, plr, true, buff);
				}
			} else {
				// This hook only runs for the server and in singleplayer, so we can be certain than this game isn't a multiplayer client here
				ProcessPlayer(npc, type, Main.LocalPlayer, false, buff);
			}
		}

		private static void ProcessPlayer(NPC npc, int npcType, Player player, bool isClient, StaminaBossKillBuff buff) {
			BossLogPlayer mp = player.GetModPlayer<BossLogPlayer>();
			
			// Only do the stamina buff checks on bosses that exist in the predefined Dictionaries
			StaminaBuffData data = new(npcType);

			if (npc.boss && !npc.friendly && npc.playerInteraction[player.whoAmI] && !mp.BossesKilled.Has(data)) {
				// It's a boss and it's dead; add it to the list if it's not there already and print the message for it
				mp.BossesKilled.Add(data);

				foreach (string line in buff.GetClientMessage()) {
					if (isClient)
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(line), Color.HotPink, player.whoAmI);
					else
						Main.NewText(line, Color.HotPink);
				}
			}
		}
	}
}
