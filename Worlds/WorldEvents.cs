using CosmivengeonMod.Projectiles.Desomode;
using CosmivengeonMod.Utility;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Worlds{
	public class WorldEvents : ModWorld{
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
			if(Main.gameMenu){
				for(int i = 0; i < Main.maxProjectiles; i++){
					Projectile proj = Main.projectile[i];

					if(!proj.active)
						continue;

					if(proj.modProjectile is BrainPsychicMine mine)
						mine.teleport?.Stop();
					else if(proj.modProjectile is BrainPsychicLightning lightning)
						lightning.zap?.Stop();
				}
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

			Debug.InitializeFlags();
		}

		public override void NetSend(BinaryWriter writer){
			BitsByte flags = new BitsByte();
			flags[0] = downedDraekBoss;
			flags[1] = desoMode;
			flags[2] = obtainedLore_DraekBoss;
			flags[3] = obtainedDesolator_DraekBoss;
			flags[4] = downedFrostbiteBoss;
			flags[5] = obtainedLore_FrostbiteBoss;
			writer.Write(flags);

			Debug.NetSend(writer);
		}

		public override void NetReceive(BinaryReader reader){
			BitsByte flags = reader.ReadByte();
			downedDraekBoss = flags[0];
			desoMode = flags[1];
			obtainedLore_DraekBoss = flags[2];
			obtainedDesolator_DraekBoss = flags[3];
			downedFrostbiteBoss = flags[4];
			obtainedLore_FrostbiteBoss = flags[5];

			Debug.NetReceive(reader);
		}
	}
}
