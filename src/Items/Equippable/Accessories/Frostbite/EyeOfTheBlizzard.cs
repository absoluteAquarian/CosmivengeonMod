using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Frostbite {
	public class EyeOfTheBlizzard : HidableTooltip {
		protected override bool CloneNewInstances => true;

		public override string ItemName => "Eye of the Blizzard";

		public override string FlavourText => "Summons an ice crystal that hovers above the player's head" +
				"\nThe crystal occasionally shoots icicles at nearby enemies" +
				"\nThe crystal regenerates <5N> health every 20 seconds" +
				"\nDouble-tapping the <N> key grants an immediate <10N> health regenerated," +
				"\na 5% increase in attack speed and an increased crystal shoot speed for 5 seconds" +
				"\nThe crystal must recharge for 60 seconds until the player can use the ability again";

		public override void SafeSetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(ticksperframe: 9, frameCount: 6));
		}

		public override void SetDefaults() {
			Item.accessory = true;
			Item.width = 30;
			Item.height = 46;
			Item.scale = 0.6667f;
			Item.value = Item.sellPrice(silver: 25, copper: 60);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		private int abilityTimer = -1;
		private Projectile Crystal = null;

		public override void UpdateInventory(Player player) {
			if (player.dead || !player.active) {
				abilityTimer = -1;
				Crystal = null;
				return;
			}

			if (player.GetModPlayer<AccessoriesPlayer>().blizzardEye)
				return;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile projectile = Main.projectile[i];

				if (projectile.active && projectile.ModProjectile is EyeOfTheBlizzardCrystal && projectile.owner == player.whoAmI)
					projectile.Kill();
			}
		}

		private void DespawnCrystal() {
			abilityTimer = -1;

			Crystal?.Kill();
			Crystal = null;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			AccessoriesPlayer mp = player.GetModPlayer<AccessoriesPlayer>();
			mp.blizzardEye = true;
			mp.blizzardEyeAccessory = Item;

			if (player.dead || !player.active) {
				DespawnCrystal();
				return;
			}

			Crystal = null;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile projectile = Main.projectile[i];

				if (projectile.active && projectile.ModProjectile is EyeOfTheBlizzardCrystal && projectile.owner == player.whoAmI) {
					Crystal = projectile;

					if (abilityTimer == 5 * 60)
						(Crystal.ModProjectile as EyeOfTheBlizzardCrystal).timer = 0;

					Crystal.ai[0] = abilityTimer;
					break;
				}
			}

			if (player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>()))
				return;

			player.AddBuff(ModContent.BuffType<EyeOfTheBlizzardBuff>(), 2);

			if (--abilityTimer > 0) {
				mp.activeBlizzardEye = true;
			} else if (abilityTimer == 0 && !player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>())) {
				mp.activeBlizzardEye = false;
				player.AddBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>(), 60 * 60);
				abilityTimer = -1;
			}

			bool doubleTap = player.controlUp && player.releaseUp && player.doubleTapCardinalTimer[SnakeShieldPlayer.DashUp] < 14;
			if (Crystal != null && abilityTimer < 0 && !((player.mount?.Active ?? false) && player.mount.CanFly()) && !player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>()) && doubleTap) {
				abilityTimer = 5 * 60;

				SoundEngine.PlaySound(SoundID.Item27, Crystal.Center);
				for (int i = 0; i < 30; i++) {
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, DustID.GreenFairy, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, DustID.TerraBlade, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
				}
			}
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips) {
			int customIndex = FindCustomTooltipIndex(tooltips);

			if (customIndex < 0 || tooltips.Any(t => t.Name == "SocialDesc"))
				return;

			do {
				TooltipLine customLine = tooltips[customIndex];

				if (customLine.Text.Contains("<5N>")) {
					List<string> text = customLine.Text.Split(new string[] { "<5N>" }, StringSplitOptions.None).ToList();
					text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.05f)}");
					customLine.Text = string.Join("", text.ToArray());
				} else if (customLine.Text.Contains("<10N>")) {
					List<string> text2 = customLine.Text.Split(new string[] { "<10N>" }, StringSplitOptions.None).ToList();
					text2.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.1f)}");
					customLine.Text = string.Join("", text2.ToArray());
				} else if (customLine.Text.Contains("<N>")) {
					List<string> text3 = customLine.Text.Split(new string[] { "<N>" }, StringSplitOptions.None).ToList();
					text3.Insert(1, Language.GetTextValue("Key.UP"));
					customLine.Text = string.Join("", text3.ToArray());
				}

				customIndex++;
			} while (tooltips[customIndex].Name.StartsWith("CustomTooltip"));
		}
	}
}
