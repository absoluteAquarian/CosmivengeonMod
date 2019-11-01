using System;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Linq;

namespace CosmivengeonMod{
	public class CosmivengeonMod : Mod{
		public static bool debug_toggleDesoMode = false;
		public static bool debug_canUseExpertModeToggle = false;
		public static bool debug_canUsePotentiometer = false;
		public static bool allowModFlagEdit = true;

		//Mod instance properties
		private static bool bossCheckListInstanceLoadAttempted = false;
		private static Mod bossCheckListInstance;
		public static Mod BossChecklistInstance{
			get{
				if(!bossCheckListInstanceLoadAttempted){
					bossCheckListInstanceLoadAttempted = true;
					bossCheckListInstance = ModLoader.GetMod("BossChecklist");
				}
				return bossCheckListInstance;
			}
		}
		public static bool BossChecklistActive => BossChecklistInstance != null;

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
	}

	internal enum CosmivengeonModMessageType : byte{
		SyncPlayer
	}
}