using CosmivengeonMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Global{
	public class StaminaPunishment : GlobalItem{
		public override void HoldItem(Item item, Player player){
			if(item.IsAir || item.damage <= 0 || !item.channel)
				return;

			var mp = player.GetModPlayer<StaminaPlayer>();

			//If the stamina is recharging, punish the player for attacking
			//Items that "channel" will apply a lesser penalty during the use
			if(mp.stamina.Recharging && mp.stamina.Value < mp.stamina.MaxValue){
				float amt = player.HeldItem.useAnimation / 15f;

				mp.stamina.AddAttackPunishment(amt / 40f);
			}
		}
	}
}
