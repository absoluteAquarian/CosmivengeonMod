﻿using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CosmivengeonMod.Abilities {
	public class EnergizedParticle {
		public Player Parent { get; internal set; }
		public Vector2 Velocity = Vector2.Zero;
		private Vector2 offset = Vector2.Zero;
		public Vector2 Position => Parent.position + offset;
		public float Scale = 1f;
		public bool Active = false;

		private bool deleted = false;

		public EnergizedParticle(Player parent, Vector2 initialOffset, Vector2 velocity) {
			Parent = parent;
			offset = initialOffset;
			Velocity = velocity;
			Active = true;
		}

		public DrawData GetDrawData() {
			var asset = ModContent.Request<Texture2D>("CosmivengeonMod/Abilities/EnergizedParticle");

			var stamina = Parent.GetModPlayer<StaminaPlayer>().stamina;

			int frame = 1;
			if (stamina.Value > stamina.MaxValue * Stamina.FlashThreshold)
				frame = 0;
			else if (stamina.Value > stamina.MaxValue * Stamina.FlashThreshold / 2)
				frame = 2;

			Rectangle src = asset.Value.Frame(1, 3, 0, frame, 0, -2);

			return new DrawData(
				asset.Value,
				Position - Main.screenPosition,
				src,
				Color.White,
				0f,
				src.Size() / 2f,
				Scale,
				SpriteEffects.None,
				0
			);
		}

		public void Update() {
			if (!Active) {
				if (!deleted)
					Delete();
				return;
			}

			offset += Velocity;
			Scale *= 0.973f;

			if (Scale < 0.25f)
				Active = false;
		}

		public void Delete() {
			deleted = true;
			Active = false;
			Parent = null;
			offset = Vector2.Zero;
			Velocity = Vector2.Zero;
			Scale = 0f;
		}
	}
}
