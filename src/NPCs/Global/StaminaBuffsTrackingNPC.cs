using CosmivengeonMod.Abilities;
using CosmivengeonMod.API.Commands;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Players;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global {
	public class StaminaBuffsTrackingNPC : GlobalNPC {
		public static List<int> BossIDs;
		public static Dictionary<int, Action<Stamina>> BuffActions;
		public static Dictionary<int, string> OnKillMessages;

		public static Dictionary<int, List<string>> BossNames;

		public static void LoadBossNames() {
			BossIDs = new List<int>();
			BuffActions = new Dictionary<int, Action<Stamina>>();
			OnKillMessages = new Dictionary<int, string>();

			BossNames = new Dictionary<int, List<string>>() {
				//Vanilla bosses
				[NPCID.KingSlime] = new List<string>() { "King Slime", "Slime King", "KS", "SK" },
				[NPCID.EyeofCthulhu] = new List<string>() { "Eye of Cthulhu", "EoC" },
				[NPCID.EaterofWorldsHead] = new List<string>() { "Eater of Worlds", "EoW" },
				[NPCID.BrainofCthulhu] = new List<string>() { "Brain of Cthulhu", "Brain", "BoC" },
				[NPCID.QueenBee] = new List<string>() { "Queen Bee", "Bee Queen", "QB", "BQ" },
				[NPCID.SkeletronHead] = new List<string>() { "Skeletron", "Sans" },
				[NPCID.WallofFlesh] = new List<string>() { "Wall of Flesh", "WoF", "Waffle" },
				[NPCID.Retinazer] = new List<string>() { "The Twins", "Retinazer", "Spazmatism" },
				[NPCID.TheDestroyer] = new List<string>() { "The Destroyer", "Destroyer" },
				[NPCID.SkeletronPrime] = new List<string>() { "Skeletron Prime", "SkelePrime", "Sans Prime" },
				[NPCID.Plantera] = new List<string>() { "Plantera", "Plant" },
				[NPCID.Golem] = new List<string>() { "Golem" },
				[NPCID.CultistBoss] = new List<string>() { "Lunatic Cultist", "Cultist", "CultistBoss", "Cultist Boss" },
				[NPCID.MoonLordCore] = new List<string>() { "Moon Lord", "The Moon Lord" },
				//Vanilla minibosses
				[NPCID.DD2DarkMageT1] = new List<string>() { "Dark Mage", "DD2 Mage", "DD2 Dark Mage" },
				[NPCID.DD2OgreT2] = new List<string>() { "Ogre", "DD2 Ogre" },
				[NPCID.DD2Betsy] = new List<string>() { "Betsy", "DD2 Betsy" },
				[NPCID.MourningWood] = new List<string>() { "Mourning Wood", "Morning Wood", "Pumpkin Tree" },
				[NPCID.Pumpking] = new List<string>() { "Pumpking" },
				[NPCID.Everscream] = new List<string>() { "Everscream", "Frost Tree" },
				[NPCID.SantaNK1] = new List<string>() { "Santa-NK1", "SantaNK1", "Mecha Santa", "Santa Boss", "Santa" },
				[NPCID.IceQueen] = new List<string>() { "Ice Queen" },
				//Cosmivengeon bosses
				[ModContent.NPCType<Frostbite>()] = new List<string>() { "Frostbite", "Frost" },
				[ModContent.NPCType<DraekP2Head>()] = new List<string>() { "Draek", "Snek" }
			};
		}

		public static void AddStaminaBossBuff(int type, string message, Action<Stamina> action) {
			BossIDs.Add(type);
			BuffActions.Add(type, action);
			OnKillMessages.Add(type, message);
		}

		public static void UnloadCollections() {
			BossIDs = null;
			BuffActions = null;
			OnKillMessages = null;
			BossNames = null;
		}

		public override void OnKill(NPC npc) {
			// If we're not in Desolation mode, don't do anything
			if (!Main.expertMode || !WorldEvents.desoMode)
				return;

			// If this NPC is a boss and it's a separating segment of a worm boss, force the type to be the head segment
			int type = npc.type;
			if (type == NPCID.EaterofWorldsBody || type == NPCID.EaterofWorldsTail)
				type = NPCID.EaterofWorldsHead;

			string key = ClearBossKilledCommand.GetNPCKeyFromID(type);
			var words = key.Split('.');
			StaminaBuffData data = new StaminaBuffData(words[0], words[1]);

			if (Main.netMode == NetmodeID.Server) {
				// If this game is a multiplayer server, handle this effect differently
				for (int i = 0; i < Main.maxNetPlayers; i++)
					ProcessPlayer(npc, type, Main.player[i], true, words, data);
			} else {
				// This hook only runs for the server and in singleplayer, so we can be certain than this game isn't a multiplayer client here
				ProcessPlayer(npc, type, Main.LocalPlayer, false, words, data);
			}
		}

		private static void ProcessPlayer(NPC npc, int npcType, Player player, bool isClient, string[] words, StaminaBuffData data) {
			if (!player.active)
				return;

			BossLogPlayer mp = player.GetModPlayer<BossLogPlayer>();
			// Only do the stamina buff checks on bosses that exist in the predefined Dictionaries
			if (npc.boss && !npc.friendly && npc.playerInteraction[player.whoAmI] && !mp.BossesKilled.HasKey(words[0], words[1]) && BossIDs.Contains(npcType)) {
				// It's a boss and it's dead; add it to the list if it's not there already and print the message for it
				mp.BossesKilled.Add(data);
				var lines = OnKillMessages[npcType].Split('\n');
				foreach (string line in lines) {
					if (isClient)
						ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(line), Color.HotPink, player.whoAmI);
					else
						Main.NewText(line, Color.HotPink);
				}
			}
		}
	}
}
