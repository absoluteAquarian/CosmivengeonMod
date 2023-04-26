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

			StaminaBuffsTrackingNPC.LoadBossNames();

			//Only run this segment if we're not loading on a server
			if (!Main.dedServ && Main.netMode != NetmodeID.Server) {
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

			//Vanilla bosses
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.KingSlime,
				"Defeating the monarch of slime has loosened up your muscles, allowing you to use Stamina for longer and recover from Exhaustion faster." +
					"\n Idle restoration rate: +10%, Exhausted restoration rate: +6%, Active consumption rate: -3.5%" +
					"\n Maximum Stamina: +1000 units",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.restorationRate.active += 0.1f;
					stat.restorationRate.exhausted += 0.6f;
					stat.consumptionRate -= 0.035f;
					stat.maxQuantity.Base += 0.1f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.EyeofCthulhu,
				"Defeating the master observer of the night has honed your senses, allowing you to move and attack faster while in the Active state." +
					"\n Active attack speed rate: +3%" +
					"\n Active run acceleration rate: +6%, Active max run speed: +5%",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.03f;
					stat.runAcceleration.active += 0.06f;
					stat.maxRunSpeed.active += 0.05f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.EaterofWorldsHead,
				"Defeating the grotesque harbinger from the Corruption has strengthened your resolve, reducing the harmful effects from Exhaustion." +
					"\n Exhausted attack speed rate: +1.5%" +
					"\n Exhausted run acceleration rate: +4%, Exhausted max run speed: +5.75%" +
					"\n Exhausted restoration rate: +6%",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.exhausted += 0.015f;
					stat.runAcceleration.exhausted += 0.04f;
					stat.maxRunSpeed.exhausted += 0.0575f;
					stat.restorationRate.exhausted += 0.06f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.BrainofCthulhu,
				"Defeating the Crimson's mastermind has sharpened your wits, letting you react faster to your surroundings." +
					"\n Active attack speed rate: +2.5%" +
					"\n Active run acceleration rate: +7%, Active max run speed: +4%",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.025f;
					stat.runAcceleration.active += 0.07f;
					stat.maxRunSpeed.active += 0.04f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.QueenBee,
				"Defeating the monarch of the jungle has improved your resilience to Exhaustion and your overall abilities while in the Active state." +
					"\n Active attack speed rate: +2%" +
					"\n Active run acceleration rate: +3%" +
					"\n Exhausted attack speed rate: +0.75%" +
					"\n Exhausted run acceleration rate: +1.5%",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.02f;
					stat.runAcceleration.active += 0.03f;
					stat.attackSpeed.exhausted += 0.0075f;
					stat.runAcceleration.exhausted += 0.015f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.SkeletronHead,
				"Defeating the cursed guardian of the Dungeon has further increased your control over your Stamina." +
					"\n Active attack speed rate: +3%" +
					"\n Active run acceleration rate: +5%, Active max run speed: +5%" +
					"\n Exhausted attack speed rate: +2.5%" +
					"\n Exhausted run acceleration rate: +3.125%, Exhausted max run speed: +3.125%" +
					"\n Idle restoration rate: +22.5%, Exhausted restoration rate: +8%, Active consumption rate: -6%" +
					"\n Maximum Stamina: +2500 units",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.03f;
					stat.runAcceleration.active += 0.05f;
					stat.maxRunSpeed.active += 0.05f;
					stat.attackSpeed.exhausted += 0.025f;
					stat.runAcceleration.exhausted += 0.03125f;
					stat.maxRunSpeed.exhausted += 0.03125f;
					stat.restorationRate.active += 0.225f;
					stat.restorationRate.exhausted += 0.08f;
					stat.consumptionRate -= 0.06f;
					stat.maxQuantity.Base += 0.25f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.WallofFlesh,
				"Defeating the horrifying guarding of the world has enlightened you to a new level of mastery over your Stamina." +
					"\n Active attack speed rate: +3.25%" +
					"\n Active run acceleration rate: +6.5%, Active max run speed: +6.5%" +
					"\n Exhausted attack speed rate: +2%" +
					"\n Exhausted run acceleration rate: +3%, Exhausted max run speed: +3%" +
					"\n Idle restoration rate: +30%, Exhausted restoration rate: +10%, Active consumption rate: -10%" +
					"\n Maximum Stamina: +5000 units",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.0325f;
					stat.runAcceleration.active += 0.065f;
					stat.maxRunSpeed.active += 0.065f;
					stat.attackSpeed.exhausted += 0.02f;
					stat.runAcceleration.exhausted += 0.03f;
					stat.maxRunSpeed.exhausted += 0.03f;
					stat.restorationRate.active += 0.3f;
					stat.restorationRate.exhausted += 0.1f;
					stat.consumptionRate -= 0.1f;
					stat.maxQuantity.Base += 0.5f;
				});
			// TODO: add the rest of the vanilla boss stuff
			//Vanilla minibosses
			// TODO: add the vanilla miniboss stuff
			//Cosmivengeon bosses
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(ModContent.NPCType<Frostbite>(),
				"Defeating the mutant frost demon has boosted your resistance to the effects of Exhaustion." +
					"\n Exhausted attack speed rate: +1.75%" +
					"\n Exhausted run acceleration rate: +1.25%, Exhausted max run speed: +2.5%" +
					"\n Exhausted restoration rate: +5%",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.exhausted += 0.0175f;
					stat.runAcceleration.exhausted += 0.0125f;
					stat.maxRunSpeed.exhausted += 0.025f;
					stat.restorationRate.exhausted += 0.05f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(ModContent.NPCType<DraekP2Head>(),
				"Defeating the serpentine master of the Forest has taught you to steady your form, allowing you to use your Stamina for longer and slightly increasing its benefits." +
					"\n Idle restoration rate: +18%, Active consumption rate: -7.5%" +
					"\n Active attack speed rate: +1.25%" +
					"\n Active run acceleration rate: +2%, Active max run speed: +2%" +
					"\n Maximum Stamina: +1500 units",
				(out StaminaStatModifier stat) => {
					stat = StaminaStatModifier.Default;
					stat.attackSpeed.active += 0.0125f;
					stat.runAcceleration.active += 0.02f;
					stat.maxRunSpeed.active += 0.02f;
					stat.restorationRate.active += 0.18f;
					stat.consumptionRate -= 0.075f;
					stat.maxQuantity.Base += 0.15f;
				});
			// TODO: add the rest of the Cosmivengeon boss stuff
			// TODO: add cross-mod support

			//Make certain debuffs show the time remaining
			Main.buffNoTimeDisplay[BuffID.Slimed] = false;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = false;
		}

		public override void PostSetupContent() {
			CrossMod.Load();
		}

		public override void Unload() {
			StaminaHotKey = null;

			ModReferences.Unload();

			StaminaBuffsTrackingNPC.UnloadCollections();

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