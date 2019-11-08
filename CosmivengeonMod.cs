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

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode;
		public static bool debug_canUseExpertModeToggle;
		public static bool debug_canUsePotentiometer;

		public static bool allowModFlagEdit = true;
		public static bool allowWorldFlagEdit;
		public static bool allowTimeEdit;
		public static bool allowStaminaNoDecay;

		//Stamina use hotkey
		public static ModHotKey StaminaHotKey;

		public static CosmivengeonMod Instance;

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
					calamityInstance = ModLoader.GetMod("BossChecklist");
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

		public override void Load(){
			StaminaHotKey = RegisterHotKey("Toggle Stamina Use", "G");

			//Only run this segment if we're not loading on a server
			if(!Main.dedServ){
				staminaUI = new StaminaUI();
				staminaUI.Activate();

				userInterface = new UserInterface();
				userInterface.SetState(staminaUI);
			}

			Instance = this;
		}

		public override void PostSetupContent(){
			//Set the boss's position in BossChecklist if the mod is active
			if(BossChecklistActive){
				//2.7f ==> just before Eater of Worlds
				BossChecklistInstance.Call("AddBossWithInfo",
					"Draek",
					2.7f,
					(Func<bool>)(() => CosmivengeonWorld.downedDraekBoss),
					$"Use a [i:{ModContent.ItemType<Items.Draek.DraekSummon>()}] in the Purity biome."
				);
			}
		}

		public static bool CalamityRevengeanceActive(){
			if(!CalamityActive)
				return false;

			ModWorld world = CalamityInstance.GetModWorld("CalamityWorld");

			FieldInfo field = world.GetType().GetField("revenge", BindingFlags.Public | BindingFlags.Static);

			return (bool)field.GetValue(null);
		}

		public static bool CalamityDeathActive(){
			if(!CalamityActive)
				return false;

			ModWorld world = CalamityInstance.GetModWorld("CalamityWorld");

			FieldInfo field = world.GetType().GetField("death", BindingFlags.Public | BindingFlags.Static);

			return (bool)field.GetValue(null);
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
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer
	}
}