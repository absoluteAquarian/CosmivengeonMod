using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CosmivengeonMod.Abilities{
	public class EnergizedParticle{
		public Player Parent{ get; internal set; }
		public Vector2 Velocity = Vector2.Zero;
		private Vector2 offset = Vector2.Zero;
		public Vector2 Position => Parent.position + offset;
		public float Scale = 1f;
		public static Texture2D DrawTexture => ModContent.GetTexture("CosmivengeonMod/Abilities/EnergizedParticle");
		public bool Active = false;

		private bool deleted = false;

		public EnergizedParticle(Player parent, Vector2 initialOffset, Vector2 velocity){
			Parent = parent;
			offset = initialOffset;
			Velocity = velocity;
			Active = true;
		}

		public DrawData GetDrawData()
			=> new DrawData(
				DrawTexture,
				Position - Main.screenPosition,
				new Rectangle(0, 0, DrawTexture.Width, DrawTexture.Height),
				Lighting.GetColor((int)(Position.X / 16f), (int)(Position.Y / 16f)),
				0f,
				new Vector2(DrawTexture.Width / 2f, DrawTexture.Height / 2f),
				Scale,
				SpriteEffects.None,
				0
			);

		public void Update(){
			if(!Active){
				if(!deleted)
					Delete();
				return;
			}

			offset += Velocity;
			Scale *= 0.973f;

			if(Scale < 0.25f)
				Active = false;
		}

		public void Delete(){
			deleted = true;
			Active = false;
			Parent = null;
			offset = Vector2.Zero;
			Velocity = Vector2.Zero;
			Scale = 0f;
		}
	}
}
