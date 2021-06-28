using CosmivengeonMod.Buffs.Harmful;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class BuffPlayer : ModPlayer{
		public bool primordialWrath;
		public bool rotting;

		public override void ResetEffects(){
			primordialWrath = false;
			rotting = false;
		}

		public override void UpdateBadLifeRegen(){
			if(rotting){
				if(player.lifeRegen > 0){
					player.lifeRegen -= 4 * 2;
					if(player.lifeRegen < 0)
						player.lifeRegen = 0;
				}

				player.statDefense -= 10;
				player.endurance -= 0.05f;
			}
			if(primordialWrath){
				if(player.lifeRegen > 0)
					player.lifeRegen = 0;
				player.statDefense -= 10;
				player.endurance -= 0.1f;
				player.lifeRegen -= 15 * 2;
			}
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource){
			if(player.HasBuff(ModContent.BuffType<PrimordialWrath>()))
				damageSource = PlayerDeathReason.ByCustomReason($"{player.name}'s flesh was melted off.");
			return true;
		}
	}
}
