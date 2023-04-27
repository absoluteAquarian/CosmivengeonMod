using CosmivengeonMod.API;
using CosmivengeonMod.API.Edits.Desomode;
using CosmivengeonMod.API.Managers;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Placeable.MusicBoxes;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using CosmivengeonMod.Tiles.MusicBoxes;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Content.Sources;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;
using CosmivengeonMod.Abilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.Utilities;

namespace CosmivengeonMod {
	public class CoreMod : Mod {
		public static class RecipeGroups {
			/// <summary>
			/// "Cosmivengeon: Evil Drops" - "Any Shadow Scale"
			/// </summary>
			public static readonly string EvilDrops = "Cosmivengeon: Evil Drops";

			/// <summary>
			/// "Cosmivengeon: Evil Bars" - "Any Demonite Bar"
			/// </summary>
			public static readonly string EvilBars = "Cosmivengeon: Evil Bars";

			/// <summary>
			/// "Cosmivengeon: Gold or Platinum" - "Any Gold Bar"
			/// </summary>
			public static readonly string Tier4Bars = "Cosmivengeon: Gold or Platinum";

			/// <summary>
			/// "Cosmviengeon: Strange Plants" - "Any Strange Plant"
			/// </summary>
			public static readonly string WeirdPlant = "Cosmivengeon: Strange Plants";
		}

		public static class Descriptions {
			/// <summary>
			/// "Description to be added..."
			/// </summary>
			public static readonly string PlaceHolder = "Description to be added...";

			/// <summary>
			/// "THIS IS A DEBUG ITEM"
			/// </summary>
			public static readonly string DebugItem = "[c/555555:THIS IS A DEBUG ITEM]";
		}

		public static CoreMod Instance => ModContent.GetInstance<CoreMod>();

		/// <summary>
		/// Whether or not this version of the mod is a public release or a test build.
		/// </summary>
		public static readonly bool Release = false;

		//Stamina use hotkey
		public static ModKeybind StaminaHotKey;

		private static readonly MethodInfo Mod_get_File = typeof(Mod).GetProperty("File", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(nonPublic: true);

		public override IContentSource CreateDefaultContentSource() => new PossibleAssetsDirectoryRedirectContentSource(Mod_get_File.Invoke(this, null) as TmodFile);

		public override void Load() {
			StaminaHotKey = KeybindLoader.RegisterKeybind(this, "Toggle Stamina Use", "G");

			ModReferences.Load();

			//Only run this segment if we're not loading on a server
			bool server = Main.dedServ || Main.netMode == NetmodeID.Server;

			if (!server) {
				//Add music boxes
				MusicLoader.AddMusicBox(this, MusicLoader.GetMusicSlot(this, "Sounds/Music/Frigid_Feud"),
					ModContent.ItemType<FrostbiteBox>(),
					ModContent.TileType<FrostbiteBoxTile>());
				MusicLoader.AddMusicBox(this, MusicLoader.GetMusicSlot(this, "Sounds/Music/Successor_of_the_Jewel"),
					ModContent.ItemType<DraekBox>(),
					ModContent.TileType<DraekBoxTile>());

				Ref<Effect> eocEffect = new Ref<Effect>(Assets.Request<Effect>("Effects/screen_eoc", AssetRequestMode.ImmediateLoad).Value);

				FilterCollection.Screen_EoC = new Filter(new ScreenShaderData(eocEffect, "ScreenDarken"), EffectPriority.High);
			}

			//Make certain debuffs show the time remaining
			Main.buffNoTimeDisplay[BuffID.Slimed] = false;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = false;

			if (!server)
				Main.rand ??= new();

			BossPackage.bossInfo = new Dictionary<CosmivengeonBoss, BossPackage>() {
				[CosmivengeonBoss.Frostbite] = new BossPackage(ModContent.NPCType<Frostbite>(),
					-1,
					"\"Looks like the lure didn't work.  Maybe it would work better in a colder area?\"",
					static player => player.ZoneSnow,
					server
						? null
						: new WeightedTable<int>(Main.rand)
							.Add(MusicLoader.GetMusicSlot(Instance, "Sounds/Music/Frigid_Feud"), 1.0)),
				[CosmivengeonBoss.Draek] = new BossPackage(ModContent.NPCType<Draek>(),
					ModContent.NPCType<DraekP2Head>(),
					"\"The geode was unresponsive.  Maybe I should try using it in the forest?\"",
					static player => player.ZoneForest,
					server
						? null
						: new WeightedTable<int>(Main.rand)
							// 0.5% chance - retro kazoo theme
							.Add(MusicLoader.GetMusicSlot(Instance, "Sounds/Music/successor_of_the_kazoo"), 0.005)
							// 0.5% chance - kazoo theme
							.Add(MusicLoader.GetMusicSlot(Instance, "Sounds/Music/Successor_of_the_Kazoo_Round_2"), 0.005)
							// 5% chance - retro theme
							.Add(MusicLoader.GetMusicSlot(Instance, "Sounds/Music/RETRO_SuccessorOfTheJewel"), 0.05)
							// Remaining chance - current theme
							.AddExcess(MusicLoader.GetMusicSlot(Instance, "Sounds/Music/Successor_of_the_Jewel"), 1.0))
			};
		}

		public override void PostSetupContent() {
			CrossMod.Load();
			StaminaBossKillBuffLoader.ValidateBuffData();
		}

		public override void Unload() {
			StaminaHotKey = null;

			ModReferences.Unload();

			if (FilterCollection.Screen_EoC?.Active ?? false)
				FilterCollection.Screen_EoC.Deactivate();

			//Restore the original setting for the buffs
			Main.buffNoTimeDisplay[BuffID.Slimed] = true;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = true;

			BossPackage.bossInfo = null;
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType message = (MessageType)reader.ReadByte();

			switch (message) {
				case MessageType.SyncPlayer:
					byte clientWhoAmI = reader.ReadByte();
					StaminaPlayer mp = Main.player[clientWhoAmI].GetModPlayer<StaminaPlayer>();
					mp.stamina.ReceiveData(reader);
					break;
				case MessageType.StaminaChanged:
					clientWhoAmI = reader.ReadByte();
					mp = Main.player[clientWhoAmI].GetModPlayer<StaminaPlayer>();
					mp.stamina.ReceiveData(reader);

					if (Main.netMode == NetmodeID.Server) {
						ModPacket packet = GetPacket();
						packet.Write((byte)MessageType.StaminaChanged);
						packet.Write(clientWhoAmI);
						mp.stamina.SendData(packet);
						packet.Send(-1, clientWhoAmI);
					}
					break;
				case MessageType.SyncEoWGrab:
					DetourNPCHelper.EoW_GrabbingNPC = reader.ReadInt32();
					DetourNPCHelper.EoW_GrabbedPlayer = reader.ReadInt32();
					break;
				case MessageType.SyncGlobalNPCBossData:
					DetourNPCHelper.ReceiveData(reader);
					break;
				case MessageType.SpawnBoss:
					clientWhoAmI = reader.ReadByte();
					int npcType = reader.ReadInt32();
					float tileRange = reader.ReadSingle();

					MiscUtils.SummonBossNearPlayer(Main.player[clientWhoAmI], npcType, tileRange);
					break;
				case MessageType.SpawnBossAbovePlayer:
					clientWhoAmI = reader.ReadByte();
					npcType = reader.ReadInt32();
					float leftOffset = reader.ReadSingle();
					float rightOffset = reader.ReadSingle();
					float upOffset = reader.ReadSingle();
					float downOffset = reader.ReadSingle();

					MiscUtils.SummonBossAbovePlayer(Main.player[clientWhoAmI], npcType, leftOffset, rightOffset, upOffset, downOffset);
					break;
				default:
					Logger.WarnFormat("CosmivengeonMod: Unknown message type: {0}", message);
					break;
			}
		}

		public override object Call(params object[] args) {
			/*		Possible commands:
			 *	"GetDesolation"/"Desolation"
			 *	"SetDesolation",              true/false
			 */
			if (args[0] is not string command)
				throw new ArgumentException("First argument is expected to be a command string");

			switch (command.ToLower()) {
				case "getdesolation":
				case "desolation":
					if (args[1] is string arg && arg.ToLower() is "desolation" or "desomode" or "deso") {
						return WorldEvents.desoMode;
					} else
						throw new Exception("Invalid argument list detected for command: " + command);
				case "setdesolation":
					if (args[1] is string flag && bool.TryParse(flag, out bool value)) {
						WorldEvents.desoMode = value;
						return null;
					} else
						throw new Exception("Invalid argument list detected for command: " + command);
				default:
					throw new Exception("Unknown command: " + command);
			}
		}
	}
}