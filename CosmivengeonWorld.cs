using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod {
	public class CosmivengeonWorld : ModWorld{
		private const int saveVersion = 413;
		public static bool desoMode;
		public static bool downedDraekBoss;

		public static bool obtainedLore_DraekBoss;

		public static bool obtainedDesolator_DraekBoss;

		public override void Initialize(){
			desoMode = false;
			downedDraekBoss = false;
			obtainedLore_DraekBoss = false;
			obtainedDesolator_DraekBoss = false;
		}

		public override TagCompound Save(){
			List<string> downed = new List<string>();

			if(downedDraekBoss)
				downed.Add("draek");

			return new TagCompound{
				["downed"] = downed,
				["desolation"] = desoMode,
				["lore_Draek"] = obtainedLore_DraekBoss,
				["desolator_Draek"] = obtainedDesolator_DraekBoss
			};
		}

		public override void Load(TagCompound tag){
			var downed = tag.GetList<string>("downed");
			downedDraekBoss = downed.Contains("draek");
			desoMode = tag.GetBool("desolation");
			obtainedLore_DraekBoss = tag.GetBool("lore_Draek");
			obtainedDesolator_DraekBoss = tag.GetBool("desolator_Draek");
		}

		public override void LoadLegacy(BinaryReader reader){
			int loadVersion = reader.ReadInt32();
			if(loadVersion == saveVersion){
				BitsByte flags = reader.ReadByte();
				downedDraekBoss = flags[0];
				desoMode = flags[1];
				obtainedLore_DraekBoss = flags[2];
				obtainedDesolator_DraekBoss = flags[3];
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
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader){
			reader.ReadInt32();	//ignore the "saveVersion" write
			BitsByte flags = reader.ReadByte();
			downedDraekBoss = flags[0];
			desoMode = flags[1];
			obtainedLore_DraekBoss = flags[2];
			obtainedDesolator_DraekBoss = flags[3];
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
	}
}