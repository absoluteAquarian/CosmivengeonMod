﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.UI;

namespace CosmivengeonMod.DataStructures.Bestiary {
	internal class SnowPortraitBackgroundProviderBestiaryInfoElement : IBestiaryInfoElement, IBestiaryBackgroundImagePathAndColorProvider {
		public Color? GetBackgroundColor() => Color.White;

		public Asset<Texture2D> GetBackgroundImage() => Main.Assets.Request<Texture2D>("Images/MapBG12");

		public UIElement ProvideUIElement(BestiaryUICollectionInfo info) => null;
	}
}