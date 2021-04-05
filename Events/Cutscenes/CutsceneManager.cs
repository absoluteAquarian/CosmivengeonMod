namespace CosmivengeonMod.Events.Cutscenes{
	public static class CutsceneManager{
		public static Cutscene CurrentCutscene{ get; private set; }

		public static void AssignNewCutscene(Cutscene scene){
			CurrentCutscene = scene;
			CurrentCutscene.Active = false;
		}

		public static void BeginCutscene() => CurrentCutscene.Active = true;

		public static void Update(){
			if(CurrentCutscene?.Active ?? false)
				CurrentCutscene.Update();
		}
	}
}
