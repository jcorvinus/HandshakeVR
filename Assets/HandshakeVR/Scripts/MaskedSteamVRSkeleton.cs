using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class MaskedSteamVRSkeleton : SteamVR_Behaviour_Skeleton
	{
		/// <summary>
		/// How much of a blend to apply to the transform positions and rotations. 
		/// Set to 0 for the transform orientation to be set by an animation. 
		/// Set to 1 for the transform orientation to be set by the skeleton action.
		/// </summary>
		[Range(0, 1)]
		[Tooltip("Modify this to blend between animations setup on the hand")]
		public float thumbSkeletonBlend = 1f;

		[Range(0, 1)]
		public float indexSkeletonBlend = 1;

		[Range(0, 1)]
		public float middleSkeletonBlend = 1;

		[Range(0, 1)]
		public float ringSkeletonBlend = 1;

		[Range(0, 1)]
		public float pinkySkeletonBlend = 1;

		SteamVR_Skeleton_FingerIndexEnum GetFingerForBone(int boneID)
		{
			SteamVR_Skeleton_JointIndexEnum jointIndexEnum = (SteamVR_Skeleton_JointIndexEnum)boneID;

			switch (jointIndexEnum)
			{
				case SteamVR_Skeleton_JointIndexEnum.root:
					return (SteamVR_Skeleton_FingerIndexEnum)(-1);
				case SteamVR_Skeleton_JointIndexEnum.wrist:
					return (SteamVR_Skeleton_FingerIndexEnum)(-1);

				case SteamVR_Skeleton_JointIndexEnum.thumbProximal:
				case SteamVR_Skeleton_JointIndexEnum.thumbMiddle:
				case SteamVR_Skeleton_JointIndexEnum.thumbDistal:
				case SteamVR_Skeleton_JointIndexEnum.thumbTip:
					return SteamVR_Skeleton_FingerIndexEnum.thumb;

				case SteamVR_Skeleton_JointIndexEnum.indexMetacarpal:
				case SteamVR_Skeleton_JointIndexEnum.indexProximal:
				case SteamVR_Skeleton_JointIndexEnum.indexMiddle:
				case SteamVR_Skeleton_JointIndexEnum.indexDistal:
				case SteamVR_Skeleton_JointIndexEnum.indexTip:
					return SteamVR_Skeleton_FingerIndexEnum.index;

				case SteamVR_Skeleton_JointIndexEnum.middleMetacarpal:
				case SteamVR_Skeleton_JointIndexEnum.middleProximal:
				case SteamVR_Skeleton_JointIndexEnum.middleMiddle:
				case SteamVR_Skeleton_JointIndexEnum.middleDistal:
				case SteamVR_Skeleton_JointIndexEnum.middleTip:
					return SteamVR_Skeleton_FingerIndexEnum.middle;

				case SteamVR_Skeleton_JointIndexEnum.ringMetacarpal:
				case SteamVR_Skeleton_JointIndexEnum.ringProximal:
				case SteamVR_Skeleton_JointIndexEnum.ringMiddle:
				case SteamVR_Skeleton_JointIndexEnum.ringDistal:
				case SteamVR_Skeleton_JointIndexEnum.ringTip:
					return SteamVR_Skeleton_FingerIndexEnum.ring;

				case SteamVR_Skeleton_JointIndexEnum.pinkyMetacarpal:
				case SteamVR_Skeleton_JointIndexEnum.pinkyProximal:
				case SteamVR_Skeleton_JointIndexEnum.pinkyMiddle:
				case SteamVR_Skeleton_JointIndexEnum.pinkyDistal:
				case SteamVR_Skeleton_JointIndexEnum.pinkyTip:
					return SteamVR_Skeleton_FingerIndexEnum.pinky;

				case SteamVR_Skeleton_JointIndexEnum.thumbAux:
					return SteamVR_Skeleton_FingerIndexEnum.thumb;

				case SteamVR_Skeleton_JointIndexEnum.indexAux:
					return SteamVR_Skeleton_FingerIndexEnum.index;

				case SteamVR_Skeleton_JointIndexEnum.middleAux:
					return SteamVR_Skeleton_FingerIndexEnum.middle;

				case SteamVR_Skeleton_JointIndexEnum.ringAux:
					return SteamVR_Skeleton_FingerIndexEnum.ring;

				case SteamVR_Skeleton_JointIndexEnum.pinkyAux:
					return SteamVR_Skeleton_FingerIndexEnum.pinky;

				default:
					return (SteamVR_Skeleton_FingerIndexEnum)(-1);
			}
		}

		public override void UpdateSkeletonTransforms()
		{
			Vector3[] bonePositions = GetBonePositions();
			Quaternion[] boneRotations = GetBoneRotations();

			for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
			{
				if (bones[boneIndex] == null)
					continue;

				float skeletonBlend = 1;

				// get proper skeletal blend value
				SteamVR_Skeleton_FingerIndexEnum finger = GetFingerForBone(boneIndex);

				switch (finger)
				{
					case SteamVR_Skeleton_FingerIndexEnum.thumb:
						skeletonBlend = thumbSkeletonBlend;
						break;
					case SteamVR_Skeleton_FingerIndexEnum.index:
						skeletonBlend = indexSkeletonBlend;
						break;
					case SteamVR_Skeleton_FingerIndexEnum.middle:
						skeletonBlend = middleSkeletonBlend;
						break;
					case SteamVR_Skeleton_FingerIndexEnum.ring:
						skeletonBlend = ringSkeletonBlend;
						break;
					case SteamVR_Skeleton_FingerIndexEnum.pinky:
						skeletonBlend = pinkySkeletonBlend;
						break;
					default:
						break;
				}

				if (skeletonBlend >= 1)
				{
					SetBonePosition(boneIndex, bonePositions[boneIndex]);
					SetBoneRotation(boneIndex, boneRotations[boneIndex]);
				}
				else
				{
					SetBonePosition(boneIndex, Vector3.Lerp(bones[boneIndex].localPosition, bonePositions[boneIndex], skeletonBlend));
					SetBoneRotation(boneIndex, Quaternion.Lerp(bones[boneIndex].localRotation, boneRotations[boneIndex], skeletonBlend));
				}
			}
		}
	}
}