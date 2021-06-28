using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CosmivengeonMod.API.Managers{
	public static class PrimitiveDrawing{
		internal static BasicEffect simpleVertexEffect;
		internal static void Init(GraphicsDevice device){
			simpleVertexEffect = new BasicEffect(device){
				VertexColorEnabled = true
			};
		}

		public static void DrawLineStrip(Vector2[] points, Color color){
			//Nothing to draw, so don't
			if(points.Length < 2)
				return;

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(points[0], color), ToPrimitive(points[1], color));
			for(int i = 2; i < points.Length; i++)
				packet.AddDraw(ToPrimitive(points[i], color));

			SubmitPacket(packet);
		}

		public static void DrawHollowRectangle(Vector2 coordTL, Vector2 coordBR, Color color){
			Vector2 tr = new Vector2(coordBR.X, coordTL.Y);
			Vector2 bl = new Vector2(coordTL.X, coordBR.Y);

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			packet.AddDraw(ToPrimitive(coordTL, color), ToPrimitive(tr, color));
			packet.AddDraw(ToPrimitive(tr, color),      ToPrimitive(coordBR, color));
			packet.AddDraw(ToPrimitive(coordBR, color), ToPrimitive(bl, color));
			packet.AddDraw(ToPrimitive(bl, color),      ToPrimitive(coordTL, color));

			SubmitPacket(packet);
		}

		public static void DrawFilledRectangle(Vector2 coordTL, Vector2 coordBR, Color color){
			Vector2 tr = new Vector2(coordBR.X, coordTL.Y);
			Vector2 bl = new Vector2(coordTL.X, coordBR.Y);

			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);

			packet.AddDraw(ToPrimitive(coordTL, color), ToPrimitive(tr, color), ToPrimitive(bl, color));
			packet.AddDraw(ToPrimitive(bl, color), ToPrimitive(tr, color), ToPrimitive(coordBR, color));

			SubmitPacket(packet);
		}

		public static void DrawHollowCircle(Vector2 center, float radius, Color color){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.LineStrip);

			Vector2 rotate = Vector2.UnitX * radius;
			packet.AddDraw(ToPrimitive(rotate + center, color), ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, color));
			for(int i = 2; i < 360; i++)
				packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 306f * i), color));

			SubmitPacket(packet);
		}

		public static void DrawFilledCircle(Vector2 center, float radius, Color color, bool fadeEdge = false){
			PrimitivePacket packet = new PrimitivePacket(PrimitiveType.TriangleList);

			Color edgeColor = fadeEdge ? color * 0.13f : color;
			Vector2 rotate = Vector2.UnitX * radius;
			packet.AddDraw(ToPrimitive(rotate + center, edgeColor),
				ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f) + center, edgeColor),
				ToPrimitive(center, color));
			for(int i = 1; i < 360; i++){
				packet.AddDraw(ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360 * i) + center, edgeColor),
					ToPrimitive(rotate.RotatedBy(MathHelper.TwoPi / 360f * (i + 1)) + center, edgeColor),
					ToPrimitive(center, color));
			}

			SubmitPacket(packet);
		}

		public static void SubmitPacket(PrimitivePacket packet){
			VertexBuffer buffer = new VertexBuffer(Main.graphics.GraphicsDevice, typeof(VertexPositionColor), packet.draws.Count, BufferUsage.WriteOnly);

			//Calculate the number of primitives that will be drawn
			int count = packet.GetPrimitivesCount();

			//Device must not have a buffer attached for a buffer to be given data
			Main.graphics.GraphicsDevice.SetVertexBuffer(null);
			buffer.SetData(packet.draws.ToArray());

			//Set the buffer
			Main.graphics.GraphicsDevice.SetVertexBuffer(buffer);
			simpleVertexEffect.CurrentTechnique.Passes[0].Apply();

			//Draw the vertices
			Main.graphics.GraphicsDevice.DrawPrimitives(packet.type, 0, count);
		}

		/// <summary>
		/// Converts the riven world <paramref name="worldPos"/> coordinate, <paramref name="color"/> draw color and <paramref name="drawDepth"/> draw depth into a <seealso cref="VertexPositionColor"/> that can be used in primitives drawing
		/// </summary>
		/// <param name="worldPos">The absolute world position</param>
		/// <param name="color">The draw color</param>
		public static VertexPositionColor ToPrimitive(Vector2 worldPos, Color color){
			Vector3 pos = (worldPos - Main.screenPosition).ScreenCoord();
			if(Main.LocalPlayer.gravDir == -1)
				pos.Y = -pos.Y;

			return new VertexPositionColor(pos, color);
		}
	}
}
