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
	public class DataHand : HandModelBase {
    private Hand hand_;

	[SerializeField] private bool visualizeBones = true;
	[SerializeField] private bool visualizeBasis = true;

	/** The colors used for each bone. */
	protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };

	public override ModelType HandModelType
	{
		get
		{
			return ModelType.Physics;
		}
	}

	[SerializeField]
	private Chirality handedness;
	public override Chirality Handedness
	{
		get
		{
			return handedness;
		}
		set { }
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
		DrawDebugLines();
	}

	/**
    * Updates the hand and calls the line drawing function.
    */
	public override void UpdateHand()
	{
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