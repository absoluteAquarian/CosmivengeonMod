using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;
using System.Reflection;

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode = false;
		public static bool debug_canUseExpertModeToggle = false;
		public static bool debug_canUsePotentiometer = false;
		public static bool allowModFlagEdit = true;
		public static bool allowWorldFlagEdit = true;
		public static bool allowTimeEdit = true;

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

		public CosmivengeonMod(){
			
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
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer
	}
}