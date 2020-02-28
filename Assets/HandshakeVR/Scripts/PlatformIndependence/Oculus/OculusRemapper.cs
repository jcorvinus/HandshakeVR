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

		[Range(0, 1)] [SerializeField] float grabValueNoTouchFloor = 0.4f;
		[Range(0, 1)] [SerializeField] float grabValueTouchFloor = 0.56f;
		[Range(0, 1)] [SerializeField] float thumbValueTouchCeiling = 0.75f;
		[Range(0, 1)] [SerializeField] float indexTouchFloor = 0.35f;

		[Header("Debug Vars")]
		[SerializeField] bool drawDebugMesh;
		[SerializeField] GameObject debugMesh;
		[SerializeField] bool applyOffsetEveryFrame;

		private void Awake()
		{
			controllerHand = GetComponent<SkeletalControllerHand>();
			handAnimator = GetComponent<Animator>();

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
			if(applyOffsetEveryFrame) ApplySpecificControllerOffset(ovrTouchOffset, controllerHand.Wrist.parent);

			ProcessOVRTouchInput();

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
	}
}