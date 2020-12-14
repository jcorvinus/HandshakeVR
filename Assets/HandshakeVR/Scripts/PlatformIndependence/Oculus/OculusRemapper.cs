using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Oculus;

namespace HandshakeVR
{
	public class OculusRemapper : MonoBehaviour
	{
		[System.Serializable]
		public struct ControllerOffset
		{
			[SerializeField] public Vector3 LocalPositionLeft;
			[SerializeField] public Vector3 LocalRotationLeft;

			[SerializeField] public Vector3 LocalPositionRight;
			[SerializeField] public Vector3 LocalRotationRight;
		}

		[SerializeField] Transform controllerTransform;
		SkeletalControllerHand controllerHand;
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

		Animator handAnimator;
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

		[Header("Debug Vars")]
		[SerializeField] bool drawDebugMesh;
		[SerializeField] GameObject debugMesh;

		[SerializeField] bool drawSkeleton;
		[SerializeField] bool drawBindPose;
		[SerializeField] bool drawBasis;

		private void Awake()
		{
			controllerHand = GetComponent<SkeletalControllerHand>();
			handAnimator = GetComponent<Animator>();

			hand = controllerTransform.GetComponentInChildren<OVRHand>();
			skeleton = hand.GetComponent<OVRSkeleton>();

			GetControllerHashes();
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
					/*controllerHand.Wrist.transform.localPosition = Vector3.zero;
					controllerHand.Wrist.localRotation = Quaternion.identity;*/
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

		void DoSkeletalTracking()
		{
			// do any pre-flight checks here
			// confidence maybe?
			if(true)
			{
				handAnimator.enabled = true;
				BoneBasis basis = new BoneBasis() { Forward = fingerForward, Up = fingerUp };

				// do our wrist pose
				OVRBone wristBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot];

				Vector3 wristPos = wristBone.Transform.position;
				Quaternion wristboneOrientation = controllerHand.GetLocalBasis();

				/*controllerHand.Wrist.SetPositionAndRotation(wristBone.Transform.position,
					GlobalRotationFromBasis(wristBone.Transform, basis) * wristboneOrientation);*/
					controllerHand.Wrist.rotation = GlobalRotationFromBasis(wristBone.Transform, basis) * wristboneOrientation;

				// do our fingers. Skip metacarpals, Oculus does not provide them.
				// we could possibly compute the missing bone rotation, if needed.
				// do the index bones
				OVRBone indexProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index1];
				OVRBone indexMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index2];
				OVRBone indexDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Index3];
				Transform indexProximal = controllerHand.IndexMetacarpal.GetChild(0);
				Transform indexMedial = indexProximal.GetChild(0);
				Transform indexDistal = indexMedial.GetChild(0);

				MatchBone(indexProximalBone.Transform, indexProximal, basis, wristboneOrientation);
				MatchBone(indexMedialBone.Transform, indexMedial, basis, wristboneOrientation);
				MatchBone(indexDistalBone.Transform, indexDistal, basis, wristboneOrientation);

				// do the middle bones
				OVRBone middleProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle1];
				OVRBone middleMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle2];
				OVRBone middleDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Middle3];
				Transform middleProximal = controllerHand.MiddleMetacarpal.GetChild(0);
				Transform middleMedial = middleProximal.GetChild(0);
				Transform middleDistal = middleMedial.GetChild(0);

				MatchBone(middleProximalBone.Transform, middleProximal, basis, wristboneOrientation);
				MatchBone(middleMedialBone.Transform, middleMedial, basis, wristboneOrientation);
				MatchBone(middleDistalBone.Transform, middleDistal, basis, wristboneOrientation);

				// do the ring bones
				OVRBone ringProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring1];
				OVRBone ringMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring2];
				OVRBone ringDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Ring3];
				Transform ringProximal = controllerHand.RingMetacarpal.GetChild(0);
				Transform ringMedial = ringProximal.GetChild(0);
				Transform ringDistal = ringMedial.GetChild(0);

				MatchBone(ringProximalBone.Transform, ringProximal, basis, wristboneOrientation);
				MatchBone(ringMedialBone.Transform, ringMedial, basis, wristboneOrientation);
				MatchBone(ringDistalBone.Transform, ringDistal, basis, wristboneOrientation);

				// do the pinky bones
				OVRBone pinkyMetacarpalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky0];
				OVRBone pinkyProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky1];
				OVRBone pinkyMedialBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky2];
				OVRBone pinkyDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Pinky2];
				Transform pinkyMetacarpal = controllerHand.PinkyMetacarpal;
				Transform pinkyPromial = pinkyMetacarpal.GetChild(0);
				Transform pinkyMedial = pinkyPromial.GetChild(0);
				Transform pinkyDistal = pinkyMedial.GetChild(0);

				MatchBone(pinkyMetacarpalBone.Transform, pinkyMetacarpal, basis, wristboneOrientation);
				MatchBone(pinkyProximalBone.Transform, pinkyPromial, basis, wristboneOrientation);
				MatchBone(pinkyMedialBone.Transform, pinkyMedial, basis, wristboneOrientation);
				MatchBone(pinkyDistalBone.Transform, pinkyDistal, basis, wristboneOrientation);

				// do the thumb bones
				OVRBone thumbRootBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb0];
				OVRBone thumbMetacarpalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb1];
				OVRBone thumbProximalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb2];
				OVRBone thumbDistalBone = skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_Thumb3];
				Transform thumbMetacarpal = controllerHand.ThumbMetacarpal; // naming gets weird here, sorry.
				Transform thumbProximal = thumbMetacarpal.GetChild(0);
				Transform thumbDistal = thumbProximal.GetChild(0);

				MatchBone(thumbMetacarpalBone.Transform, thumbMetacarpal, basis, wristboneOrientation);
				MatchBone(thumbProximalBone.Transform, thumbProximal, basis, wristboneOrientation);
				MatchBone(thumbDistalBone.Transform, thumbDistal, basis, wristboneOrientation);
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

			/*if (OVRInput.GetUp(OVRInput.Button.Two, controller))
			{
				DispatchSwitchEvent();
			}

			if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller)) DispatchToolActivateEvent();
			else if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controller)) DispatchToolDeactivateEvent();*/
		}

		Quaternion GlobalRotationFromBasis(Transform bone, BoneBasis basis)
		{
			return Quaternion.LookRotation(bone.TransformDirection(basis.Forward),
				bone.TransformDirection(basis.Up));
		}

		private void DrawBasis(Transform bone, BoneBasis basis)
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