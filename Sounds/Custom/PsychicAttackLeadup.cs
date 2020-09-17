﻿using Microsoft.Xna.Framework.Audio;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Sounds.Custom{
	public class PsychicAttackLeadup : ModSound{
		//Uses the "teleport" sound effect from http://starmen.net/mother2/soundfx/

		public override SoundEffectInstance PlaySound(ref SoundEffectInstance soundInstance, float volume, float pan, SoundType type){
			//An optional variable to make controlling the volume easier
			float volumeFactor = 0.5f;
 
			if(soundInstance is null){
				//This is a new sound instance
				
				soundInstance = sound.CreateInstance();
				soundInstance.Volume = volume * volumeFactor;
				soundInstance.Pan = pan;
				Main.PlaySoundInstance(soundInstance);
				return soundInstance;
			}else if(soundInstance.State == SoundState.Stopped){
				//This is an existing sound instance that just stopped (OPTIONAL: use this if you want a looping sound effect!)
				
				soundInstance.Volume = volume * volumeFactor;
				soundInstance.Pan = pan;
				Main.PlaySoundInstance(soundInstance);
				return soundInstance;
			}
 
			//This is an existing sound instance that's still playing
			soundInstance.Volume = volume * volumeFactor;
			soundInstance.Pan = pan;
			return soundInstance;
		}
	}
}
