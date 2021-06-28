using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Utility.Extensions{
	public static partial class Extensions{
		/// <summary>
		/// Mirrors the angle (in radians) over the X-axis and/or Y-axis.
		/// </summary>
		/// <param name="angle">The original angle in radians.</param>
		/// <param name="mirrorX">Whether or not this angle should be mirrored over the X-axis.</param>
		/// <param name="mirrorY">Whether or not this angle should be mirrored over the Y-axis.</param>
		public static void MirrorAngle(this ref float angle, bool mirrorX = false, bool mirrorY = false){
			//Make the angle positive
			while(angle < 0)
				angle += MathHelper.TwoPi;

			//... and within [0, 2pi)
			angle %= MathHelper.TwoPi;
			
			//If neither option was chosen, don't do anything
			if(!mirrorX && !mirrorY)
				return;

			//If they were both chosen, just add Pi to the angle
			if(mirrorX && mirrorY)
				angle += MathHelper.Pi;
			//Otherwise, handle one case or the other
			else if(mirrorY)
				angle = (angle >= MathHelper.Pi ? 3 : 1) * MathHelper.Pi - angle;
			else if(mirrorX)
				angle = MathHelper.TwoPi - angle;

			//Make the angle be within [0, 2pi) again
			angle %= MathHelper.TwoPi;
		}
	}
}
