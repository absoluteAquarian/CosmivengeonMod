using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod {
	public class CosmivengeonWorld : ModWorld{
		private const int saveVersion = 0;
		public static bool desoMode;
		public static bool downedDraekBoss;

		public override void Initialize() {
			downedDraekBoss = false;
			desoMode = false;
		}

		public override TagCompound Save(){
			var downed = new List<string>();

			if(downedDraekBoss)
				downed.Add("draek");

			return new TagCompound{
				["downed"] = downed,
				["desolation"] = desoMode
			};
		}

		public override void Load(TagCompound tag){
			var downed = tag.GetList<string>("downed");
			downedDraekBoss = downed.Contains("draek");
			desoMode = tag.GetBool("desolation");
		}

		public override void LoadLegacy(BinaryReader reader){
			int loadVersion = reader.ReadInt32();
			if(loadVersion == 0){
				BitsByte flags = reader.ReadByte();
				downedDraekBoss = flags[0];
				desoMode = flags[1];
			}else
				mod.Logger.WarnFormat("CosmivengeonMod:  Unknown loadVersion: {0}", loadVersion);
		}

		public override void NetSend(BinaryWriter writer){
			writer.Write(saveVersion);
			BitsByte flags = new BitsByte();
			flags[0] = downedDraekBoss;
			flags[1] = desoMode;
			writer.Write(flags);
		}

		public override void NetReceive(BinaryReader reader){
			reader.ReadInt32();	//ignore the "saveVersion" write
			BitsByte flags = reader.ReadByte();
			downedDraekBoss = flags[0];
			desoMode = flags[1];
		}
	}
}