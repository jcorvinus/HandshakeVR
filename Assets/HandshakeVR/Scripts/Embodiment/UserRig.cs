using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class UserRig : MonoBehaviour
	{
		#region Defines
		public enum BodyReference
		{
			Head,

			LeftWrist,
			LeftPalm,
			/*LeftIndexTip,
			LeftMiddleTip,
			LeftRingTip,
			LeftPinkyTip,*/

			RightWrist,
			RightPalm
			/*RightIndexTip,
			RightMiddleTip,
			RightRingTip,
			RightPinkyTip*/
		}
		#endregion

		private static UserRig instance;
		public static UserRig Instance { get { return instance; } }

		PlatformManager platformManager;

		UserHand leftHand;
		UserHand rightHand;
		[SerializeField] Camera viewCamera;
		[SerializeField] Transform combinedEye;
		[SerializeField] Transform leftEye;
		[SerializeField] Transform rightEye;
		Transform leftWrist;
		Transform rightWrist;

		private Vector3 leftShoulder;
		private Vector3 rightShoulder;
		bool userPresence;

		private void Awake()
		{
			instance = this;

			UserHand[] hands = GetComponentsInChildren<UserHand>(true);

			leftHand = hands.First<UserHand>(item => item.IsLeft);
			rightHand = hands.First<UserHand>(item => !item.IsLeft);

			GetHandPoints();
		}

		void GetHandPoints()
		{
			// we're going to do the left and right wrists as attachment hand points
			Leap.Unity.Attachments.AttachmentHands attachmentHands = GetComponentInChildren<Leap.Unity.Attachments.AttachmentHands>();
			Leap.Unity.Attachments.AttachmentHand leftAttachHand = attachmentHands.attachmentHands.First(item => item.chirality == Leap.Unity.Chirality.Left);
			Leap.Unity.Attachments.AttachmentHand rightAttachHand = attachmentHands.attachmentHands.First(item => item.chirality == Leap.Unity.Chirality.Right);

			Leap.Unity.Attachments.AttachmentPointBehaviour[] leftPoints = leftAttachHand.GetComponentsInChildren<Leap.Unity.Attachments.AttachmentPointBehaviour>(true);
			Leap.Unity.Attachments.AttachmentPointBehaviour[] rightPoints = rightAttachHand.GetComponentsInChildren<Leap.Unity.Attachments.AttachmentPointBehaviour>(true);

			leftWrist = leftPoints.FirstOrDefault(item => item.attachmentPoint == Leap.Unity.Attachments.AttachmentPointFlags.Wrist).transform;
			rightWrist = rightPoints.FirstOrDefault(item => item.attachmentPoint == Leap.Unity.Attachments.AttachmentPointFlags.Wrist).transform;
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		private void Update()
		{
			UpdateUserPresence();
			UpdateBodyEstimation();

#if SteamVR
			if (!debugDisableEyeTracking)
			{
				// vive anipal 
				bool isProEye = ViveSR.anipal.Eye.SRanipal_Eye_API.IsViveProEye();

				useEyeTracking = isProEye && (ViveSR.anipal.Eye.SRanipal_Eye_Framework.Status ==
					ViveSR.anipal.Eye.SRanipal_Eye_Framework.FrameworkStatus.WORKING);
			}
			else useEyeTracking = false;
#endif
		}

		#region Body Estimation
		Quaternion GetShoulderBasis(Vector3 headForward)
		{
			return Quaternion.LookRotation(Vector3.ProjectOnPlane(headForward, Vector3.up),
				Vector3.up);
		}

		Vector3 GetShoulderPoint(Vector3 headPos, Quaternion shoulderBasis, bool isLeft)
		{
			return headPos
				+ (shoulderBasis * (new Vector3(0f, -0.2f, -0.1f)
				+ Vector3.left * 0.1f * (isLeft ? 1f : -1f)));
		}

		void UpdateBodyEstimation()
		{
			Quaternion shoulderBasis = GetShoulderBasis(viewCamera.transform.forward);

			leftShoulder = GetShoulderPoint(viewCamera.transform.position, shoulderBasis, true);
			rightShoulder = GetShoulderPoint(viewCamera.transform.position, shoulderBasis, false);
		}

		void UpdateUserPresence()
		{
			switch (platformManager.Platform)
			{
				case PlatformID.None:
					break;
				case PlatformID.Oculus:
					userPresence = OVRManager.isHmdPresent;
					break;
				case PlatformID.SteamVR:
					SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("headsetonhead");
					userPresence = headsetOnHead.GetState(SteamVR_Input_Sources.Head);
					break;
				default:
					break;
			}
		}
		#endregion

		private void OnDrawGizmos()
		{
			Vector3 headPos = viewCamera.transform.position;

			// draw our left and right virtual shoulder points
			// so we can make sure they're calculated properly
			Quaternion shoulderBasis = GetShoulderBasis(viewCamera.transform.forward);
			Vector3 leftShoulder = GetShoulderPoint(headPos, shoulderBasis, true);
			Vector3 rightShoulder = GetShoulderPoint(headPos, shoulderBasis, false);

			Gizmos.DrawLine(headPos, leftShoulder);
			Gizmos.DrawLine(headPos, rightShoulder);
			Gizmos.DrawSphere(leftShoulder, 0.01f);
			Gizmos.DrawSphere(rightShoulder, 0.01f);
		}

	}
}