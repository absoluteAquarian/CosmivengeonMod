using CosmivengeonMod.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace CosmivengeonMod.Systems {
	internal class UILoader : ModSystem {
		private StaminaUI staminaUI;
		private UserInterface userInterface;

		public override void Load() {
			//Only run this segment if we're not loading on a server
			if (!Main.dedServ && Main.netMode != NetmodeID.Server) {
				staminaUI = new StaminaUI();
				staminaUI.Activate();

				userInterface = new UserInterface();
				userInterface.SetState(staminaUI);
			}
		}

		public override void Unload() {
			staminaUI = null;
			userInterface = null;
		}

		public override void UpdateUI(GameTime gameTime) {
			bool visible = !Main.gameMenu && WorldEvents.desoMode;

			if (!visible && userInterface.CurrentState is not null)
				userInterface.SetState(null);
			else if (visible && userInterface.CurrentState is null)
				userInterface.SetState(staminaUI);

			userInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			//Copied from ExampleMod :thinkies:
			int mouseIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseIndex != -1) {
				layers.Insert(mouseIndex, new LegacyGameInterfaceLayer(
					"CosmivengeonMod: Stamina UI",
					delegate {
						userInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
