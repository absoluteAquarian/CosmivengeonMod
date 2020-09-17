using CosmivengeonMod.Projectiles.Summons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class EyeOfTheBlizzard : ModItem{
		public override bool CloneNewInstances => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Eye of the Blizzard");
			Tooltip.SetDefault("Summons an ice crystal that hovers above the player's head" +
				"\nThe crystal occasionally shoots icicles at nearby enemies" +
				"\nThe crystal regenerates <5N> health every 20 seconds" +
				"\nDouble-tapping the <N> key grants an immediate <10N> health regenerated," +
				"\na 5% increase in attack speed and an increased crystal shoot speed for 5 seconds" +
				"\nThe crystal must recharge for 60 seconds until the player can use the ability again");

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

		private int passiveHealTimer = 0;
		private int abilityTimer = -1;
		private Projectile Crystal = null;

		public override void UpdateInventory(Player player){
			if(player.dead || !player.active){
				passiveHealTimer = 0;
				abilityTimer = -1;
				Crystal = null;
				return;
			}

			if(Equipped(player))
				return;

			List<Projectile> crystals = Main.projectile.Where(p => p?.active == true && p.modProjectile is EyeOfTheBlizzardCrystal && p.owner == player.whoAmI).ToList();

			for(int i = 0; i < crystals.Count; i++)
				crystals[i].Kill();

			player.GetModPlayer<CosmivengeonPlayer>().equipped_EyeOfTheBlizzard = false;
		}

		private bool Equipped(Player player){
			for(int i = 3; i < 8 + player.extraAccessorySlots; i++){
				if(player.armor[i].type == item.type)
					return true;
			}
			return false;
		}

		private void DespawnCrystal(){
			passiveHealTimer = 0;
			abilityTimer = -1;

			Crystal?.Kill();
			Crystal = null;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			if(player.dead || !player.active){
				DespawnCrystal();
				return;
			}

			List<Projectile> crystals = Main.projectile.Where(p => p?.active == true && p.modProjectile is EyeOfTheBlizzardCrystal && p.owner == player.whoAmI).ToList();
			if(crystals.Any()){
				Crystal = crystals.First();

				if(abilityTimer == 5 * 60)
					(Crystal.modProjectile as EyeOfTheBlizzardCrystal).timer = 0;

				Crystal.ai[0] = abilityTimer;
			}else
				Crystal = null;
			
			if(++passiveHealTimer % (20 * 60) == 0){
				int amount = (int)(player.statLifeMax2 * 0.05f);
				player.statLife += amount;
				player.HealEffect(amount);
			}

			if(player.HasBuff(ModContent.BuffType<Buffs.EyeOfTheBlizzard_Cooldown>()))
				return;

			player.AddBuff(ModContent.BuffType<Buffs.EyeOfTheBlizzardBuff>(), 2);

			CosmivengeonPlayer mp = player.GetModPlayer<CosmivengeonPlayer>();

			if(--abilityTimer > 0){
				mp.abilityActive_EyeOfTheBlizzard = true;
			}else if(abilityTimer == 0 && !player.HasBuff(ModContent.BuffType<Buffs.EyeOfTheBlizzard_Cooldown>())){
				mp.abilityActive_EyeOfTheBlizzard = false;
				player.AddBuff(ModContent.BuffType<Buffs.EyeOfTheBlizzard_Cooldown>(), 60 * 60);
				abilityTimer = -1;
			}

			if(Crystal != null && abilityTimer < 0 && !((player.mount?.Active ?? false) && player.mount.CanFly) && !player.HasBuff(ModContent.BuffType<Buffs.EyeOfTheBlizzard_Cooldown>()) && mp.DoubleTapUp){
				int amount = (int)(player.statLifeMax2 * 0.1f);
				player.statLife += amount;
				player.HealEffect(amount);

				abilityTimer = 5 * 60;

				Main.PlaySound(SoundID.Item27, Crystal.Center);
				for(int i = 0; i < 30; i++){
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, 74, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
					Dust.NewDust(Crystal.Center - new Vector2(8, 8), 16, 16, 107, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-8, 8), newColor: Color.Blue);
				}
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			if(tooltips.Any(t => t.Name == "SocialDesc"))
				return;

			TooltipLine health5Line = tooltips.Find(t => t.text.Contains("<5N>"));
			TooltipLine health10Line = tooltips.Find(t => t.text.Contains("<10N>"));
			TooltipLine keyLine = tooltips.Find(t => t.text.Contains("<N>"));

			List<string> text = health5Line.text.Split(new string[]{ "<5N>" }, StringSplitOptions.None).ToList();
			text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.05f)}");
			health5Line.text = string.Join("", text.ToArray());

			List<string> text2 = health10Line.text.Split(new string[]{ "<10N>" }, StringSplitOptions.None).ToList();
			text2.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.1f)}");
			health10Line.text = string.Join("", text2.ToArray());

			List<string> text3 = keyLine.text.Split(new string[]{ "<N>" }, StringSplitOptions.None).ToList();
			text3.Insert(1, Language.GetTextValue("Key.UP"));
			keyLine.text = string.Join("", text3.ToArray());
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
