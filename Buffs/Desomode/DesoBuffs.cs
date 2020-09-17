﻿using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Desomode{
	public class DesoBuffs : GlobalBuff{
		public override void ModifyBuffTip(int type, ref string tip, ref int rare){
			if(type == BuffID.Slimed)
				tip += "\nIncreased damage taken from slimes\nSlower move speed";
		}
	}
}