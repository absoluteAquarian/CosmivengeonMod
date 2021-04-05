using Terraria.ModLoader;

namespace CosmivengeonMod.Events.Cutscenes{
	public class CutscenePlayer : ModPlayer{
		public override void SetControls(){
			if(CutsceneManager.CurrentCutscene?.Active ?? false){
				//Players shouldn't move during cutscenes
				player.controlLeft = false;
				player.controlRight = false;
			}
		}
	}
}
