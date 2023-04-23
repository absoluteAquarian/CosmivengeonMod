using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Utility.LootRules {
	public class InstancedPlayerLootRule : CommonDrop {
		private readonly int protectionFrames;

		public InstancedPlayerLootRule(int itemId, int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1, int protectionFrames = 18000) : base(itemId, chanceDenominator, amountDroppedMinimum, amountDroppedMaximum, chanceNumerator) {
			this.protectionFrames = protectionFrames;
		}

		public override ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			ItemDropAttemptResult result = default;

			if (info.rng.Next(chanceDenominator) < chanceNumerator) {
				int stack = info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1);
				TryDropInternal(info, itemId, stack);

				result.State = ItemDropAttemptResultState.Success;
				return result;
			}

			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}

		private void TryDropInternal(DropAttemptInfo info, int itemId, int stack) {
			if (itemId <= 0 || itemId >= ItemLoader.ItemCount)
				return;

			if (Main.netMode == NetmodeID.Server) {
				NPC npc = info.npc;

				int idx = Item.NewItem(npc.GetSource_Loot(), npc.Center, itemId, stack, noBroadcast: true, -1);

				Main.timeItemSlotCannotBeReusedFor[idx] = protectionFrames;

				for (int i = 0; i < 255; i++) {
					if (Main.player[i].active) {
						NetMessage.SendData(MessageID.InstancedItem, i, -1, null, idx);
					}
				}

				Main.item[idx].active = false;
			} else {
				CommonCode.DropItem(info, itemId, stack);
			}
		}
	}
}
