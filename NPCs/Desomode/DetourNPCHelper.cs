using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class DetourNPCHelper : GlobalNPC{
		public override bool InstancePerEntity => true;

		//None of these fields are descriptive.  Too bad!
		public int Timer;
		public int Timer2;
		public int Timer3;
		public int Timer4;

		public bool Flag;
		public bool Flag2;
		public bool Flag3;

		//Boss-specific fields
		public float EoC_PhaseTransitionVelocityLength;
		public int EoC_TargetHeight;
		public bool EoC_UsingShader;
		public int EoC_PlayerTargetMovementDirection;
		public int[] EoC_PlayerTargetMovementTimers = new int[2];
		public Vector2 EoC_FadePositionOffset;
		public int EoC_TimerTarget;
		public static int EoC_FirstBloodWallNPC = -1;
	}
}
