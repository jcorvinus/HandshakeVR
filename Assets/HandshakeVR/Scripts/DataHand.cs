using UnityEngine;
using System.Collections;
using Leap;

using Leap.Unity;

namespace HandshakeVR
{
	/// <summary>
	/// Slight modification of the DebugHand class - 
	/// this one gets set as a physics hand so that
	/// it can still be referenced for gestures
	/// while graphics hands are disabled. 
	/// 
	/// Especially useful for using detectors
	/// when using the SteamVR hand model
	/// </summary>
	public class DataHand : HandModel
	{

		[SerializeField] private bool visualizeBones = true;
		[SerializeField] private bool visualizeBasis = true;
		[SerializeField] private bool visualizeDirections = false;
		[SerializeField] private bool visualizeVelocity = false;

		/** The colors used for each bone. */
		protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };

		PinchDetector pinchDetector;
		const float pinchMax = 0.09f; // closed is like 0.007
		float normalizedPinchValue = 0; // normalized pinch-to-detector fire distance
		float normalizedPinchActivationValue = 0; // normalized activation-to-zero distance

		/// <summary>
		/// How close to activation the pinch is. Useful for glow indicators
		/// </summary>
		public float NormalizedPinchValue { get { return normalizedPinchValue; } }

		/// <summary>
		/// how 'deeply pressed' the current activation is. Useful for 'squeeze' activators
		/// </summary>
		public float NormalizedPinchActivation { get { return normalizedPinchActivationValue; } }
		public PinchDetector PinchDetector
		{
			get
			{
				if (!pinchDetector) pinchDetector = GetComponent<PinchDetector>();
				return pinchDetector;
			}
		}

		private void Start()
		{
			pinchDetector = GetComponent<PinchDetector>();
		}

		public override ModelType HandModelType
		{
			get
			{
				return ModelType.Physics;
			}
		}

		public override Hand GetLeapHand()
		{
			return hand_;
		}

		public override void SetLeapHand(Hand hand)
		{
			hand_ = hand;
		}

		public override bool SupportsEditorPersistence()
		{
			return true;
		}

		/**
		* Initializes the hand and calls the line drawing function.
		*/
		public override void InitHand()
		{
			base.InitHand();
			DrawDebugLines();
		}

		/**
		* Updates the hand and calls the line drawing function.
		*/
		public override void UpdateHand()
		{
			if (pinchDetector)
			{
				if (IsTracked)
				{
					normalizedPinchValue = 1 - Mathf.InverseLerp(pinchDetector.ActivateDistance, pinchMax, pinchDetector.Distance);
					normalizedPinchActivationValue = Mathf.InverseLerp(0, pinchDetector.ActivateDistance, pinchDetector.Distance);
				}
				else
				{
					normalizedPinchValue = 0;
					normalizedPinchActivationValue = 0;
				}
			}

			DrawDebugLines();
		}

		/**
		* Draws lines from elbow to wrist, wrist to palm, and normal to the palm.
		*/
		protected void DrawDebugLines()
		{
			Hand hand = GetLeapHand();
			if (visualizeBones)
			{
				Debug.DrawLine(hand.Arm.ElbowPosition.ToVector3(), hand.Arm.WristPosition.ToVector3(), Color.red); //Arm
				Debug.DrawLine(hand.WristPosition.ToVector3(), hand.PalmPosition.ToVector3(), Color.white); //Wrist to palm line
				Debug.DrawLine(hand.PalmPosition.ToVector3(), (hand.PalmPosition + hand.PalmNormal * hand.PalmWidth / 2).ToVector3(), Color.black); //Hand Normal
			}

			if (visualizeBasis)
			{
				DrawBasis(hand.PalmPosition, hand.Basis, hand.PalmWidth / 4); //Hand basis
				DrawBasis(hand.Arm.ElbowPosition, hand.Arm.Basis, .01f); //Arm basis
			}

			if (visualizeBones)
			{
				for (int f = 0; f < 5; f++)
				{ //Fingers
					Finger finger = hand.Fingers[f];
					for (int i = 0; i < 4; ++i)
					{
						Bone bone = finger.Bone((Bone.BoneType)i);
						Debug.DrawLine(bone.PrevJoint.ToVector3(), bone.PrevJoint.ToVector3() + bone.Direction.ToVector3() * bone.Length, colors[i]);
						if (visualizeBasis)
							DrawBasis(bone.PrevJoint, bone.Basis, .01f);
					}
				}
			}

			if(visualizeDirections)
			{
				// draw the hand direction
				Debug.DrawLine(hand.PalmPosition.ToVector3(),
					(hand.PalmPosition.ToVector3() + (hand.Direction.ToVector3() * 0.1f)));

				for (int f = 0; f < 5; f++)
				{ //Fingers
					Finger finger = hand.Fingers[f];
					bool isExtended = finger.IsExtended;
					Debug.DrawLine(finger.TipPosition.ToVector3(),
						finger.TipPosition.ToVector3() + (finger.Direction.ToVector3() * 0.1f),
						isExtended ? Color.green : Color.red);
				}
			}

			if(visualizeVelocity)
			{
				Debug.DrawLine(hand.PalmPosition.ToVector3(),
					(hand.PalmPosition.ToVector3() + hand.PalmVelocity.ToVector3()), Color.green);
			}
		}

		public void DrawBasis(Vector position, LeapTransform basis, float scale)
		{
			Vector3 origin = position.ToVector3();
			Debug.DrawLine(origin, origin + basis.xBasis.ToVector3() * scale, Color.red);
			Debug.DrawLine(origin, origin + basis.yBasis.ToVector3() * scale, Color.green);
			Debug.DrawLine(origin, origin + basis.zBasis.ToVector3() * scale, Color.blue);
		}

	}
}