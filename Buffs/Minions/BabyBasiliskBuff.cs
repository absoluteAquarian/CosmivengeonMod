using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Summons;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Minions{
	public class BabyBasiliskBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Baby Basilisk");
			Description.SetDefault("A baby basilisk will protect you.\nWho knew a potential descendant of Draek could be so cute!");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			MinionPlayer modPlayer = player.GetModPlayer<MinionPlayer>();
			if(player.ownedProjectileCounts[ModContent.ProjectileType<BabySnek>()] > 0)
				modPlayer.babySnek = true;

			if(!modPlayer.babySnek){
				player.DelBuff(buffIndex);
				buffIndex--;
			}else
				player.buffTime[buffIndex] = 18000;
		}
	}
}
