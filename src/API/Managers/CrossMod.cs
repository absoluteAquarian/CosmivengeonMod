using CosmivengeonMod.Items.Equippable.Pets;
using CosmivengeonMod.Items.Equippable.Vanity.BossMasks;
using CosmivengeonMod.Items.Lore;
using CosmivengeonMod.Items.Spawning.Boss;
using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Managers {
	public static class CrossMod {
		public static void Load() {
			//Set the boss's position in BossChecklist if the mod is active
			//see:  https://github.com/JavidPack/BossChecklist/wiki/%5B1.4%5D-Boss-Log-Entry-Mod-Call
			if (ModReferences.BossChecklist.Active) {
				ModReferences.BossChecklist.Instance.Call("AddBoss",
					CoreMod.Instance,
					"$Mods.CosmivengeonMod.NPCName.Draek",
					new List<int>(){
						ModContent.NPCType<Draek>(),
						ModContent.NPCType<DraekP2Head>()
					},
					2.7f,  // 2.7f = just before Eater of Worlds
					() => WorldEvents.downedDraekBoss,
					() => true,
					new List<int>(){
						ModContent.ItemType<DraekMask>(),
						ModContent.ItemType<StoneTablet>(),
						ModContent.ItemType<TerraBolt>()
					},
					ModContent.ItemType<DraekSummon>(),
					"$Mods.CosmivengeonMod.CrossMod.BossChecklist.BossLogSpawnInfo.Draek",
					null,  // Ignore despawn message argument
					(Action<SpriteBatch, Rectangle, Color>)RenderDraekBossLog);

				ModReferences.BossChecklist.Instance.Call("AddBoss",
					CoreMod.Instance,
					"$Mods.CosmivengeonMod.NPCName.Frostbite",
					ModContent.NPCType<Frostbite>(),
					1.5f,  // 1.5f = between Slime King and Eye of Cthulhu
					() => WorldEvents.downedFrostbiteBoss,
					() => true,
					new List<int>(){
						ModContent.ItemType<BabyCloudBottle>(),
						ModContent.ItemType<FrostbiteMask>(),
						ModContent.ItemType<IceforgedRelic>()
					},
					ModContent.ItemType<DraekSummon>(),
					"$Mods.CosmivengeonMod.CrossMod.BossChecklist.BossLogSpawnInfo.Frostbite",
					null,  // Ignore despawn message argument
					(Action<SpriteBatch, Rectangle, Color>)RenderFrostbiteBossLog);
			}
		}

		private static void RenderGenericCenteredBossLog(SpriteBatch spriteBatch, string asset, Rectangle area, Color blind) {
			Texture2D texture = ModContent.Request<Texture2D>(asset).Value;
			Vector2 centered = new Vector2(area.X + area.Width / 2f - texture.Width / 2f, area.Y + area.Height / 2f - texture.Height / 2f);
			spriteBatch.Draw(texture, centered, blind);
		}

		private static void RenderDraekBossLog(SpriteBatch spriteBatch, Rectangle area, Color blind) {
			RenderGenericCenteredBossLog(spriteBatch, "CosmivengeonMod/Assets/CrossMod/BossChecklist/Draek_BossLog", area, blind);
		}

		private static void RenderFrostbiteBossLog(SpriteBatch spriteBatch, Rectangle area, Color blind) {
			RenderGenericCenteredBossLog(spriteBatch, "CosmivengeonMod/Assets/CrossMod/BossChecklist/Frostbite_BossLog", area, blind);
		}
	}
}
