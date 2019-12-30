using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
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

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode;
		public static bool debug_canUseExpertModeToggle;
		public static bool debug_canUsePotentiometer;
		public static bool debug_canUseCrazyHand;
		public static bool debug_canUseCalamityChecker;

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
			}
		}

		public override void Unload(){
			StaminaHotKey = null;
			staminaUI = null;
			userInterface = null;
			calamityInstance = null;
			calamityInstanceLoadAttempted = false;
			bossCheckListInstance = null;
			bossCheckListInstanceLoadAttempted = false;
		}

		public override void PostSetupContent(){
			//Set the boss's position in BossChecklist if the mod is active
			//see:  https://github.com/JavidPack/BossChecklist/wiki/Support-using-Mod-Call
			
			if(BossChecklistActive){
				//2.7f ==> just before Eater of Worlds
				_ = BossChecklistInstance.Call("AddBoss",
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
				_ = BossChecklistInstance.Call("AddBoss",
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
			RecipeGroup.RegisterGroup("Cosmivengeon: Evil Drops",
				new RecipeGroup(() => $"{Language.GetTextValue("LegacyMisc.37")} {Lang.GetItemNameValue(ItemID.ShadowScale)}",
					new int[]{ ItemID.ShadowScale, ItemID.TissueSample }));
		}
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer
	}
}