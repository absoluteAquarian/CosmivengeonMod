using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

/*			THIS IS A BROKEN CLASS
 *		I couldn't get it to work properly, but I'll
 *		keep it in this mod just in case I can make it
 *		work later.  Oh well.
 */

namespace CosmivengeonMod{
	public class PID{
		private Vector2 pidError;
		private Vector2 pidPrevError;
		private float kP = 0.8f;
		private float kI = 0.2f;
		private float kD = 0.6f;
		private Vector2 pid_I = Vector2.Zero;

		public static readonly PID Uninitialized = new PID();

		public PID(){}

		public PID(NPC npc, Vector2 target){
			pidError = Vector2.Zero;
			pidPrevError = target;
		}

		/// <summary>
		/// Use PID control to reduce wobbling around a specified target position.
		/// </summary>
		/// <param name="npc">The NPC to which the PID control is affecting.</param>
		/// <param name="target">The target position.</param>
		public void Control(NPC npc, Vector2 target){
			if(this == Uninitialized){
				pidError = Vector2.Zero;
				pidPrevError = target;
			}

			//Don't use PID if the npc is more than 15 blocks away from the target position
			if(Vector2.Distance(target, npc.position) > 15 * 16)
				return;
			
			pidError = target - npc.position;
			
			//Proportional PID
			Vector2 pid_P = pidError * kP;

			//Integral PID
			pid_I += pidError * kI;

			//Derivative PID
			Vector2 pid_D = pidError - pidPrevError;
			pid_D *= kD;

			pidPrevError = pidError;

			//Add the PID
			npc.velocity += pid_P + pid_I + pid_D;
		}

		public static void PlaceholderControl(NPC npc, Vector2 target, float friction){
			//Preemptively slow down the NPC if it overshoots its target
			if(OvershootsX(npc, target))
				npc.velocity.X *= 0.6f;
			if(OvershootsY(npc, target))
				npc.velocity.Y *= 0.6f;
			
			//Slow down the NPC anyway
			npc.velocity *= friction;
		}

#region Overshoots
		public static bool Overshoots(NPC npc, Vector2 target){
			return Overshoots(npc.position, npc.velocity, target);
		}

		public static bool Overshoots(Vector2 position, Vector2 velocity, Vector2 target){
			return OvershootsX(position, velocity, target) || OvershootsY(position, velocity, target);
		}

		public static bool OvershootsX(NPC npc, Vector2 target){
			return OvershootsX(npc.position, npc.velocity, target);
		}

		public static bool OvershootsX(Vector2 position, Vector2 velocity, Vector2 target){
			float nextX = position.X + velocity.X;
			return (nextX > target.X && position.X < target.X)
				|| (nextX < target.X && position.X > target.X);
		}

		public static bool OvershootsY(NPC npc, Vector2 target){
			return OvershootsY(npc.position, npc.velocity, target);
		}

		public static bool OvershootsY(Vector2 position, Vector2 velocity, Vector2 target){
			float nextY = position.Y + velocity.Y;
			return (nextY > target.Y && position.Y < target.Y)
				|| (nextY < target.Y && position.Y > target.Y);
		}
#endregion
	}
}
