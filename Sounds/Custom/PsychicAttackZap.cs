using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Sounds.Custom{
	public class PsychicAttackZap : ModSound{
		//Uses the "thunder1" sound effect from http://starmen.net/mother2/soundfx/

		public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type){
			//An optional variable to make controlling the volume easier
			float volumeFactor = 0.6f;

			soundInstance = sound.CreateInstance();
			soundInstance.Volume = volume * volumeFactor;
			soundInstance.Pan = pan;
			Main.PlaySoundInstance(soundInstance);
			return soundInstance;
		}
	}
}
