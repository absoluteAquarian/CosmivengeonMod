using CosmivengeonMod.API.Edits.Detours.Desomode;

namespace CosmivengeonMod.API.Edits {
	public static class EditsLoader {
		public static bool LogILEdits = false;

		public static void Load() {
			ILHelper.InitMonoModDumps();

			DetourNPC.Load();

			IL.Terraria.Player.JumpMovement += MSIL.Vanilla.Player_JumpMovement;
			IL.Terraria.Player.Update += MSIL.Vanilla.Player_Update;
			IL.Terraria.Player.PlayerFrame += MSIL.Vanilla.Player_PlayerFrame;
			IL.Terraria.Player.CarpetMovement += MSIL.Vanilla.Player_CarpetMovement;
			IL.Terraria.Player.GrappleMovement += MSIL.Vanilla.Player_GrappleMovement;
			//DoubleJumpVisuals is small enough that I can just detour it and add the custom jump's effects
			On.Terraria.Player.DoubleJumpVisuals += Detours.Vanilla.Player_DoubleJumpVisuals;

			On.Terraria.NPC.SetDefaults += DetourNPC.NPC_SetDefaults;
			On.Terraria.NPC.VanillaAI += DetourNPC.NPC_VanillaAI;
			On.Terraria.NPC.GetBossHeadTextureIndex += DetourNPC.NPC_GetBossHeadTextureIndex;

			On.Terraria.Player.StatusPlayer += DetourPlayer.Player_StatusPlayer;

			On.Terraria.Projectile.StatusPlayer += DetourProjectile.HookStatusPlayer;

			MSIL.CheatSheet.Load();

			ILHelper.DeInitMonoModDumps();
		}

		public static void Unload() {
			DetourNPC.Unload();

			MSIL.CheatSheet.Unload();
		}
	}
}
