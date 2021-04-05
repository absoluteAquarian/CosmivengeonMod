using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace CosmivengeonMod.Events.Cutscenes{
	public class Cutscene{
		/// <summary>
		/// A queue of origin points the camera will move from
		/// </summary>
		private readonly Queue<Vector2> movementOrigins;

		/// <summary>
		/// A queue for how long the movements will last for
		/// </summary>
		private readonly Queue<int> movementDurations;

		/// <summary>
		/// A queue of movement factors the camera will use to move from one target to the next
		/// </summary>
		private readonly Queue<Vector2> movementSpeeds;

		private readonly List<Action<int>> miscActions;

		private int currentTick;

		private Vector2 currentMovementLocation;
		private int currentMovementDuration;
		private Vector2 currentMovementSpeed;

		public bool Active{ get; internal set; }

		public Cutscene(Vector2 initialPosition){
			movementOrigins = new Queue<Vector2>();
			movementSpeeds = new Queue<Vector2>();
			miscActions = new List<Action<int>>();

			currentMovementDuration = -1;

			movementOrigins.Enqueue(initialPosition);
		}

		/// <summary>
		/// Registers a new camera target for the cutscene
		/// </summary>
		/// <param name="origin">The position to start from</param>
		/// <param name="duration">How long the camera movement will last for</param>
		/// <param name="speed">How quickly the camera will move</param>
		public void RegisterTarget(Vector2 origin, int duration, Vector2 speed){
			movementOrigins.Enqueue(origin);
			movementDurations.Enqueue(duration);
			movementSpeeds.Enqueue(speed);
		}

		/// <summary>
		/// Registers an action that can occur during the cutscene
		/// </summary>
		/// <param name="action">The action.  The <seealso cref="int"/> parameter is the current elapsed ticks for the cutscene</param>
		public void RegisterAction(Action<int> action){
			miscActions.Add(action);
		}

		public void Update(){
			for(int i = 0; i < miscActions.Count; i++)
				miscActions[i].Invoke(currentTick);

			if(currentMovementDuration <= 0){
				currentMovementLocation = movementOrigins.Dequeue();
				currentMovementDuration = movementDurations.Dequeue();
				currentMovementSpeed = movementSpeeds.Dequeue();
			}else{
				currentMovementLocation += currentMovementSpeed;

				currentMovementDuration--;
			}

			currentTick++;
		}

		public void ModifyScreenPosition(ref Vector2 position) => position = currentMovementLocation;
	}
}
