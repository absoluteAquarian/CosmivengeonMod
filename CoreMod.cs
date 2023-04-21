using CosmivengeonMod.API;
using CosmivengeonMod.API.Edits;
using CosmivengeonMod.API.Edits.Detours.Desomode;
using CosmivengeonMod.API.Managers;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Placeable.MusicBoxes;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Players;
using CosmivengeonMod.Tiles.MusicBoxes;
using CosmivengeonMod.UI;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod{
	public class CoreMod : Mod{
		public static class RecipeGroups{
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

		public static class Descriptions{
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

		private StaminaUI staminaUI;
		private UserInterface userInterface;

		public override void Load(){
			StaminaHotKey = KeybindLoader.RegisterKeybind(this, "Toggle Stamina Use", "G");

			ModReferences.Load();

			EditsLoader.Load();

			PrefixManager.Load();

			StaminaBuffsTrackingNPC.Load();

			//Only run this segment if we're not loading on a server
			if(!Main.dedServ && Main.netMode != NetmodeID.Server){
				staminaUI = new StaminaUI();
				staminaUI.Activate();

				userInterface = new UserInterface();
				userInterface.SetState(staminaUI);

				//Add music boxes
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Frigid_Feud"),
					ModContent.ItemType<FrostbiteBox>(),
					ModContent.TileType<FrostbiteBoxTile>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel"),
					ModContent.ItemType<DraekBox>(),
					ModContent.TileType<DraekBoxTile>());

				Ref<Effect> eocEffect = new Ref<Effect>(GetEffect("Effects/screen_eoc"));

				FilterCollection.Screen_EoC = new Filter(new ScreenShaderData(eocEffect, "ScreenDarken"), EffectPriority.High);

				PrimitiveDrawing.Init(Main.graphics.GraphicsDevice);
			}

			//Vanilla bosses
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.KingSlime,
				"Defeating the monarch of slime has loosened up your muscles, allowing you to use Stamina for longer and recover from Exhaustion faster." +
					"\n Idle increase rate: +10%, Exhaustion increase rate: +6%, Active use rate: -3.5%" +
					"\n Maximum Stamina: +1000 units",
				stamina => {
					stamina.AddEffects(incMult: 0.1f, exIncMult: 0.06f, decMult: -0.035f, maxAdd: 1000);
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.EyeofCthulhu,
				"Defeating the master observer of the night has honed your senses, allowing you to move and attack faster while in the Active state." +
					"\n Active attack speed rate: +3%" +
					"\n Active move acceleration rate: +6%, Active max move speed: +5%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.03f;
					stamina.MoveSpeedBuffMultiplier += 0.06f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.05f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.EaterofWorldsHead,
				"Defeating the grotesque harbinger from the Corruption has strengthened your resolve, reducing the harmful effects from Exhaustion." +
					"\n Exhaustion attack speed rate: +1.5%" +
					"\n Exhaustion move acceleration rate: +4%, Exhaustion max move speed: +5.75%" +
					"\n Exhausted increase rate: +6%",
				stamina => {
					stamina.AddEffects(exIncMult: 0.06f);
					stamina.AttackSpeedDebuffMultiplier += 0.015f;
					stamina.MoveSpeedDebuffMultiplier += 0.04f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.0575f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.BrainofCthulhu,
				"Defeating the Crimson's mastermind has sharpened your wits, letting you react faster to your surroundings." +
					"\n Active attack speed rate: +2.5%" +
					"\n Active move acceleration rate: +7%, Active max move speed: +4%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.025f;
					stamina.MoveSpeedBuffMultiplier += 0.07f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.04f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.QueenBee,
				"Defeating the monarch of the jungle has improved your resilience to Exhaustion and your overall abilities while in the Active state." +
					"\n Active attack speed rate: +2%" +
					"\n Active move acceleration rate: +3%" +
					"\n Exhaustion attack speed rate: +0.75%" +
					"\n Exhaustion move acceleration rate: +1.5%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.02f;
					stamina.MoveSpeedBuffMultiplier += 0.03f;
					stamina.AttackSpeedDebuffMultiplier += 0.0075f;
					stamina.MoveSpeedDebuffMultiplier += 0.015f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.SkeletronHead,
				"Defeating the cursed guardian of the Dungeon has further increased your control over your Stamina." +
					"\n Active attack speed rate: +3%" +
					"\n Active move acceleration rate: +5%, Active max move speed: +5%" +
					"\n Exhaustion attack speed rate: +2.5%" +
					"\n Exhaustion move acceleration rate: +3.125%, Exhaustion max move speed: +3.125%" +
					"\n Idle increase rate: +22.5%, Exhaustion increase rate: +8%, Active use rate: -6%" +
					"\n Maximum Stamina: +2500 units",
				stamina => {
					stamina.AddEffects(incMult: 0.225f, exIncMult: 0.08f, decMult: -0.06f, maxAdd: 2500);
					stamina.AttackSpeedBuffMultiplier += 0.03f;
					stamina.MoveSpeedBuffMultiplier += 0.05f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.05f;
					stamina.AttackSpeedDebuffMultiplier += 0.025f;
					stamina.MoveSpeedDebuffMultiplier += 0.03125f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.03125f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(NPCID.WallofFlesh,
				"Defeating the horrifying guarding of the world has enlightened you to a new level of mastery over your Stamina." +
					"\n Active attack speed rate: +3.25%" +
					"\n Active move acceleration rate: +6.5%, Active max move speed: +6.5%" +
					"\n Exhaustion attack speed rate: +2%" +
					"\n Exhaustion move acceleration rate: +3%, Exhaustion max move speed: +3%" +
					"\n Idle increase rate: +30%, Exhaustion increase rate: +10%, Active use rate: -10%" +
					"\n Maximum Stamina: +5000 units",
				stamina => {
					stamina.AddEffects(incMult: 0.3f, exIncMult: 0.1f, decMult: -0.1f, maxAdd: 5000);
					stamina.AttackSpeedBuffMultiplier += 0.0325f;
					stamina.MoveSpeedBuffMultiplier += 0.065f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.065f;
					stamina.AttackSpeedBuffMultiplier += 0.02f;
					stamina.MoveSpeedDebuffMultiplier += 0.03f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.03f;
				});
			// TODO: add the rest of the vanilla boss stuff
			//Vanilla minibosses
			// TODO: add the vanilla miniboss stuff
			//Cosmivengeon bosses
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(ModContent.NPCType<Frostbite>(),
				"Defeating the mutant frost demon has boosted your resistance to the effects of Exhaustion." +
					"\n Exhaustion attack speed rate: +1.75%" +
					"\n Exhaustion move acceleration rate: +1.25%, Exhaustion max move speed: +2.5%" +
					"\n Exhaustion increase rate: +5%",
				stamina => {
					stamina.AddEffects(exIncMult: 0.05f);
					stamina.AttackSpeedDebuffMultiplier += 0.0175f;
					stamina.MoveSpeedDebuffMultiplier += 0.0125f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.025f;
				});
			StaminaBuffsTrackingNPC.AddStaminaBossBuff(ModContent.NPCType<DraekP2Head>(),
				"Defeating the serpentine master of the Forest has taught you to steady your form, allowing you to use your Stamina for longer and slightly increasing its benefits." +
					"\n Idle increase rate: +18%, Active use rate: -7.5%" +
					"\n Active attack speed rate: +1.25%" +
					"\n Active move acceleration rate: +2%, Active max move speed: +2%" +
					"\n Maximum Stamina: +1500 units",
				stamina => {
					stamina.AddEffects(incMult: 0.18f, decMult: -0.075f, maxAdd: 1500);
					stamina.AttackSpeedBuffMultiplier += 0.0125f;
					stamina.MoveSpeedBuffMultiplier += 0.02f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.02f;
				});
			// TODO: add the rest of the Cosmivengeon boss stuff
			// TODO: add cross-mod support

			//Make certain debuffs show the time remaining
			Main.buffNoTimeDisplay[BuffID.Slimed] = false;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = false;
		}

		public override void PostSetupContent(){
			CrossMod.Load();
		}

		public static void DeactivateCalamityRevengeance(){
			if(!ModReferences.Calamity.Active)
				return;

			if(ModReferences.Calamity.Instance.Version >= new Version("1.4.2.108"))
				ModReferences.Calamity.Call("SetDifficulty", "Rev", false);
			else
				ModReferences.Calamity.Instance.GetModWorld("CalamityWorld").GetType().GetField("revenge", BindingFlags.Public | BindingFlags.Static).SetValue(null, false);
		}

		public static void DeactivateCalamityDeath(){
			if(!ModReferences.Calamity.Active)
				return;

			if(ModReferences.Calamity.Instance.Version >= new Version("1.4.2.108"))
				ModReferences.Calamity.Call("SetDifficulty", "Death", false);
			else
				ModReferences.Calamity.Instance.GetModWorld("CalamityWorld").GetType().GetField("death", BindingFlags.Public | BindingFlags.Static).SetValue(null, false);
		}

		public override void UpdateUI(GameTime gameTime)/* tModPorter Note: Removed. Use ModSystem.UpdateUI */{
			StaminaUI.Visible = !Main.gameMenu && WorldEvents.desoMode;
			if(StaminaUI.Visible)
				userInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)/* tModPorter Note: Removed. Use ModSystem.ModifyInterfaceLayers */{
			//Copied from ExampleMod :thinkies:
			int mouseIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if(mouseIndex != -1){
				layers.Insert(mouseIndex, new LegacyGameInterfaceLayer(
					"CosmivengeonMod: Stamina UI",
					delegate{
						if(StaminaUI.Visible)
							userInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}

		public override void AddRecipeGroups()/* tModPorter Note: Removed. Use ModSystem.AddRecipeGroups */{
			RegisterRecipeGroup(RecipeGroups.EvilDrops, ItemID.ShadowScale, new int[]{
				ItemID.ShadowScale, ItemID.TissueSample
			});
			RegisterRecipeGroup(RecipeGroups.EvilBars, ItemID.DemoniteBar, new int[]{
				ItemID.DemoniteBar, ItemID.CrimtaneBar
			});
			RegisterRecipeGroup(RecipeGroups.Tier4Bars, ItemID.GoldBar, new int[]{
				ItemID.GoldBar, ItemID.PlatinumBar
			});
			RegisterRecipeGroup(RecipeGroups.WeirdPlant, ItemID.StrangePlant1, new int[]{
				ItemID.StrangePlant1, ItemID.StrangePlant2, ItemID.StrangePlant3, ItemID.StrangePlant4
			});
		}

		public override void Unload(){
			StaminaHotKey = null;
			staminaUI = null;
			userInterface = null;

			ModReferences.Unload();

			PrefixManager.Unload();

			StaminaBuffsTrackingNPC.Unload();

			if(FilterCollection.Screen_EoC?.Active ?? false)
				FilterCollection.Screen_EoC.Deactivate();

			//Restore the original setting for the buffs
			Main.buffNoTimeDisplay[BuffID.Slimed] = true;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = true;

			BossPackage.bossInfo = null;
		}

		private static void RegisterRecipeGroup(string groupName, int itemForAnyName, int[] validTypes)
			=> RecipeGroup.RegisterGroup(groupName, new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(itemForAnyName)}", validTypes));

		public override void HandlePacket(BinaryReader reader, int whoAmI){
			MessageType message = (MessageType)reader.ReadByte();

			switch(message){
				case MessageType.SyncPlayer:
					byte clientWhoAmI = reader.ReadByte();
					StaminaPlayer mp = Main.player[clientWhoAmI].GetModPlayer<StaminaPlayer>();
					mp.stamina.ReceiveData(reader);
					break;
				case MessageType.StaminaChanged:
					clientWhoAmI = reader.ReadByte();
					mp = Main.player[clientWhoAmI].GetModPlayer<StaminaPlayer>();
					mp.stamina.ReceiveData(reader);

					if(Main.netMode == NetmodeID.Server){
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

		public override object Call(params object[] args){
			/*		Possible commands:
			 *	"GetDifficulty"/"Difficulty", "Desolation"/"desoMode"/"deso"
			 *	"SetDifficulty",              "Desolation"/"desoMode"/"deso", true/false
			 */
			if(args.Length == 2
					&& new string[]{ "getdifficulty", "difficulty" }.Contains(((string)args[0]).ToLower())
					&& new string[]{ "desolation", "desomode", "deso" }.Contains(((string)args[1]).ToLower())){
				return WorldEvents.desoMode;
			}else if(args.Length == 3
					&& (string)args[0] == "SetDifficulty"
					&& new string[]{ "desolation", "desomode", "deso" }.Contains(((string)args[1]).ToLower())){
				if(bool.TryParse((string)args[2], out bool value)){
					WorldEvents.desoMode = value;
					return value;
				}
			}

			return null;
		}
	}
}