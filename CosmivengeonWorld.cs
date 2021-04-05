using CosmivengeonMod.Projectiles.Desomode;
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
			//Stop all custom sounds
			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile proj = Main.projectile[i];

				if(!proj.active)
					continue;

				if(proj.modProjectile is BrainPsychicMine mine)
					mine.teleport?.Stop();
				else if(proj.modProjectile is BrainPsychicLightning lightning)
					lightning.zap?.Stop();
			}

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

		public override void PreUpdate(){
			if(!NPC.AnyNPCs(NPCID.EyeofCthulhu) && FilterCollection.Screen_EoC.Active)
				FilterCollection.Screen_EoC.Deactivate();
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
			CosmivengeonMod.debug_showEoWOutlines =         !CosmivengeonMod.Release;

			CosmivengeonMod.debug_canUseExpertModeToggle =	!CosmivengeonMod.Release;
			CosmivengeonMod.debug_canUsePotentiometer =		!CosmivengeonMod.Release;
			CosmivengeonMod.debug_canUseCrazyHand =			!CosmivengeonMod.Release;
			CosmivengeonMod.debug_canUseCalamityChecker =	!CosmivengeonMod.Release;
			CosmivengeonMod.debug_canClearBossIDs =			!CosmivengeonMod.Release;
			CosmivengeonMod.debug_canShowEoWOutlines =		!CosmivengeonMod.Release;
			CosmivengeonMod.debug_fastDiceOfFateRecharge =  !CosmivengeonMod.Release;

			CosmivengeonMod.allowModFlagEdit =				!CosmivengeonMod.Release;
			CosmivengeonMod.allowWorldFlagEdit =			!CosmivengeonMod.Release;
			CosmivengeonMod.allowTimeEdit =					!CosmivengeonMod.Release;
			CosmivengeonMod.allowStaminaNoDecay =			!CosmivengeonMod.Release;
		}
	}
}