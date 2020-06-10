using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs{
	public class BabyIceProwlerBuff : ModBuff{
		public override void SetDefaults(){
			DisplayName.SetDefault("Baby Ice Prowler");
			Description.SetDefault("A baby Ice Prowler will protect you.");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex){
			CosmivengeonPlayer modPlayer = player.GetModPlayer<CosmivengeonPlayer>();
			if(player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summons.BabyIceProwler>()] > 0)
				modPlayer.babyProwler = true;
			if(!modPlayer.babyProwler){
				player.DelBuff(buffIndex);
				buffIndex--;
			}else
				player.buffTime[buffIndex] = 18000;
		}
	}
}
