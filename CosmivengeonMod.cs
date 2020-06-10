using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Terraria.UI;
using CosmivengeonMod.Buffs.Stamina;
using CosmivengeonMod.UI;
using CosmivengeonMod.Items.Frostbite;
using CosmivengeonMod.Items.Masks;
using CosmivengeonMod.Items.Boss_Bags;
using CosmivengeonMod.Items.Draek;
using Terraria.Localization;
using System.IO;
using Terraria.ModLoader;
using Terraria.GameContent.UI;
using CosmivengeonMod.NPCs.Frostbite;
using CosmivengeonMod.NPCs.Draek;
using CosmivengeonMod.Detours;

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode;
		public static bool debug_canUseExpertModeToggle;
		public static bool debug_canUsePotentiometer;
		public static bool debug_canUseCrazyHand;
		public static bool debug_canUseCalamityChecker;
		public static bool debug_canClearBossIDs;

		public static bool allowModFlagEdit;
		public static bool allowWorldFlagEdit;
		public static bool allowTimeEdit;
		public static bool allowStaminaNoDecay;

		public static string PlaceHolderDescription => "Description to be added...";

		//Stamina use hotkey
		public static ModHotKey StaminaHotKey;

		//Mod instance properties
		private static bool bossCheckListInstanceLoadAttempted = false;
		private static Mod bossCheckListInstance;
		private static bool calamityInstanceLoadAttempted = false;
		private static Mod calamityInstance;
		public static Mod BossChecklistInstance{
			get{
				if(!bossCheckListInstanceLoadAttempted){
					bossCheckListInstanceLoadAttempted = true;
					bossCheckListInstance = ModLoader.GetMod("BossChecklist");
				}
				return bossCheckListInstance;
			}
		}
		public static Mod CalamityInstance{
			get{
				if(!calamityInstanceLoadAttempted){
					calamityInstanceLoadAttempted = true;
					calamityInstance = ModLoader.GetMod("CalamityMod");
				}
				return calamityInstance;
			}
		}
		public static bool BossChecklistActive => BossChecklistInstance != null;
		public static bool CalamityActive => CalamityInstance != null;

		//UI
		private StaminaUI staminaUI;
		private UserInterface userInterface;

		/// <summary>
		/// "Cosmivengeon: Evil Drops" - "Any Shadow Scale"
		/// </summary>
		public static readonly string RecipeGroup_EvilDrops = "Cosmivengeon: Evil Drops";

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
				bool value = bool.TryParse((string)args[2], out bool valid);
				if(valid){
					CosmivengeonWorld.desoMode = value;
					return value;
				}
				return null;
			}

			return null;
		}

		public override void Load(){
			StaminaHotKey = RegisterHotKey("Toggle Stamina Use", "G");

			//Only run this segment if we're not loading on a server
			if(!Main.dedServ){
				staminaUI = new StaminaUI();
				staminaUI.Activate();

				userInterface = new UserInterface();
				userInterface.SetState(staminaUI);

				//Add music boxes
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Frigid_Feud"), ModContent.ItemType<Items.MusicBoxes.FrostbiteBox>(), ModContent.TileType<Tiles.FrostbiteBox>());
				AddMusicBox(GetSoundSlot(SoundType.Music, "Sounds/Music/Successor_of_the_Jewel"), ModContent.ItemType<Items.MusicBoxes.DraekBox>(), ModContent.TileType<Tiles.DraekBox>());
			}

			DetourNPC.Load();
		}

		public override void Unload(){
			StaminaHotKey = null;
			staminaUI = null;
			userInterface = null;
			
			calamityInstance = null;
			calamityInstanceLoadAttempted = false;
			bossCheckListInstance = null;
			bossCheckListInstanceLoadAttempted = false;

			StaminaBuffsGlobalNPC.BossIDs = null;
			StaminaBuffsGlobalNPC.BuffActions = null;
			StaminaBuffsGlobalNPC.OnKillMessages = null;
			StaminaBuffsGlobalNPC.BossNames = null;
		}

		public override void PostSetupContent(){
			//Set the boss's position in BossChecklist if the mod is active
			//see:  https://github.com/JavidPack/BossChecklist/wiki/Support-using-Mod-Call

			if(BossChecklistActive){
				//2.7f ==> just before Eater of Worlds
				BossChecklistInstance.Call("AddBoss",
					2.7f,
					new List<int>(){
						ModContent.NPCType<NPCs.Draek.Draek>(),
						ModContent.NPCType<NPCs.Draek.DraekP2Head>()
					},
					this,
					$"${ModContent.GetInstance<NPCs.Draek.Draek>().DisplayName.Key}",
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
				BossChecklistInstance.Call("AddBoss",
					1.5f,
					ModContent.NPCType<NPCs.Frostbite.Frostbite>(),
					this,
					$"${ModContent.GetInstance<NPCs.Frostbite.Frostbite>().DisplayName.Key}",
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
		}

		private static void SetBossNamesDictionary(){
			StaminaBuffsGlobalNPC.BossNames = new Dictionary<int, List<string>>(){
				//Vanilla bosses
				[NPCID.KingSlime] =         new List<string>(){ "King Slime", "Slime King", "KS", "SK" },
				[NPCID.EyeofCthulhu] =      new List<string>(){ "Eye of Cthulhu", "EoC" },
				[NPCID.EaterofWorldsHead] = new List<string>(){ "Eater of Worlds", "EoW" },
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
			if(!CalamityActive)
				return;

			if(CalamityInstance.Version >= new Version("1.4.2.108"))
				CalamityInstance.Call("SetDifficulty", "Rev", false);
			else
				CalamityInstance.GetModWorld("CalamityWorld").GetType().GetField("revenge", BindingFlags.Public | BindingFlags.Static).SetValue(null, false);
		}

		public static void DeactivateCalamityDeath(){
			if(!CalamityActive)
				return;

			if(CalamityInstance.Version >= new Version("1.4.2.108"))
				CalamityInstance.Call("SetDifficulty", "Death", false);
			else
				CalamityInstance.GetModWorld("CalamityWorld").GetType().GetField("death", BindingFlags.Public | BindingFlags.Static).SetValue(null, false);
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
				default:
					Logger.WarnFormat("CosmivengeonMod: Unknown message type: {0}", message);
					break;
			}
		}
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer,
		StaminaChanged
	}
}