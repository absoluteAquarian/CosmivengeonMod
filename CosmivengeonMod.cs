using CosmivengeonMod.Buffs.Stamina;
using CosmivengeonMod.Detours;
using CosmivengeonMod.Items.Boss_Bags;
using CosmivengeonMod.Items.Draek;
using CosmivengeonMod.Items.Frostbite;
using CosmivengeonMod.Items.Masks;
using CosmivengeonMod.ModEdits;
using CosmivengeonMod.NPCs.Desomode;
using CosmivengeonMod.NPCs.Draek;
using CosmivengeonMod.NPCs.Frostbite;
using CosmivengeonMod.UI;
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
	public class CosmivengeonMod : Mod{
		public static CosmivengeonMod Instance => ModContent.GetInstance<CosmivengeonMod>();

		/// <summary>
		/// Whether or not the mod will log its IL edits.  Disable this flag to make the mod load faster!
		/// </summary>
		public static readonly bool LogILEdits = false;

		/// <summary>
		/// Whether or not this version of the mod is a public release or a test build.
		/// </summary>
		public static readonly bool Release = true;

		public static bool debug_toggleDesoMode;
		public static bool debug_canUseExpertModeToggle;
		public static bool debug_canUsePotentiometer;
		public static bool debug_canUseCrazyHand;
		public static bool debug_canUseCalamityChecker;
		public static bool debug_canClearBossIDs;
		public static bool debug_showEoWOutlines;
		public static bool debug_canShowEoWOutlines;

		public static bool allowModFlagEdit;
		public static bool allowWorldFlagEdit;
		public static bool allowTimeEdit;
		public static bool allowStaminaNoDecay;

		/// <summary>
		/// "Description to be added..."
		/// </summary>
		public static readonly string PlaceHolderDescription = "Description to be added...";

		/// <summary>
		/// "THIS IS A DEBUG ITEM"
		/// </summary>
		public static readonly string DebugItemDescription = "[c/555555:THIS IS A DEBUG ITEM]";

		//Stamina use hotkey
		public static ModHotKey StaminaHotKey;

		//UI
		private StaminaUI staminaUI;
		private UserInterface userInterface;

		/// <summary>
		/// "Cosmivengeon: Evil Drops" - "Any Shadow Scale"
		/// </summary>
		public static readonly string RecipeGroup_EvilDrops = "Cosmivengeon: Evil Drops";

		public static BasicEffect PrimitivesEffect{ get; private set; }

		public CosmivengeonMod(){ }

		public override object Call(params object[] args){
			/*		Possible commands:
			 *	"GetDifficulty"/"Difficulty", "Desolation"/"desoMode"/"deso"
			 *	"SetDifficulty", "Desolation"/"desoMode"/"deso", true/false
			 */
			if(args.Length == 2
					&& new string[]{ "getdifficulty", "difficulty" }.Contains(((string)args[0]).ToLower())
					&& new string[]{ "desolation", "desomode", "deso" }.Contains(((string)args[1]).ToLower())){
				return CosmivengeonWorld.desoMode;
			}else if(args.Length == 3
					&& (string)args[0] == "SetDifficulty"
					&& new string[]{ "desolation", "desomode", "deso" }.Contains(((string)args[1]).ToLower())){
				if(bool.TryParse((string)args[2], out bool value)){
					CosmivengeonWorld.desoMode = value;
					return value;
				}
			}

			return null;
		}

		public override void Load(){
			StaminaHotKey = RegisterHotKey("Toggle Stamina Use", "G");

			//Only run this segment if we're not loading on a server
			if(!Main.dedServ && Main.netMode != NetmodeID.Server){
				staminaUI = new StaminaUI();
				staminaUI.Activate();

				userInterface = new UserInterface();
				userInterface.SetState(staminaUI);

				//Add music boxes
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Frigid_Feud"), ModContent.ItemType<Items.MusicBoxes.FrostbiteBox>(), ModContent.TileType<Tiles.FrostbiteBox>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel"), ModContent.ItemType<Items.MusicBoxes.DraekBox>(), ModContent.TileType<Tiles.DraekBox>());

				Ref<Effect> eocEffect = new Ref<Effect>(GetEffect("Effects/screen_eoc"));

				FilterCollection.Screen_EoC = new Filter(new ScreenShaderData(eocEffect, "ScreenDarken"), EffectPriority.High);
			}

			ILHelper.InitMonoModDumps();

			DetourNPC.Load();
			DetourPlayer.Load();

			ModReferences.Load();

			//IL for BossChecklist isn't required anymore
			//ModEdits.BossChecklist.Load();
			ModEdits.CheatSheet.Load();
			ModEdits.Vanilla.Load();

			ILHelper.DeInitMonoModDumps();
		}

		public override void Unload(){
			StaminaHotKey = null;
			staminaUI = null;
			userInterface = null;
			
			ModReferences.Unload();

			StaminaBuffsGlobalNPC.BossIDs = null;
			StaminaBuffsGlobalNPC.BuffActions = null;
			StaminaBuffsGlobalNPC.OnKillMessages = null;
			StaminaBuffsGlobalNPC.BossNames = null;

			DetourNPC.Unload();

			//Restore the original setting for the buffs
			Main.buffNoTimeDisplay[BuffID.Slimed] = true;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = true;

			if(FilterCollection.Screen_EoC?.Active ?? false)
				FilterCollection.Screen_EoC.Deactivate();

			ModEdits.BossChecklist.Unload();
			ModEdits.CheatSheet.Unload();
		}

		public override void PostSetupContent(){
			//Set the boss's position in BossChecklist if the mod is active
			//see:  https://github.com/JavidPack/BossChecklist/wiki/Support-using-Mod-Call

			PrimitivesEffect = new BasicEffect(Main.graphics.GraphicsDevice){
				VertexColorEnabled = true
			};

			if(ModReferences.BossChecklist.Active){
				//2.7f ==> just before Eater of Worlds
				ModReferences.BossChecklist.Call("AddBoss",
					2.7f,
					new List<int>(){
						ModContent.NPCType<Draek>(),
						ModContent.NPCType<DraekP2Head>()
					},
					this,
					$"${ModContent.GetInstance<Draek>().DisplayName.Key}",
					(Func<bool>)(() => CosmivengeonWorld.downedDraekBoss),
					ModContent.ItemType<DraekSummon>(),
					new List<int>(){
						ModContent.ItemType<DraekMask>(),
						ModContent.ItemType<StoneTablet>(),
						ModContent.ItemType<TerraBolt>()
					},
					new List<int>(){
						ModContent.ItemType<BasiliskStaff>(),
						ModContent.ItemType<BoulderChunk>(),
						ModContent.ItemType<EarthBolt>(),
						ModContent.ItemType<ForsakenOronoblade>(),
						ModContent.ItemType<RockslideYoyo>(),
						ModContent.ItemType<Scalestorm>(),
						ModContent.ItemType<SlitherWand>(),
						ModContent.ItemType<Stoneskipper>(),
						ModContent.ItemType<DraekBag>(),
						ItemID.LesserHealingPotion
					},
					$"Use a [i:{ModContent.ItemType<DraekSummon>()}] in the Forest biome.",
					null,  //Ignoring custom despawn message
					"CosmivengeonMod/NPCs/Draek/Draek_BossLog"
				);

				//1.5f ==> between Slime King and Eye of Cthulhu
				ModReferences.BossChecklist.Call("AddBoss",
					1.5f,
					ModContent.NPCType<Frostbite>(),
					this,
					$"${ModContent.GetInstance<Frostbite>().DisplayName.Key}",
					(Func<bool>)(() => CosmivengeonWorld.downedFrostbiteBoss),
					ModContent.ItemType<IcyLure>(),
					new List<int>(){
						ModContent.ItemType<BabyCloudBottle>(),
						ModContent.ItemType<FrostbiteMask>(),
						ModContent.ItemType<IceforgedRelic>()
					},
					new List<int>(){
						ModContent.ItemType<FrostbiteFlamethrower>(),
						ModContent.ItemType<SnowballFlail>(),
						ModContent.ItemType<IceDisk>(),
						ModContent.ItemType<BlizzardRod>(),
						ModContent.ItemType<FrostRifle>(),
						ModContent.ItemType<SubZero>(),
						ModContent.ItemType<IceScepter>(),
						ModContent.ItemType<FrostDemonHorn>(),
						ModContent.ItemType<SnowscaleCoat>(),
						ModContent.ItemType<FrostbiteBag>(),
						ItemID.LesserHealingPotion
					},
					$"Use a [i:{ModContent.ItemType<IcyLure>()}] in the Snow biome.",
					null,  //Ignoring custom despawn message
					"CosmivengeonMod/NPCs/Frostbite/Frostbite_BossLog"
				);
			}

			//Initialize the stamina buff dictionary
	/*		StaminaBuffsGlobalNPC.BossKilled = new Dictionary<int, bool>(){
				//Vanilla bosses
				[NPCID.KingSlime] = false,
				[NPCID.EyeofCthulhu] = false,
				[NPCID.EaterofWorldsHead] = false,
				[NPCID.BrainofCthulhu] = false,
				[NPCID.QueenBee] = false,
				[NPCID.SkeletronHead] = false,
				[NPCID.WallofFlesh] = false,
				[NPCID.Retinazer] = false,
				[NPCID.SkeletronPrime] = false,
				[NPCID.TheDestroyer] = false,
				[NPCID.Plantera] = false,
				[NPCID.Golem] = false,
				[NPCID.DukeFishron] = false,
				[NPCID.CultistBoss] = false,
				[NPCID.MoonLordCore] = false,
				//Vanilla minibosses
				[NPCID.DD2DarkMageT1] = false,
				[NPCID.DD2OgreT2] = false,
				[NPCID.DD2Betsy] = false,
				[NPCID.MourningWood] = false,
				[NPCID.Pumpking] = false,
				[NPCID.Everscream] = false,
				[NPCID.SantaNK1] = false,
				[NPCID.IceQueen] = false,
				//Cosmivengeon bosses
				[ModContent.NPCType<Frostbite>()] = false,
				[ModContent.NPCType<DraekP2Head>()] = false
			};
	*/
			StaminaBuffsGlobalNPC.BossIDs = new List<int>();
			StaminaBuffsGlobalNPC.BuffActions = new Dictionary<int, Action<Stamina>>();
			StaminaBuffsGlobalNPC.OnKillMessages = new Dictionary<int, string>();
			SetBossNamesDictionary();

			//Vanilla bosses
			AddStaminaBossBuff(NPCID.KingSlime,
				"Defeating the monarch of slime has loosened up your muscles, allowing you to use Stamina for longer and recover from Exhaustion faster." +
					"\nIdle increase rate: +10%, Exhaustion increase rate: +6%, Active use rate: -3.5%, Maximum Stamina: +1000 units",
				stamina => {
					stamina.AddEffects(incMult: 0.1f, exIncMult: 0.06f, decMult: -0.035f, maxAdd: 1000);
				});
			AddStaminaBossBuff(NPCID.EyeofCthulhu,
				"Defeating the master observer of the night has honed your senses, allowing you to move and attack faster while in the Active state." +
					"\nActive attack speed rate: +8%, Active move acceleration rate: +6%, Active max move speed: +5%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.08f;
					stamina.MoveSpeedBuffMultiplier += 0.06f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.05f;
				});
			AddStaminaBossBuff(NPCID.EaterofWorldsHead,
				"Defeating the grotesque harbinger from the Corruption has strengthened your resolve, reducing the harmful effects from Exhaustion." +
					"\nExhaustion attack speed rate: +3%, Exhaustion move acceleration rate: +4%, Exhaustion max move speed: +5.75%, Exhausted increase rate: +6%",
				stamina => {
					stamina.AddEffects(exIncMult: 0.06f);
					stamina.AttackSpeedDebuffMultiplier += 0.03f;
					stamina.MoveSpeedDebuffMultiplier += 0.04f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.0575f;
				});
			AddStaminaBossBuff(NPCID.BrainofCthulhu,
				"Defeating the Crimson's mastermind has sharpened your wits, letting you react faster to your surroundings." +
					"\nActive attack speed rate: +6.5%, Active move acceleration rate: +7%, Active max move speed: +4%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.065f;
					stamina.MoveSpeedBuffMultiplier += 0.07f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.04f;
				});
			AddStaminaBossBuff(NPCID.QueenBee,
				"Defeating the monarch of the jungle has improved your resilience to Exhaustion and your overall abilities while in the Active state." +
					"\nActive attack speed rate: +4%, Active move acceleration rate: +3%, Exhaustion move acceleration rate: +1.5%, Exhaustion attack speed rate: +2%",
				stamina => {
					stamina.AttackSpeedBuffMultiplier += 0.04f;
					stamina.MoveSpeedBuffMultiplier += 0.03f;
					stamina.MoveSpeedDebuffMultiplier += 0.015f;
					stamina.AttackSpeedDebuffMultiplier += 0.02f;
				});
			AddStaminaBossBuff(NPCID.SkeletronHead,
				"Defeating the cursed guardian of the Dungeon has further increased your control over your Stamina." +
					"\nAll Active buffs: +5%, All Exhaustion debuffs: +3.125%, Idle increase rate: +22.5%, Active use rate: -6%, Maximum Stamina: +2500 units",
				stamina => {
					stamina.AddEffects(incMult: 0.225f, exIncMult: 0.08f, decMult: -0.06f, maxAdd: 2500);
					stamina.AttackSpeedBuffMultiplier += 0.05f;
					stamina.MoveSpeedBuffMultiplier += 0.05f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.05f;
					stamina.AttackSpeedDebuffMultiplier += 0.03125f;
					stamina.MoveSpeedDebuffMultiplier += 0.03125f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.03125f;
				});
			// TODO: add the rest of the vanilla boss stuff
			//Vanilla minibosses
			// TODO: add the vanilla miniboss stuff
			//Cosmivengeon bosses
			AddStaminaBossBuff(ModContent.NPCType<Frostbite>(),
				"Defeating the mutant frost demon has boosted your resistance to the effects of Exhaustion." +
					"\nExhaustion attack speed rate: +2.25%, Exhaustion move acceleration rate: +1.25%, Exhaustion max move speed: +2.5%, Exhaustion increase rate: +5%",
				stamina => {
					stamina.AddEffects(exIncMult: 0.05f);
					stamina.AttackSpeedDebuffMultiplier += 0.0225f;
					stamina.MoveSpeedDebuffMultiplier += 0.0125f;
					stamina.MaxMoveSpeedDebuffMultiplier += 0.025f;
				});
			AddStaminaBossBuff(ModContent.NPCType<DraekP2Head>(),
				"Defeating the serpentine master of the Forest has taught you to steady your form, allowing you to use your Stamina for longer and slightly increasing its benefits." +
					"\nIdle increase rate: +18%, Active use rate: -7.5%, All Active buffs: +2%, Maximum Stamina: +1500 units",
				stamina => {
					stamina.AddEffects(incMult: 0.18f, decMult: -0.075f, maxAdd: 1500);
					stamina.AttackSpeedBuffMultiplier += 0.02f;
					stamina.MoveSpeedBuffMultiplier += 0.02f;
					stamina.MaxMoveSpeedBuffMultiplier += 0.02f;
				});
			// TODO: add the rest of the Cosmivengeon boss stuff
			// TODO: add cross-mod support

			//Make certain debuffs show the time remaining
			Main.buffNoTimeDisplay[BuffID.Slimed] = false;
			Main.buffNoTimeDisplay[BuffID.Obstructed] = false;
		}

		private static void SetBossNamesDictionary(){
			StaminaBuffsGlobalNPC.BossNames = new Dictionary<int, List<string>>(){
				//Vanilla bosses
				[NPCID.KingSlime] =         new List<string>(){ "King Slime", "Slime King", "KS", "SK" },
				[NPCID.EyeofCthulhu] =      new List<string>(){ "Eye of Cthulhu", "EoC" },
				[NPCID.EaterofWorldsHead] = new List<string>(){ "Eater of Worlds", "EoW" },
				[NPCID.BrainofCthulhu] =    new List<string>(){ "Brain of Cthulhu" , "Brain", "BoC"},
				[NPCID.QueenBee] =          new List<string>(){ "Queen Bee", "Bee Queen", "QB", "BQ" },
				[NPCID.SkeletronHead] =     new List<string>(){ "Skeletron", "Skele", "Skelebutt", "Sans" },
				[NPCID.WallofFlesh] =       new List<string>(){ "Wall of Flesh", "Wall of Meat", "WoF" },
				[NPCID.Retinazer] =         new List<string>(){ "The Twins", "Retinazer", "Spazmatism" },
				[NPCID.TheDestroyer] =      new List<string>(){ "The Destroyer", "Destroyer" },
				[NPCID.SkeletronPrime] =    new List<string>(){ "Skeletron Prime", "SkelePrime", "Sans Prime" },
				[NPCID.Plantera] =          new List<string>(){ "Plantera", "Plant" },
				[NPCID.Golem] =             new List<string>(){ "Golem" },
				[NPCID.CultistBoss] =       new List<string>(){ "Lunatic Cultist", "Cultist", "CultistBoss", "Cultist Boss" },
				[NPCID.MoonLordCore] =      new List<string>(){ "Moon Lord", "The Moon Lord" },
				//Vanilla minibosses
				[NPCID.DD2DarkMageT1] =     new List<string>(){ "Dark Mage", "DD2 Mage", "DD2 Dark Mage" },
				[NPCID.DD2OgreT2] =         new List<string>(){ "Ogre", "DD2 Ogre" },
				[NPCID.DD2Betsy] =          new List<string>(){ "Betsy", "DD2 Betsy" },
				[NPCID.MourningWood] =      new List<string>(){ "Mourning Wood", "Morning Wood", "Pumpkin Tree" },
				[NPCID.Pumpking] =          new List<string>(){ "Pumpking" },
				[NPCID.Everscream] =        new List<string>(){ "Everscream", "Frost Tree" },
				[NPCID.SantaNK1] =          new List<string>(){ "Santa-NK1", "SantaNK1", "Mecha Santa", "Santa Boss" },
				[NPCID.IceQueen] =          new List<string>(){ "Ice Queen" },
				//Cosmivengeon bosses
				[ModContent.NPCType<Frostbite>()] =   new List<string>(){ "Frostbite" },
				[ModContent.NPCType<DraekP2Head>()] = new List<string>(){ "Draek" }
			};
		}

		private static void AddStaminaBossBuff(int type, string message, Action<Stamina> action){
			StaminaBuffsGlobalNPC.BossIDs.Add(type);
			StaminaBuffsGlobalNPC.BuffActions.Add(type, action);
			StaminaBuffsGlobalNPC.OnKillMessages.Add(type, message);
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

		public override void UpdateUI(GameTime gameTime){
			StaminaUI.Visible = !Main.gameMenu && CosmivengeonWorld.desoMode;
			if(StaminaUI.Visible){
				userInterface?.Update(gameTime);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers){
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

		public override void AddRecipeGroups(){
			RecipeGroup.RegisterGroup(RecipeGroup_EvilDrops,
				new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.ShadowScale)}",
					new int[]{ ItemID.ShadowScale, ItemID.TissueSample }));
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI){
			CosmivengeonModMessageType message = (CosmivengeonModMessageType)reader.ReadByte();

			switch(message){
				case CosmivengeonModMessageType.SyncPlayer:
					byte clientWhoAmI = reader.ReadByte();
					CosmivengeonPlayer mp = Main.player[clientWhoAmI].GetModPlayer<CosmivengeonPlayer>();
					mp.stamina.ReceiveData(reader);
					break;
				case CosmivengeonModMessageType.StaminaChanged:
					clientWhoAmI = reader.ReadByte();
					mp = Main.player[clientWhoAmI].GetModPlayer<CosmivengeonPlayer>();
					mp.stamina.ReceiveData(reader);

					if(Main.netMode == NetmodeID.Server){
						ModPacket packet = GetPacket();
						packet.Write((byte)CosmivengeonModMessageType.StaminaChanged);
						packet.Write(clientWhoAmI);
						mp.stamina.SendData(packet);
						packet.Send(-1, clientWhoAmI);
					}
					break;
				case CosmivengeonModMessageType.SyncEoWGrab:
					DetourNPCHelper.EoW_GrabbingNPC = reader.ReadInt32();
					DetourNPCHelper.EoW_GrabbedPlayer = reader.ReadInt32();
					break;
				default:
					Logger.WarnFormat("CosmivengeonMod: Unknown message type: {0}", message);
					break;
			}
		}
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer,
		StaminaChanged,
		SyncEoWGrab
	}
}