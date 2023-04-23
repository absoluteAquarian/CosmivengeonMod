using Microsoft.Xna.Framework;
using Terraria;

namespace CosmivengeonMod.Utility {
	public static partial class Extensions {
		/// <summary>
		/// Equivalent to <paramref name="vector"/>.RotatedBy(<paramref name="rotateBy"/>).RotatedByRandom(<paramref name="rotateByRandom"/>)
		/// </summary>
		/// <param name="vector">The original vector</param>
		/// <param name="rotateBy">The absolute rotation in radians</param>
		/// <param name="rotateByRandom">The relative random rotation in radians/></param>
		/// <returns></returns>
		public static Vector2 Rotate(this Vector2 vector, double rotateBy, double rotateByRandom)
			=> vector.RotatedBy(rotateBy).RotatedByRandom(rotateByRandom);

		/// <summary>
		/// Calls the Vector2.Rotate() extension, but it converts the parameters to radians
		/// </summary>
		/// <param name="vector">The original rotation</param>
		/// <param name="rotateByDegrees">The absolute rotation in degrees</param>
		/// <param name="rotateByRandomDegrees">The relative random rotation in degrees</param>
		/// <returns></returns>
		public static Vector2 RotateDegrees(this Vector2 vector, float rotateByDegrees, float rotateByRandomDegrees)
			=> vector.Rotate(MathHelper.ToRadians(rotateByDegrees), MathHelper.ToRadians(rotateByRandomDegrees));

		public static Vector3 ScreenCoord(this Vector2 vector) {
			//"vector" is a point on the screen... given the zoom is 1x
			//Let's correct that
			Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);
			Vector2 diff = vector - screenCenter;
			diff *= Main.GameZoomTarget;
			vector = screenCenter + diff;

			return new Vector3(-1 + vector.X / Main.screenWidth * 2, (-1 + vector.Y / Main.screenHeight * 2f) * -1, 0);
		}
	}
}
