using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

using Oculus;

namespace HandshakeVR
{
	public class OculusRemapper : HandInputProvider
	{
		[SerializeField] Transform controllerTransform;
		[SerializeField] RuntimeAnimatorController animatorController;

		ControllerOffset ovrTouchOffset = new ControllerOffset()
		{
			LocalPositionLeft = new Vector3(-0.026f, 0.01f, -0.094f),
			LocalRotationLeft = new Vector3(56.448f, 166.685f, 78.11401f),

			LocalPositionRight = new Vector3(-0.026f, 0.01f, -0.094f),
			LocalRotationRight = new Vector3(56.448f, 166.685f, 78.11401f)
		};

		[SerializeField]
		Vector3 localPosOffset = new Vector3(-0.026f, 0.01f, -0.094f);
		[SerializeField]
		Vector3 localRotOffset = new Vector3(56.448f, 116.685f, 78.11401f);

		bool poseCanGrip;

		int xAxisHash, yAxisHash, gripHash;
		int indexGripHash, thumbHash, pinchHash;
		float thumbValue;
		float indexGripValue;

		bool triggerTouch, gripTouch, stickTouch,
			upButtonTouch, downButtonTouch, thumbrestTouch;

		bool controllerIsPinching;

		[Header("Controller Gesture Vars")]
		[Range(0, 1)] [SerializeField] float grabValueNoTouchFloor = 0.4f;
		[Range(0, 1)] [SerializeField] float grabValueTouchFloor = 0.56f;
		[Range(0, 1)] [SerializeField] float thumbValueTouchCeiling = 0.75f;
		[Range(0, 1)] [SerializeField] float indexTouchFloor = 0.35f;

		[Header("Hand Remapping Vars")]
		[SerializeField] Vector3 fingerForward;
		[SerializeField] Vector3 fingerUp;
		OVRHand hand;
		OVRSkeleton skeleton;
		OVRPlugin.Skeleton2 rawSkeleton;
		FieldInfo skeletonField = null;
		const float tipScale=0.659f;

		[Header("Debug Vars")]
		[SerializeField] bool drawDebugMesh;
		[SerializeField] GameObject debugMesh;

		[SerializeField] bool drawSkeleton;
		[SerializeField] bool drawBindPose;
		[SerializeField] bool drawBasis;

		public override HandTrackingType TrackingType()
		{
			OVRInput.Controller controllerType = OVRInput.GetActiveController();
			switch (controllerType)
			{
				// touch cases
				case OVRInput.Controller.LTouch:
				case OVRInput.Controller.RTouch:
				case OVRInput.Controller.Touch:
					return HandTrackingType.Emulation;

				// hands cases
				case OVRInput.Controller.Hands:
				case OVRInput.Controller.LHand:
				case OVRInput.Controller.RHand:
					return HandTrackingType.Skeletal;

				default:
					return HandTrackingType.None;
			}
		}

		protected override void Awake()
		{
			base.Awake();

			hand = controllerTransform.GetComponentInChildren<OVRHand>();
			skeleton = hand.GetComponent<OVRSkeleton>();

			GetControllerHashes();

			fingerBasis = new BoneBasis() { Forward = fingerForward, Up = fingerUp };
		}

		void GetControllerHashes()
		{
			xAxisHash = Animator.StringToHash("xAxis");
			yAxisHash = Animator.StringToHash("yAxis");
			gripHash = Animator.StringToHash("Grip");
			thumbHash = Animator.StringToHash("Thumb");
			indexGripHash = Animator.StringToHash("IndexGrip");
			pinchHash = Animator.StringToHash("Pinching");
		}

		private void Start()
		{
			handAnimator.runtimeAnimatorController = animatorController;
			handAnimator.enabled = true;

			ApplySpecificControllerOffset(ovrTouchOffset, controllerHand.Wrist);

			// get our skeleton2
			FieldInfo[] privateFields = typeof(OVRSkeleton).GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			skeletonField = privateFields.First<FieldInfo>(item => item.Name == "_skeleton");
			rawSkeleton = (OVRPlugin.Skeleton2)skeletonField.GetValue(skeleton);
		}

		private void ApplySpecificControllerOffset(ControllerOffset offset, Transform offsetTransform)
		{
			bool isLeft = controllerHand.IsLeft;

			offsetTransform.localPosition = localPosOffset;
			offsetTransform.localRotation = Quaternion.Euler(localRotOffset);
		}

		// Update is called once per frame
		void Update()
		{
			// move our position
			transform.SetPositionAndRotation(controllerTransform.position,
				controllerTransform.rotation);

			OVRInput.Controller controllerType = OVRInput.GetActiveController();

			switch (controllerType)
			{
				// lolwat cases
				case OVRInput.Controller.Active:
				case OVRInput.Controller.All:
					break;

				// touch cases
				case OVRInput.Controller.LTouch:
				case OVRInput.Controller.RTouch:
				case OVRInput.Controller.Touch:
					ApplySpecificControllerOffset(ovrTouchOffset, controllerHand.Wrist.parent);
					ProcessOVRTouchInput();
					break;

				// hands cases
				case OVRInput.Controller.Hands:
				case OVRInput.Controller.LHand:
				case OVRInput.Controller.RHand:
					DoSkeletalTracking();
					break;

				// do nothing cases
				case OVRInput.Controller.None:
				case OVRInput.Controller.Remote:
				case OVRInput.Controller.Gamepad:
				default:
					break;
			}

			if(drawDebugMesh)
			{
				debugMesh.gameObject.SetActive(true);
			}
			else if(debugMesh != null) debugMesh.gameObject.SetActive(false);
		}

		void MatchBone(Transform oculusBone, Transform leapBone, BoneBasis basis,
			Quaternion leapOrientation)
		{
			controllerHand.SetTransformWithConstraint(leapBone,
			oculusBone.transform.position,
			GlobalRotationFromBasis(oculusBone, basis) * leapOrientation);
		}

		BoneBasis fingerBasis;

		void DoSkeletalTracking()
		{
			rawSkeleton = (OVRPlugin.Skeleton2)skeletonField.GetValue(skeleton);

			// do any pre-flight checks here
			// confidence maybe?
			if (true)
			{
				controllerHand.Confidence = skeleton.IsDataHighConfidence ? 1 : 0;
				handAnimator.enabled = true;

				// do our wrist pose
				OVRBone wristBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot];

				Vector3 wristPos = wristBone.Transform.position;
				Quaternion wristboneOrientation = controllerHand.GetLocalBasis();
				controllerHand.Wrist.rotation = GlobalRotationFromBasis(wristBone.Transform, fingerBasis) * wristboneOrientation;

				// do our fingers. Skip metacarpals, Oculus does not provide them.
				// we could possibly compute the missing bone rotation, if needed.
				// do the index bones
				OVRBone indexProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index1];
				OVRBone indexMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index2];
				OVRBone indexDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index3];
				OVRBone indexDistalTip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_IndexTip];
				Transform indexProximal = controllerHand.IndexMetacarpal.GetChild(0);
				Transform indexMedial = indexProximal.GetChild(0);
				Transform indexDistal = indexMedial.GetChild(0);
				Transform indexTip = indexDistal.GetChild(0);

				controllerHand.FingerWidth[(int)Leap.Finger.FingerType.TYPE_INDEX] = rawSkeleton.BoneCapsules[(int)OVRSkeleton.BoneId.Hand_Index1].Radius * 0.5f;
				MatchBone(indexProximalBone.Transform, indexProximal, fingerBasis, wristboneOrientation);
				MatchBone(indexMedialBone.Transform, indexMedial, fingerBasis, wristboneOrientation);
				MatchBone(indexDistalBone.Transform, indexDistal, fingerBasis, wristboneOrientation);
				Vector3 indexTipLocal = indexDistal.InverseTransformPoint(indexDistalTip.Transform.position);				

				// do the middle bones
				OVRBone middleProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle1];
				OVRBone middleMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle2];
				OVRBone middleDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle3];
				OVRBone middleDistalTip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_MiddleTip];
				Transform middleProximal = controllerHand.MiddleMetacarpal.GetChild(0);
				Transform middleMedial = middleProximal.GetChild(0);
				Transform middleDistal = middleMedial.GetChild(0);
				Transform middleTip = middleDistal.GetChild(0);

				controllerHand.FingerWidth[(int)Leap.Finger.FingerType.TYPE_MIDDLE] = rawSkeleton.BoneCapsules[(int)OVRSkeleton.BoneId.Hand_Middle1].Radius * 0.5f;
				MatchBone(middleProximalBone.Transform, middleProximal, fingerBasis, wristboneOrientation);
				MatchBone(middleMedialBone.Transform, middleMedial, fingerBasis, wristboneOrientation);
				MatchBone(middleDistalBone.Transform, middleDistal, fingerBasis, wristboneOrientation);
				Vector3 middleTipLocal = middleDistal.InverseTransformPoint(middleDistalTip.Transform.position);

				// do the ring bones
				OVRBone ringProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring1];
				OVRBone ringMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring2];
				OVRBone ringDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring3];
				OVRBone ringDistalTip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_RingTip];
				Transform ringProximal = controllerHand.RingMetacarpal.GetChild(0);
				Transform ringMedial = ringProximal.GetChild(0);
				Transform ringDistal = ringMedial.GetChild(0);
				Transform ringTip = ringDistal.GetChild(0);

				controllerHand.FingerWidth[(int)Leap.Finger.FingerType.TYPE_RING] = rawSkeleton.BoneCapsules[(int)OVRSkeleton.BoneId.Hand_Ring1].Radius * 0.5f;
				MatchBone(ringProximalBone.Transform, ringProximal, fingerBasis, wristboneOrientation);
				MatchBone(ringMedialBone.Transform, ringMedial, fingerBasis, wristboneOrientation);
				MatchBone(ringDistalBone.Transform, ringDistal, fingerBasis, wristboneOrientation);
				Vector3 ringTipLocal = ringDistal.InverseTransformPoint(ringDistalTip.Transform.position);

				// do the pinky bones
				OVRBone pinkyMetacarpalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky0];
				OVRBone pinkyProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky1];
				OVRBone pinkyMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky2];
				OVRBone pinkyDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky3];
				OVRBone pinkyDistalTip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_PinkyTip];
				Transform pinkyMetacarpal = controllerHand.PinkyMetacarpal;
				Transform pinkyPromial = pinkyMetacarpal.GetChild(0);
				Transform pinkyMedial = pinkyPromial.GetChild(0);
				Transform pinkyDistal = pinkyMedial.GetChild(0);
				Transform pinkyTip = pinkyDistal.GetChild(0);

				controllerHand.FingerWidth[(int)Leap.Finger.FingerType.TYPE_PINKY] = rawSkeleton.BoneCapsules[(int)OVRSkeleton.BoneId.Hand_Pinky1].Radius * 0.5f;
				MatchBone(pinkyMetacarpalBone.Transform, pinkyMetacarpal, fingerBasis, wristboneOrientation);
				MatchBone(pinkyProximalBone.Transform, pinkyPromial, fingerBasis, wristboneOrientation);
				MatchBone(pinkyMedialBone.Transform, pinkyMedial, fingerBasis, wristboneOrientation);
				MatchBone(pinkyDistalBone.Transform, pinkyDistal, fingerBasis, wristboneOrientation);
				Vector3 pinkyLocal = pinkyDistal.InverseTransformPoint(pinkyDistalTip.Transform.position);

				// do the thumb bones
				OVRBone thumbRootBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb0];
				OVRBone thumbMetacarpalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb1];
				OVRBone thumbProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb2];
				OVRBone thumbDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb3];
				OVRBone thumbDistalTip = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_ThumbTip];
				Transform thumbMetacarpal = controllerHand.ThumbMetacarpal; // naming gets weird here, sorry.
				Transform thumbProximal = thumbMetacarpal.GetChild(0);
				Transform thumbDistal = thumbProximal.GetChild(0);
				Transform thumbTip = thumbDistal.GetChild(0);

				controllerHand.FingerWidth[(int)Leap.Finger.FingerType.TYPE_THUMB] = rawSkeleton.BoneCapsules[(int)OVRSkeleton.BoneId.Hand_Thumb0].Radius * 0.5f;
				MatchBone(thumbMetacarpalBone.Transform, thumbMetacarpal, fingerBasis, wristboneOrientation);
				MatchBone(thumbProximalBone.Transform, thumbProximal, fingerBasis, wristboneOrientation);
				MatchBone(thumbDistalBone.Transform, thumbDistal, fingerBasis, wristboneOrientation);
				Vector3 thumbLocal = thumbDistal.InverseTransformPoint(thumbDistalTip.Transform.position);

				// Apply our tip shortening
				Vector3 scaleFactor = new Vector3(
					(fingerForward.x != 0) ? tipScale : 1,
					(fingerForward.y != 0) ? tipScale : 1,
					(fingerForward.z != 0) ? tipScale : 1); // do this once at startup maybe?
				indexTip.transform.localPosition = Vector3.Scale(indexTipLocal, scaleFactor);
				middleTip.localPosition = Vector3.Scale(middleTipLocal, scaleFactor);
				ringTip.transform.localPosition = Vector3.Scale(ringTipLocal, scaleFactor);
				pinkyTip.transform.localPosition = Vector3.Scale(pinkyLocal, scaleFactor);
				thumbTip.localPosition = Vector3.Scale(thumbLocal, scaleFactor * 1.25f);
			}
		}

		bool IsLeft()
		{
			return controllerHand.IsLeft;
		}

		void ProcessOVRTouchInput()
		{
			handAnimator.enabled = true;
			OVRInput.Controller controller = (IsLeft()) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

			triggerTouch = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, controller);
			gripTouch = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller) > 0.01f;

			bool thumbUp = !OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, controller);

			bool pinch = (triggerTouch && !thumbUp) && !gripTouch;
			controllerIsPinching = pinch;

			// get grab pose
			float grabValue = Mathf.Lerp(((triggerTouch) ? grabValueTouchFloor : grabValueNoTouchFloor),
				1f, OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, controller)); // set floor for grippiness, since Touch has no capacitive detection for grip button.

			// do thumb posing
			if (thumbUp)
			{
				thumbValue = Mathf.Lerp(thumbValue, 0, Time.deltaTime * 8);
			}
			else
			{
				if (triggerTouch && gripTouch)
				{
					thumbValue = Mathf.Lerp(thumbValue, grabValue, Time.deltaTime * 8);
				}
				else
				{
					thumbValue = Mathf.Lerp(thumbValue, (controllerIsPinching) ? 1 : thumbValueTouchCeiling, Time.deltaTime * 8);
				}
			}

			if (!pinch)
			{
				indexGripValue = Mathf.Lerp(indexGripValue,
					(triggerTouch) ? Mathf.Clamp(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller), indexTouchFloor, 1) :
					OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller), Time.deltaTime * 8);
			}
			else
			{
				indexGripValue = Mathf.Lerp(indexGripValue, OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller), Time.deltaTime * 8);
			}

			handAnimator.SetFloat(thumbHash, thumbValue);
			handAnimator.SetFloat(gripHash, grabValue);
			handAnimator.SetFloat(indexGripHash, indexGripValue);
			handAnimator.SetBool(pinchHash, pinch);

			handAnimator.SetLayerWeight(0, 1);
			handAnimator.SetLayerWeight(1, 1);
			handAnimator.SetLayerWeight(2, 1);

			poseCanGrip = grabValue > 0;
		}

		void DrawBone(Transform bone)
		{
			if (bone.parent)
			{
				Gizmos.DrawLine(bone.parent.position, bone.position);
			}

			if (drawBasis)
			{
				DrawBasis(bone, new BoneBasis() { Forward = fingerForward, Up = fingerUp });
			}

			for (int i = 0; i < bone.childCount; i++)
			{
				DrawBone(bone.GetChild(i));
			}
		}

		private void OnDrawGizmosSelected()
		{
			if(drawBindPose && hand && skeleton && skeleton.Bones.Count > 0)
			{
				DrawBone(skeleton.BindPoses[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform);
			}

			if(drawSkeleton && hand && skeleton && skeleton.Bones.Count > 0)
			{
				DrawBone(skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform);
			}
		}
	}
}