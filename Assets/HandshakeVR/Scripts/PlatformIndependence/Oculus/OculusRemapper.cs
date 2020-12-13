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
					controllerHand.Wrist.transform.localPosition = Vector3.zero;
					controllerHand.Wrist.localRotation = Quaternion.identity;
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

		bool IsLeft()
		{
			return controllerHand.IsLeft;
		}

		void ProcessOVRTouchInput()
		{
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
				DrawBone(skeleton.Bones[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform);
			}

			if(drawSkeleton && hand && skeleton && skeleton.Bones.Count > 0)
			{
				DrawBone(skeleton.BindPoses[(int)OVRSkeleton.BoneId.Hand_WristRoot].Transform);
			}
		}
	}
}