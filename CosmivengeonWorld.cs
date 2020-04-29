using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace CosmivengeonMod {
	public class CosmivengeonWorld : ModWorld{
		private const int saveVersion = 413;
		public static bool desoMode;
		public static bool downedDraekBoss;
		public static bool downedFrostbiteBoss;

		public static bool obtainedLore_DraekBoss;
		public static bool obtainedLore_FrostbiteBoss;

		public static bool obtainedDesolator_DraekBoss;

		public override void Initialize(){
			//World flags
			desoMode = false;
			downedDraekBoss = false;
			downedFrostbiteBoss = false;
			obtainedLore_DraekBoss = false;
			obtainedLore_FrostbiteBoss = false;
			obtainedDesolator_DraekBoss = false;
		}

		public override TagCompound Save(){
			List<string> downed = new List<string>();

			if(downedDraekBoss)
				downed.Add("draek");
			if(downedFrostbiteBoss)
				downed.Add("frostbite");

			return new TagCompound{
				["downed"] = downed,
				["desolation"] = desoMode,
				["lore_Draek"] = obtainedLore_DraekBoss,
				["lore_Frostbite"] = obtainedLore_FrostbiteBoss,
				["desolator_Draek"] = obtainedDesolator_DraekBoss
			};
		}

		public override void Load(TagCompound tag){
			var downed = tag.GetList<string>("downed");
			downedDraekBoss = downed.Contains("draek");
			downedFrostbiteBoss = downed.Contains("frostbite");
			desoMode = tag.GetBool("desolation");
			obtainedLore_DraekBoss = tag.GetBool("lore_Draek");
			obtainedLore_FrostbiteBoss = tag.GetBool("lore_Frostbite");
			obtainedDesolator_DraekBoss = tag.GetBool("desolator_Draek");

			SetModFlags();
		}

		public override void LoadLegacy(BinaryReader reader){
			int loadVersion = reader.ReadInt32();
			if(loadVersion == saveVersion){
				BitsByte flags = reader.ReadByte();
				downedDraekBoss = flags[0];
				desoMode = flags[1];
				obtainedLore_DraekBoss = flags[2];
				obtainedDesolator_DraekBoss = flags[3];
				downedFrostbiteBoss = flags[4];
				obtainedLore_FrostbiteBoss = flags[5];

				SetModFlags();
			}else
				mod.Logger.WarnFormat("CosmivengeonMod:  Unknown loadVersion: {0}", loadVersion);
		}

		public override void NetSend(BinaryWriter writer){
			writer.Write(saveVersion);
			BitsByte flags = new BitsByte();
			flags[0] = downedDraekBoss;
			flags[1] = desoMode;
			flags[2] = obtainedLore_DraekBoss;
			flags[3] = obtainedDesolator_DraekBoss;
			flags[4] = downedFrostbiteBoss;
			flags[5] = obtainedLore_FrostbiteBoss;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader){
			reader.ReadInt32();	//ignore the "saveVersion" write
			BitsByte flags = reader.ReadByte();
			downedDraekBoss = flags[0];
			desoMode = flags[1];
			obtainedLore_DraekBoss = flags[2];
			obtainedDesolator_DraekBoss = flags[3];
			downedFrostbiteBoss = flags[4];
			obtainedLore_FrostbiteBoss = flags[5];
		}

		public static void CheckWorldFlagUpdate(string flag){
			FieldInfo field = typeof(CosmivengeonWorld).GetField(flag, BindingFlags.Public | BindingFlags.Static);
			bool currentValue = (bool)field.GetValue(null);

			if(!currentValue){
				field.SetValue(null, true);
				if(Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.WorldData);	//Immediately inform clients of new world state
			}
		}

		private void SetModFlags(){
			//Mod Flags
			CosmivengeonMod.debug_toggleDesoMode = true;
			CosmivengeonMod.debug_canUseExpertModeToggle = false;
			CosmivengeonMod.debug_canUsePotentiometer = false;
			CosmivengeonMod.debug_canUseCrazyHand = true;
			CosmivengeonMod.debug_canUseCalamityChecker = false;
			CosmivengeonMod.debug_canClearBossIDs = true;

			CosmivengeonMod.allowModFlagEdit = true;
			CosmivengeonMod.allowWorldFlagEdit = false;
			CosmivengeonMod.allowTimeEdit = true;
			CosmivengeonMod.allowStaminaNoDecay = false;
		}
	}
}