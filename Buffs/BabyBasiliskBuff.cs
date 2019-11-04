using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class BabyBasiliskBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Baby Basilisk");
			Description.SetDefault("A baby basilisk will protect you.\nWho knew a potential descendant of Draek could be so cute!");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			CosmivengeonPlayer modPlayer = player.GetModPlayer<CosmivengeonPlayer>();
			if(player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summons.BabySnek>()] > 0)
				modPlayer.babySnek = true;
			if(!modPlayer.babySnek){
				player.DelBuff(buffIndex);
				buffIndex--;
			}else
				player.buffTime[buffIndex] = 18000;
		}
	}
}