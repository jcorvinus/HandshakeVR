using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	/// <summary>
	/// Describes how the hands get their pose data.
	/// </summary>
	public enum HandTrackingType
	{
		// something is wrong, or nothing is available
		None,

		/// <summary>The SDK exposes skeletal tracking directly, so hands have full articulation.</summary>
		Skeletal,
		/// <summary>The user is using some kind of motion controller without hand tracking. We provide poses ourselves.</summary>
		Emulation
	}

	[System.Serializable]
	public struct ControllerOffset
	{
		[SerializeField] public Vector3 LocalPositionLeft;
		[SerializeField] public Vector3 LocalRotationLeft;

		[SerializeField] public Vector3 LocalPositionRight;
		[SerializeField] public Vector3 LocalRotationRight;
	}

	/// <summary>
	/// This is a base class meant to serve data to the SkeletalControllerHand.
	/// Common code from the classes 'OculusRemapper' and 'SteamVRRemapper' go here
	/// </summary>
	public abstract class HandInputProvider : MonoBehaviour
	{
		protected SkeletalControllerHand controllerHand;
		protected Animator handAnimator;

		/// <summary>
		/// If this is Skeletal, then PlatformControllerManager will allow skeletal-based grasping,
		/// otherwise it will use action based grasping.
		/// </summary>
		public abstract HandTrackingType TrackingType();

		protected virtual void Awake()
		{
			controllerHand = GetComponent<SkeletalControllerHand>();
			handAnimator = GetComponent<Animator>();
		}

		private void OnEnable()
		{
			controllerHand.ActiveProvider = this;
		}

		protected Quaternion GlobalRotationFromBasis(Transform bone, BoneBasis basis)
		{
			return Quaternion.LookRotation(bone.TransformDirection(basis.Forward),
				bone.TransformDirection(basis.Up));
		}

		private void DrawBones(Transform parent, BoneBasis basis)
		{
			DrawBasis(parent, basis);

			for (int i = 0; i < parent.childCount; i++)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawLine(parent.transform.position,
					parent.GetChild(i).position);

				DrawBones(parent.GetChild(i), basis);
			}
		}

		protected void DrawBasis(Transform bone, BoneBasis basis)
		{
			Quaternion rotation = GlobalRotationFromBasis(bone, basis);

			Vector3 up, forward, right;

			up = rotation * Vector3.up;
			forward = rotation * Vector3.forward;
			right = rotation * Vector3.right;

			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(bone.position, bone.position + (up * 0.025f));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(bone.position, bone.position + (right * 0.025f));
			Gizmos.color = Color.blue;
			Gizmos.DrawLine(bone.position, bone.position + (forward * 0.025f));
		}
	}
}