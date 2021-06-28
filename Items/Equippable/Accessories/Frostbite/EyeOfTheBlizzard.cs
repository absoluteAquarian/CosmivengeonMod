using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Frostbite{
	public class EyeOfTheBlizzard : HidableTooltip{
		public override bool CloneNewInstances => true;

		public override string ItemName => "Eye of the Blizzard";

		public override string FlavourText => "Summons an ice crystal that hovers above the player's head" +
				"\nThe crystal occasionally shoots icicles at nearby enemies" +
				"\nThe crystal regenerates <5N> health every 20 seconds" +
				"\nDouble-tapping the <N> key grants an immediate <10N> health regenerated," +
				"\na 5% increase in attack speed and an increased crystal shoot speed for 5 seconds" +
				"\nThe crystal must recharge for 60 seconds until the player can use the ability again";

		public override void SafeSetStaticDefaults(){
			Main.RegisterItemAnimation(item.type, new EyeOfTheBlizzardAnimation());
		}

		public override void SetDefaults(){
			item.accessory = true;
			item.width = 30;
			item.height = 46;
			item.scale = 0.6667f;
			item.value = Item.sellPrice(silver: 25, copper: 60);
			item.rare = ItemRarityID.Expert;
			item.expert = true;
		}

		private int abilityTimer = -1;
		private Projectile Crystal = null;

		public override void UpdateInventory(Player player){
			if(player.dead || !player.active){
				abilityTimer = -1;
				Crystal = null;
				return;
			}

			if(Equipped(player))
				return;

			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile projectile = Main.projectile[i];

				if(projectile.active && projectile.modProjectile is EyeOfTheBlizzardCrystal && projectile.owner == player.whoAmI)
					projectile.Kill();
			}

			player.GetModPlayer<AccessoriesPlayer>().blizzardEye = false;
		}

		private bool Equipped(Player player){
			for(int i = 3; i < 8 + player.extraAccessorySlots; i++){
				if(player.armor[i].type == item.type)
					return true;
			}
			return false;
		}

		private void DespawnCrystal(){
			abilityTimer = -1;

			Crystal?.Kill();
			Crystal = null;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			if(player.dead || !player.active){
				DespawnCrystal();
				return;
			}

			Crystal = null;
			for(int i = 0; i < Main.maxProjectiles; i++){
				Projectile projectile = Main.projectile[i];

				if(projectile.active && projectile.modProjectile is EyeOfTheBlizzardCrystal && projectile.owner == player.whoAmI){
					Crystal = projectile;

					if(abilityTimer == 5 * 60)
						(Crystal.modProjectile as EyeOfTheBlizzardCrystal).timer = 0;

					Crystal.ai[0] = abilityTimer;
					break;
				}
			}

			if(player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>()))
				return;

			player.AddBuff(ModContent.BuffType<EyeOfTheBlizzardBuff>(), 2);

			AccessoriesPlayer mp = player.GetModPlayer<AccessoriesPlayer>();

			if(--abilityTimer > 0){
				mp.activeBlizzardEye = true;
			}else if(abilityTimer == 0 && !player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>())){
				mp.activeBlizzardEye = false;
				player.AddBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>(), 60 * 60);
				abilityTimer = -1;
			}

			bool doubleTap = player.controlUp && player.releaseUp && player.doubleTapCardinalTimer[SnakeShieldPlayer.DashUp] < 14;
			if(Crystal != null && abilityTimer < 0 && !((player.mount?.Active ?? false) && player.mount.CanFly) && !player.HasBuff(ModContent.BuffType<EyeOfTheBlizzardCooldown>()) && doubleTap){
				abilityTimer = 5 * 60;

				Main.PlaySound(SoundID.Item27, Crystal.Center);
				for(int i = 0; i < 30; i++){
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, 74, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, 107, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
				}
			}
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips){
			int customIndex = FindCustomTooltipIndex(tooltips);

			if(customIndex < 0 || tooltips.Any(t => t.Name == "SocialDesc"))
				return;

			do{
				TooltipLine customLine = tooltips[customIndex];

				if(customLine.text.Contains("<5N>")){
					List<string> text = customLine.text.Split(new string[]{ "<5N>" }, StringSplitOptions.None).ToList();
					text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.05f)}");
					customLine.text = string.Join("", text.ToArray());
				}else if(customLine.text.Contains("<10N>")){
					List<string> text2 = customLine.text.Split(new string[]{ "<10N>" }, StringSplitOptions.None).ToList();
					text2.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.1f)}");
					customLine.text = string.Join("", text2.ToArray());
				}else if(customLine.text.Contains("<N>")){
					List<string> text3 = customLine.text.Split(new string[]{ "<N>" }, StringSplitOptions.None).ToList();
					text3.Insert(1, Language.GetTextValue("Key.UP"));
					customLine.text = string.Join("", text3.ToArray());
				}

				customIndex++;
			}while(tooltips[customIndex].Name.StartsWith("CustomTooltip"));
		}
	}

	public class EyeOfTheBlizzardAnimation : DrawAnimation{
		public EyeOfTheBlizzardAnimation(){
			FrameCount = 6;
			TicksPerFrame = 9;
		}

		public override Rectangle GetFrame(Texture2D texture)
			=> texture.Frame(1, FrameCount, 0, Frame);

		public override void Update(){
			if(++FrameCounter % TicksPerFrame == 0)
				Frame = ++Frame % FrameCount;
		}
	}
}
