using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class MaskedSteamVRSkeleton : MonoBehaviour, ISteamVR_Action_Skeleton
	{
		[Tooltip("If not set, will try to auto assign this based on 'Skeleton' + inputSource")]
		/// <summary>The action this component will use to update the model. Must be a Skeleton type action.</summary>
		public SteamVR_Action_Skeleton skeletonAction;

		/// <summary>The device this action should apply to. Any if the action is not device specific.</summary>
		[Tooltip("The device this action should apply to. Any if the action is not device specific.")]
		public SteamVR_Input_Sources inputSource;

		/// <summary>The range of motion you'd like the hand to move in. With controller is the best estimate of the fingers wrapped around a controller. Without is from a flat hand to a fist.</summary>
		[Tooltip("The range of motion you'd like the hand to move in. With controller is the best estimate of the fingers wrapped around a controller. Without is from a flat hand to a fist.")]
		public EVRSkeletalMotionRange rangeOfMotion = EVRSkeletalMotionRange.WithoutController;

		/// <summary>The root Transform of the skeleton. Needs to have a child (wrist) then wrist should have children in the order thumb, index, middle, ring, pinky</summary>
		[Tooltip("This needs to be in the order of: root -> wrist -> thumb, index, middle, ring, pinky")]
		public Transform skeletonRoot;

		/// <summary>The transform this transform should be relative to</summary>
		[Tooltip("If not set, relative to parent")]
		public Transform origin;

		/// <summary>Whether or not to update this transform's position and rotation inline with the skeleton transforms or if this is handled in another script</summary>
		[Tooltip("Set to true if you want this script to update its position and rotation. False if this will be handled elsewhere")]
		public bool updatePose = true;

		/// <summary>Check this to not set the positions of the bones. This is helpful for differently scaled skeletons.</summary>
		[Tooltip("Check this to not set the positions of the bones. This is helpful for differently scaled skeletons.")]
		public bool onlySetRotations = false;

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

		/// <summary>Can be set to mirror the bone data across the x axis</summary>
		[Tooltip("Is this rendermodel a mirror of another one?")]
		public MirrorType mirroring;

		/// <summary>Returns whether this action is bound and the action set is active</summary>
		public bool isActive { get { return skeletonAction.GetActive(); } }


		/// <summary>An array of five 0-1 values representing how curled a finger is. 0 being straight, 1 being fully curled. Index 0 being thumb, index 4 being pinky</summary>
		public float[] fingerCurls { get { return skeletonAction.GetFingerCurls(); } }

		/// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
		public float thumbCurl { get { return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.thumb); } }

		/// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
		public float indexCurl { get { return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.index); } }

		/// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
		public float middleCurl { get { return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.middle); } }

		/// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
		public float ringCurl { get { return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.ring); } }

		/// <summary>An 0-1 value representing how curled a finger is. 0 being straight, 1 being fully curled.</summary>
		public float pinkyCurl { get { return skeletonAction.GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum.pinky); } }


		public Transform root { get { return bones[SteamVR_Skeleton_JointIndexes.root]; } }
		public Transform wrist { get { return bones[SteamVR_Skeleton_JointIndexes.wrist]; } }
		public Transform indexMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.indexMetacarpal]; } }
		public Transform indexProximal { get { return bones[SteamVR_Skeleton_JointIndexes.indexProximal]; } }
		public Transform indexMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.indexMiddle]; } }
		public Transform indexDistal { get { return bones[SteamVR_Skeleton_JointIndexes.indexDistal]; } }
		public Transform indexTip { get { return bones[SteamVR_Skeleton_JointIndexes.indexTip]; } }
		public Transform middleMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.middleMetacarpal]; } }
		public Transform middleProximal { get { return bones[SteamVR_Skeleton_JointIndexes.middleProximal]; } }
		public Transform middleMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.middleMiddle]; } }
		public Transform middleDistal { get { return bones[SteamVR_Skeleton_JointIndexes.middleDistal]; } }
		public Transform middleTip { get { return bones[SteamVR_Skeleton_JointIndexes.middleTip]; } }
		public Transform pinkyMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyMetacarpal]; } }
		public Transform pinkyProximal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyProximal]; } }
		public Transform pinkyMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyMiddle]; } }
		public Transform pinkyDistal { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyDistal]; } }
		public Transform pinkyTip { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyTip]; } }
		public Transform ringMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.ringMetacarpal]; } }
		public Transform ringProximal { get { return bones[SteamVR_Skeleton_JointIndexes.ringProximal]; } }
		public Transform ringMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.ringMiddle]; } }
		public Transform ringDistal { get { return bones[SteamVR_Skeleton_JointIndexes.ringDistal]; } }
		public Transform ringTip { get { return bones[SteamVR_Skeleton_JointIndexes.ringTip]; } }
		public Transform thumbMetacarpal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbMetacarpal]; } } //doesn't exist - mapped to proximal
		public Transform thumbProximal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbProximal]; } }
		public Transform thumbMiddle { get { return bones[SteamVR_Skeleton_JointIndexes.thumbMiddle]; } }
		public Transform thumbDistal { get { return bones[SteamVR_Skeleton_JointIndexes.thumbDistal]; } }
		public Transform thumbTip { get { return bones[SteamVR_Skeleton_JointIndexes.thumbTip]; } }
		public Transform thumbAux { get { return bones[SteamVR_Skeleton_JointIndexes.thumbAux]; } }
		public Transform indexAux { get { return bones[SteamVR_Skeleton_JointIndexes.indexAux]; } }
		public Transform middleAux { get { return bones[SteamVR_Skeleton_JointIndexes.middleAux]; } }
		public Transform ringAux { get { return bones[SteamVR_Skeleton_JointIndexes.ringAux]; } }
		public Transform pinkyAux { get { return bones[SteamVR_Skeleton_JointIndexes.pinkyAux]; } }

		/// <summary>An array of all the finger proximal joint transforms</summary>
		public Transform[] proximals { get; protected set; }

		/// <summary>An array of all the finger middle joint transforms</summary>
		public Transform[] middles { get; protected set; }

		/// <summary>An array of all the finger distal joint transforms</summary>
		public Transform[] distals { get; protected set; }

		/// <summary>An array of all the finger tip transforms</summary>
		public Transform[] tips { get; protected set; }

		/// <summary>An array of all the finger aux transforms</summary>
		public Transform[] auxs { get; protected set; }

		protected Coroutine blendRoutine;
		protected Coroutine rangeOfMotionBlendRoutine;

		protected Transform[] bones;

		/// <summary>The range of motion that is set temporarily (call ResetTemporaryRangeOfMotion to reset to rangeOfMotion)</summary>
		protected EVRSkeletalMotionRange? temporaryRangeOfMotion = null;

		/// <summary>Returns true if we are in the process of blending the skeletonBlend field (between animation and bone data)</summary>
		public bool isBlending
		{
			get
			{
				return blendRoutine != null;
			}
		}

		public float predictedSecondsFromNow
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).predictedSecondsFromNow;
			}

			set
			{
				((ISteamVR_Action_Skeleton)skeletonAction).predictedSecondsFromNow = value;
			}
		}

		public float changeTolerance
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).changeTolerance;
			}

			set
			{
				((ISteamVR_Action_Skeleton)skeletonAction).changeTolerance = value;
			}
		}

		public string fullPath
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).fullPath;
			}
		}

		public ulong handle
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).handle;
			}
		}

		public SteamVR_ActionSet actionSet
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).actionSet;
			}
		}

		public SteamVR_ActionDirections direction
		{
			get
			{
				return ((ISteamVR_Action_Skeleton)skeletonAction).direction;
			}
		}

		protected virtual void Awake()
		{
			AssignBonesArray();

			proximals = new Transform[] { thumbProximal, indexProximal, middleProximal, ringProximal, pinkyProximal };
			middles = new Transform[] { thumbMiddle, indexMiddle, middleMiddle, ringMiddle, pinkyMiddle };
			distals = new Transform[] { thumbDistal, indexDistal, middleDistal, ringDistal, pinkyDistal };
			tips = new Transform[] { thumbTip, indexTip, middleTip, ringTip, pinkyTip };
			auxs = new Transform[] { thumbAux, indexAux, middleAux, ringAux, pinkyAux };

			if (skeletonAction == null)
				skeletonAction = SteamVR_Input.GetAction<SteamVR_Action_Skeleton>("Skeleton" + inputSource.ToString());
		}

		protected virtual void AssignBonesArray()
		{
			bones = skeletonRoot.GetComponentsInChildren<Transform>();
		}

		protected virtual void OnEnable()
		{
			SteamVR_Input.OnSkeletonsUpdated += SteamVR_Input_OnSkeletonsUpdated;
		}

		protected virtual void OnDisable()
		{
			SteamVR_Input.OnSkeletonsUpdated -= SteamVR_Input_OnSkeletonsUpdated;
		}

		protected virtual void SteamVR_Input_OnSkeletonsUpdated(bool obj)
		{
			UpdateSkeleton();
		}

		protected virtual void UpdateSkeleton()
		{

			if (skeletonAction == null || skeletonAction.GetActive() == false)
				return;

			if (updatePose)
				UpdatePose();

			if (rangeOfMotionBlendRoutine == null)
			{
				if (temporaryRangeOfMotion != null)
					skeletonAction.SetRangeOfMotion(temporaryRangeOfMotion.Value);
				else
					skeletonAction.SetRangeOfMotion(rangeOfMotion); //this may be a frame behind

				UpdateSkeletonTransforms();
			}
		}

		/// <summary>
		/// Sets a temporary range of motion for this action that can easily be reset (using ResetTemporaryRangeOfMotion).
		/// This is useful for short range of motion changes, for example picking up a controller shaped object
		/// </summary>
		/// <param name="newRangeOfMotion">The new range of motion you want to apply (temporarily)</param>
		/// <param name="blendOverSeconds">How long you want the blend to the new range of motion to take (in seconds)</param>
		public void SetTemporaryRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
		{
			if (rangeOfMotion != newRangeOfMotion || temporaryRangeOfMotion != newRangeOfMotion)
			{
				TemporaryRangeOfMotionBlend(newRangeOfMotion, blendOverSeconds);
			}
		}

		/// <summary>
		/// Resets the previously set temporary range of motion. 
		/// Will return to the range of motion defined by the rangeOfMotion field.
		/// </summary>
		/// <param name="blendOverSeconds">How long you want the blend to the standard range of motion to take (in seconds)</param>
		public void ResetTemporaryRangeOfMotion(float blendOverSeconds = 0.1f)
		{
			ResetTemporaryRangeOfMotionBlend(blendOverSeconds);
		}

		/// <summary>
		/// Permanently sets the range of motion for this component.
		/// </summary>
		/// <param name="newRangeOfMotion">
		/// The new range of motion to be set. 
		/// WithController being the best estimation of where fingers are wrapped around the controller (pressing buttons, etc). 
		/// WithoutController being a range between a flat hand and a fist.</param>
		/// <param name="blendOverSeconds">How long you want the blend to the new range of motion to take (in seconds)</param>
		public void SetRangeOfMotion(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds = 0.1f)
		{
			if (rangeOfMotion != newRangeOfMotion)
			{
				RangeOfMotionBlend(newRangeOfMotion, blendOverSeconds);
			}
		}

		/// <summary>
		/// Blend from the current skeletonBlend amount to full bone data. (skeletonBlend = 1)
		/// </summary>
		/// <param name="overTime">How long you want the blend to take (in seconds)</param>
		public void BlendToSkeleton(float overTime = 0.1f)
		{
			BlendTo(1, overTime);
		}

		/// <summary>
		/// Blend from the current skeletonBlend amount to full animation data (no bone data. skeletonBlend = 0)
		/// </summary>
		/// <param name="overTime">How long you want the blend to take (in seconds)</param>
		public void BlendToAnimation(float overTime = 0.1f)
		{
			BlendTo(0, overTime);
		}

		/// <summary>
		/// Blend from the current skeletonBlend amount to a specified new amount.
		/// </summary>
		/// <param name="blendToAmount">The amount of blend you want to apply. 
		/// 0 being fully set by animations, 1 being fully set by bone data from the action.</param>
		/// <param name="overTime">How long you want the blend to take (in seconds)</param>
		public void BlendTo(float blendToAmount, float overTime)
		{
			if (blendRoutine != null)
				StopCoroutine(blendRoutine);

			if (this.gameObject.activeInHierarchy)
				blendRoutine = StartCoroutine(DoBlendRoutine(blendToAmount, overTime));
		}

		protected IEnumerator DoBlendRoutine(float blendToAmount, float overTime)
		{
			float startTime = Time.time;
			float endTime = startTime + overTime;

			//float startAmount = skeletonBlend;

			while (Time.time < endTime)
			{
				yield return null;
				//skeletonBlend = Mathf.Lerp(startAmount, blendToAmount, (Time.time - startTime) / overTime);
			}

			//skeletonBlend = blendToAmount;
			blendRoutine = null;
		}

		protected void RangeOfMotionBlend(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds)
		{
			if (rangeOfMotionBlendRoutine != null)
				StopCoroutine(rangeOfMotionBlendRoutine);

			EVRSkeletalMotionRange oldRangeOfMotion = rangeOfMotion;
			rangeOfMotion = newRangeOfMotion;

			if (this.gameObject.activeInHierarchy)
			{
				rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
			}
		}

		protected void TemporaryRangeOfMotionBlend(EVRSkeletalMotionRange newRangeOfMotion, float blendOverSeconds)
		{
			if (rangeOfMotionBlendRoutine != null)
				StopCoroutine(rangeOfMotionBlendRoutine);

			EVRSkeletalMotionRange oldRangeOfMotion = rangeOfMotion;
			if (temporaryRangeOfMotion != null)
				oldRangeOfMotion = temporaryRangeOfMotion.Value;

			temporaryRangeOfMotion = newRangeOfMotion;

			if (this.gameObject.activeInHierarchy)
			{
				rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
			}
		}

		protected void ResetTemporaryRangeOfMotionBlend(float blendOverSeconds)
		{
			if (temporaryRangeOfMotion != null)
			{
				if (rangeOfMotionBlendRoutine != null)
					StopCoroutine(rangeOfMotionBlendRoutine);

				EVRSkeletalMotionRange oldRangeOfMotion = temporaryRangeOfMotion.Value;

				EVRSkeletalMotionRange newRangeOfMotion = rangeOfMotion;

				temporaryRangeOfMotion = null;

				if (this.gameObject.activeInHierarchy)
				{
					rangeOfMotionBlendRoutine = StartCoroutine(DoRangeOfMotionBlend(oldRangeOfMotion, newRangeOfMotion, blendOverSeconds));
				}
			}
		}

		protected IEnumerator DoRangeOfMotionBlend(EVRSkeletalMotionRange oldRangeOfMotion, EVRSkeletalMotionRange newRangeOfMotion, float overTime)
		{
			float startTime = Time.time;
			float endTime = startTime + overTime;

			Vector3[] oldBonePositions;
			Quaternion[] oldBoneRotations;

			Vector3[] newBonePositions;
			Quaternion[] newBoneRotations;

			while (Time.time < endTime)
			{
				yield return null;
				float lerp = (Time.time - startTime) / overTime;
			}


			rangeOfMotionBlendRoutine = null;
		}

		protected virtual void UpdateSkeletonTransforms()
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

		protected virtual void SetBonePosition(int boneIndex, Vector3 localPosition)
		{
			if (onlySetRotations == false) //ignore position sets if we're only setting rotations
				bones[boneIndex].localPosition = localPosition;
		}

		protected virtual void SetBoneRotation(int boneIndex, Quaternion localRotation)
		{
			bones[boneIndex].localRotation = localRotation;
		}

		/// <summary>
		/// Gets the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes 
		/// </summary>
		/// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
		public virtual Transform GetBone(int joint)
		{
			return bones[joint];
		}


		/// <summary>
		/// Gets the position of the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes 
		/// </summary>
		/// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
		/// <param name="local">true to get the localspace position for the joint (position relative to this joint's parent)</param>
		public Vector3 GetBonePosition(int joint, bool local = false)
		{
			if (local)
				return bones[joint].localPosition;
			else
				return bones[joint].position;
		}

		/// <summary>
		/// Gets the rotation of the transform for a bone by the joint index. Joint indexes specified in: SteamVR_Skeleton_JointIndexes 
		/// </summary>
		/// <param name="joint">The joint index of the bone. Specified in SteamVR_Skeleton_JointIndexes</param>
		/// <param name="local">true to get the localspace rotation for the joint (rotation relative to this joint's parent)</param>
		public Quaternion GetBoneRotation(int joint, bool local = false)
		{
			if (local)
				return bones[joint].localRotation;
			else
				return bones[joint].rotation;
		}

		protected Vector3[] GetBonePositions()
		{
			Vector3[] rawSkeleton = skeletonAction.GetBonePositions();
			if (mirroring == MirrorType.LeftToRight || mirroring == MirrorType.RightToLeft)
			{
				for (int boneIndex = 0; boneIndex < rawSkeleton.Length; boneIndex++)
				{
					if (boneIndex == SteamVR_Skeleton_JointIndexes.wrist || IsMetacarpal(boneIndex))
					{
						rawSkeleton[boneIndex].Scale(new Vector3(-1, 1, 1));
					}
					else if (boneIndex != SteamVR_Skeleton_JointIndexes.root)
					{
						rawSkeleton[boneIndex] = rawSkeleton[boneIndex] * -1;
					}
				}
			}

			return rawSkeleton;
		}

		protected Quaternion rightFlipAngle = Quaternion.AngleAxis(180, Vector3.right);
		protected Quaternion[] GetBoneRotations()
		{
			Quaternion[] rawSkeleton = skeletonAction.GetBoneRotations();
			if (mirroring == MirrorType.LeftToRight || mirroring == MirrorType.RightToLeft)
			{
				for (int boneIndex = 0; boneIndex < rawSkeleton.Length; boneIndex++)
				{
					if (boneIndex == SteamVR_Skeleton_JointIndexes.wrist)
					{
						rawSkeleton[boneIndex].y = rawSkeleton[boneIndex].y * -1;
						rawSkeleton[boneIndex].z = rawSkeleton[boneIndex].z * -1;
					}

					if (IsMetacarpal(boneIndex))
					{
						rawSkeleton[boneIndex] = rightFlipAngle * rawSkeleton[boneIndex];
					}
				}
			}

			return rawSkeleton;
		}

		protected virtual void UpdatePose()
		{
			if (skeletonAction == null)
				return;

			if (origin == null)
				skeletonAction.UpdateTransform(this.transform);
			else
			{
				this.transform.position = origin.TransformPoint(skeletonAction.GetLocalPosition());
				this.transform.eulerAngles = origin.TransformDirection(skeletonAction.GetLocalRotation().eulerAngles);
			}
		}

		public enum MirrorType
		{
			None,
			LeftToRight,
			RightToLeft
		}

		protected bool IsMetacarpal(int boneIndex)
		{
			return (boneIndex == SteamVR_Skeleton_JointIndexes.indexMetacarpal ||
				boneIndex == SteamVR_Skeleton_JointIndexes.middleMetacarpal ||
				boneIndex == SteamVR_Skeleton_JointIndexes.ringMetacarpal ||
				boneIndex == SteamVR_Skeleton_JointIndexes.pinkyMetacarpal ||
				boneIndex == SteamVR_Skeleton_JointIndexes.thumbMetacarpal);
		}

		Vector3[] ISteamVR_Action_Skeleton.GetBonePositions()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetBonePositions();
		}

		Quaternion[] ISteamVR_Action_Skeleton.GetBoneRotations()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetBoneRotations();
		}

		/// <summary>
		/// Gets the bone positions in local space from the previous update
		/// </summary>
		public Vector3[] GetLastBonePositions()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastBonePositions();
		}

		/// <summary>
		/// Gets the bone rotations in local space from the previous update
		/// </summary>
		public Quaternion[] GetLastBoneRotations()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastBoneRotations();
		}

		/// <summary>
		/// Set the range of the motion of the bones in this skeleton. Options are "With Controller" as if your hand is holding your VR controller. 
		/// Or "Without Controller" as if your hand is empty.
		/// </summary>
		public void SetRangeOfMotion(EVRSkeletalMotionRange range)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).SetRangeOfMotion(range);
		}

		/// <summary>
		/// Sets the space that you'll get bone data back in. Options are relative to the Model, relative to the Parent bone, and Additive.
		/// </summary>
		/// <param name="space">the space that you'll get bone data back in. Options are relative to the Model, relative to the Parent bone, and Additive.</param>
		public void SetSkeletalTransformSpace(EVRSkeletalTransformSpace space)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).SetSkeletalTransformSpace(space);
		}

		/// <summary>
		/// Returns the total number of bones in the skeleton
		/// </summary>
		public uint GetBoneCount()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetBoneCount();
		}

		/// <summary>
		/// Returns the order of bones in the hierarchy
		/// </summary>
		public int[] GetBoneHierarchy()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetBoneHierarchy();
		}

		/// <summary>
		/// Returns the name of the bone
		/// </summary>
		public string GetBoneName(int boneIndex)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetBoneName(boneIndex);
		}

		/// <summary>
		/// Returns an array of positions/rotations that represent the state of each bone in a reference pose.
		/// </summary>
		/// <param name="transformSpace">What to get the position/rotation data relative to, the model, or the bone's parent</param>
		/// <param name="referencePose">Which reference pose to return</param>
		/// <returns></returns>
		public SteamVR_Utils.RigidTransform[] GetReferenceTransforms(EVRSkeletalTransformSpace transformSpace, EVRSkeletalReferencePose referencePose)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetReferenceTransforms(transformSpace, referencePose);
		}

		/// <summary>
		/// Get the accuracy level of the skeletal tracking data. 
		/// </summary>
		/// <returns>
		/// <list type="bullet">
		/// <item><description>Estimated: Body part location can’t be directly determined by the device. Any skeletal pose provided by the device is estimated based on the active buttons, triggers, joysticks, or other input sensors. Examples include the Vive Controller and gamepads. </description></item>
		/// <item><description>Partial: Body part location can be measured directly but with fewer degrees of freedom than the actual body part.Certain body part positions may be unmeasured by the device and estimated from other input data.Examples include Knuckles or gloves that only measure finger curl</description></item>
		/// <item><description>Full: Body part location can be measured directly throughout the entire range of motion of the body part.Examples include hi-end mocap systems, or gloves that measure the rotation of each finger segment.</description></item>
		/// </list>
		/// </returns>
		public EVRSkeletalTrackingLevel GetSkeletalTrackingLevel()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetSkeletalTrackingLevel();
		}

		/// <summary>
		/// Get the skeletal summary data structure from openvr. Contains curl and splay data in finger order: thumb, index, middlg, ring, pinky.
		/// </summary>
		public VRSkeletalSummaryData_t GetSkeletalSummaryData(bool force = false)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetSkeletalSummaryData();
		}


		/// <summary>
		/// Returns the finger curl data that we calculate each update. This array may be modified later so if you want to hold this data then pass true to get a copy of the data instead of the actual array
		/// </summary>
		/// <param name="copy">This array may be modified later so if you want to hold this data then pass true to get a copy of the data instead of the actual array</param>
		public float[] GetFingerCurls(bool copy = false)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetFingerCurls(copy);
		}

		/// <summary>
		/// Returns a value indicating how much the passed in finger is currently curled.
		/// </summary>
		/// <param name="finger">The index of the finger to return a curl value for. 0-4. thumb, index, middle, ring, pinky</param>
		/// <returns>0-1 value. 0 being straight, 1 being fully curled.</returns>
		public float GetFingerCurl(int finger)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetFingerCurl(finger);
		}

		/// <summary>
		/// Returns a value indicating how the size of the gap between fingers.
		/// </summary>
		/// <param name="fingerGapIndex">The index of the finger gap to return a splay value for. 0 being the gap between thumb and index, 1 being the gap between index and middle, 2 being the gap between middle and ring, and 3 being the gap between ring and pinky.
		/// <returns>0-1 value. 0 being no gap, 1 being "full" gap</returns>
		public float GetSplay(int fingerGapIndex)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetSplay(fingerGapIndex);
		}

		/// <summary>
		/// Returns a value indicating how much the passed in finger is currently curled.
		/// </summary>
		/// <param name="finger">The finger to return a curl value for</param>
		/// <returns>0-1 value. 0 being straight, 1 being fully curled.</returns>
		public float GetFingerCurl(SteamVR_Skeleton_FingerIndexEnum finger)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetFingerCurl(finger);
		}

		/// <summary>
		/// Returns a value indicating how the size of the gap between fingers.
		/// </summary>
		/// <param name="fingerGapIndex">The finger gap to return a splay value for. thumb being the gap between thumb and index, index being the gap between index and middle, middle being the gap between middle and ring, and ring being the gap between ring and pinky.
		/// <returns>0-1 value. 0 being no gap, 1 being "full" gap</returns>
		public float GetSplay(SteamVR_Skeleton_FingerIndexEnum finger)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetSplay(finger);
		}

		/// <summary>Executes a function when this action's bound state changes</summary>
		/// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
		public void AddOnActiveChangeListener(Action<SteamVR_Action_Skeleton, bool> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnActiveChangeListener(functionToCall);
		}

		/// <summary>Stops executing the function setup by the corresponding AddListener</summary>
		/// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
		public void RemoveOnActiveChangeListener(Action<SteamVR_Action_Skeleton, bool> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnActiveChangeListener(functionToStopCalling);
		}

		/// <summary>Executes a function when the state of this action (with the specified inputSource) changes</summary>
		/// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
		public void AddOnChangeListener(Action<SteamVR_Action_Skeleton> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnChangeListener(functionToCall);
		}

		/// <summary>Stops executing the function setup by the corresponding AddListener</summary>
		/// <param name="functionToStopCalling">The local function that you've setup to receive on change events</param>
		public void RemoveOnChangeListener(Action<SteamVR_Action_Skeleton> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnChangeListener(functionToStopCalling);
		}

		/// <summary>Executes a function when the state of this action (with the specified inputSource) is updated.</summary>
		/// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
		public void AddOnUpdateListener(Action<SteamVR_Action_Skeleton> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnUpdateListener(functionToCall);
		}

		/// <summary>Stops executing the function setup by the corresponding AddListener</summary>
		/// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
		public void RemoveOnUpdateListener(Action<SteamVR_Action_Skeleton> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnUpdateListener(functionToStopCalling);
		}

		#region pose stuff
		public bool GetVelocitiesAtTimeOffset(SteamVR_Input_Sources inputSource, float secondsFromNow, out Vector3 velocity, out Vector3 angularVelocity)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetVelocitiesAtTimeOffset(inputSource, secondsFromNow, out velocity, out angularVelocity);
		}

		public bool GetPoseAtTimeOffset(SteamVR_Input_Sources inputSource, float secondsFromNow, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetPoseAtTimeOffset(inputSource, secondsFromNow, out position, out rotation, out velocity, out angularVelocity);
		}

		public void UpdateTransform(SteamVR_Input_Sources inputSource, Transform transformToUpdate)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).UpdateTransform(inputSource, transformToUpdate);
		}

		public Vector3 GetLocalPosition(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLocalPosition(inputSource);
		}

		public Quaternion GetLocalRotation(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLocalRotation(inputSource);
		}

		public Vector3 GetVelocity(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetVelocity(inputSource);
		}

		public Vector3 GetAngularVelocity(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetAngularVelocity(inputSource);
		}

		public bool GetDeviceIsConnected(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetDeviceIsConnected(inputSource);
		}

		public bool GetPoseIsValid(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetPoseIsValid(inputSource);
		}

		public ETrackingResult GetTrackingResult(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetTrackingResult(inputSource);
		}

		public Vector3 GetLastLocalPosition(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastLocalPosition(inputSource);
		}

		public Quaternion GetLastLocalRotation(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastLocalRotation(inputSource);
		}

		public Vector3 GetLastVelocity(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastVelocity(inputSource);
		}

		public Vector3 GetLastAngularVelocity(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastAngularVelocity(inputSource);
		}

		public bool GetLastDeviceIsConnected(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastDeviceIsConnected(inputSource);
		}

		public bool GetLastPoseIsValid(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastPoseIsValid(inputSource);
		}

		public ETrackingResult GetLastTrackingResult(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetLastTrackingResult(inputSource);
		}

		public void AddOnDeviceConnectedChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnDeviceConnectedChanged(inputSource, functionToCall);
		}

		public void RemoveOnDeviceConnectedChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnDeviceConnectedChanged(inputSource, functionToStopCalling);
		}

		public void AddOnTrackingChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, ETrackingResult> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnTrackingChanged(inputSource, functionToCall);
		}

		public void RemoveOnTrackingChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, ETrackingResult> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnTrackingChanged(inputSource, functionToStopCalling);
		}

		public void AddOnValidPoseChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnValidPoseChanged(inputSource, functionToCall);
		}

		public void RemoveOnValidPoseChanged(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnValidPoseChanged(inputSource, functionToStopCalling);
		}

		public void AddOnActiveChangeListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnActiveChangeListener(inputSource, functionToCall);
		}

		public void RemoveOnActiveChangeListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources, bool> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnActiveChangeListener(inputSource, functionToStopCalling);
		}

		public void AddOnChangeListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnChangeListener(inputSource, functionToCall);
		}

		public void RemoveOnChangeListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnChangeListener(inputSource, functionToStopCalling);
		}

		public void AddOnUpdateListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources> functionToCall)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).AddOnUpdateListener(inputSource, functionToCall);
		}

		public void RemoveOnUpdateListener(SteamVR_Input_Sources inputSource, Action<SteamVR_Action_Pose, SteamVR_Input_Sources> functionToStopCalling)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).RemoveOnUpdateListener(inputSource, functionToStopCalling);
		}

		public void UpdateValue(SteamVR_Input_Sources inputSource)
		{
			((ISteamVR_Action_Skeleton)skeletonAction).UpdateValue(inputSource);
		}

		public string GetDeviceComponentName(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetDeviceComponentName(inputSource);
		}

		public ulong GetDevicePath(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetDevicePath(inputSource);
		}

		public uint GetDeviceIndex(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetDeviceIndex(inputSource);
		}

		public bool GetChanged(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetChanged(inputSource);
		}

		public bool GetActive(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetActive(inputSource);
		}

		public float GetTimeLastChanged(SteamVR_Input_Sources inputSource)
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetTimeLastChanged(inputSource);
		}

		public string GetShortName()
		{
			return ((ISteamVR_Action_Skeleton)skeletonAction).GetShortName();
		}
		#endregion
	}
}