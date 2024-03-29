﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if XR_PLUGIN_MANAGEMENT
using UnityEngine.SpatialTracking;
#endif

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
	[DefaultExecutionOrder(-40)]
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
		ProviderSwitcher providerSwitcher;

		UserHand leftHand;
		UserHand rightHand;
		[SerializeField] Camera viewCamera;
		[SerializeField] Transform combinedEye;
		[SerializeField] Transform leftEye;
		[SerializeField] Transform rightEye;
		Transform leftWrist;
		Transform leftPalm;
		Transform rightWrist;
		Transform rightPalm;

#if XR_PLUGIN_MANAGEMENT
		TrackedPoseDriver viewCameraDriver;
#endif

		public ProviderSwitcher ProviderSwitcher { get { return providerSwitcher; } }
		public PlatformManager PlatformManager { get { return platformManager; } }
		public Camera ViewCamera { get { return viewCamera; } }
		public UserHand LeftHand { get { return leftHand; } }
		public UserHand RightHand { get { return rightHand; } }
		public Transform CombinedEye { get { return combinedEye; } }
		public bool UserPresence { get { return userPresence; } }

		private Vector3 leftShoulder;
		private Vector3 rightShoulder;
		bool userPresence;

		private void Awake()
		{
			instance = this;

#if XR_PLUGIN_MANAGEMENT
			viewCameraDriver = viewCamera.transform.GetComponent<TrackedPoseDriver>();
			viewCameraDriver.enabled = true;
#endif

			UserHand[] hands = GetComponentsInChildren<UserHand>(true);

			leftHand = hands.First<UserHand>(item => item.IsLeft);
			rightHand = hands.First<UserHand>(item => !item.IsLeft);

			GetHandPoints();

			providerSwitcher = GetComponentInChildren<ProviderSwitcher>();
			platformManager = providerSwitcher.GetComponent<PlatformManager>();
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
			leftPalm = leftPoints.FirstOrDefault(item => item.attachmentPoint == Leap.Unity.Attachments.AttachmentPointFlags.Palm).transform;

			rightWrist = rightPoints.FirstOrDefault(item => item.attachmentPoint == Leap.Unity.Attachments.AttachmentPointFlags.Wrist).transform;
			rightPalm = rightPoints.FirstOrDefault(item => item.attachmentPoint == Leap.Unity.Attachments.AttachmentPointFlags.Palm).transform;
		}

		private void Update()
		{
			UpdateUserPresence();
			UpdateBodyEstimation();
		}

		/// <summary>
		/// Will let you retrieve a reference to a known tracked body part.
		/// </summary>
		/// <param name="bodyRef"></param>
		/// <returns></returns>
		public Transform GetTransformForBone(BodyReference bodyRef)
		{
			switch (bodyRef)
			{
				case BodyReference.Head:
					return viewCamera.transform;

				case BodyReference.LeftPalm:
					return leftPalm;

				case BodyReference.LeftWrist:
					return leftWrist;

				/*case BodyReference.LeftIndexTip:
					break;

				case BodyReference.LeftMiddleTip:
					break;

				case BodyReference.LeftRingTip:
					break;

				case BodyReference.LeftPinkyTip:
					break;*/

				case BodyReference.RightPalm:
					return rightPalm;

				case BodyReference.RightWrist:
					return rightWrist;

				/*case BodyReference.RightIndexTip:
					break;

				case BodyReference.RightMiddleTip:
					break;

				case BodyReference.RightRingTip:
					break;

				case BodyReference.RightPinkyTip:
					break;*/

				default:
					break;
			}

			return null;
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
#if UNITY_STANDALONE
					SteamVR_Action_Boolean headsetOnHead = SteamVR_Input.GetBooleanAction("headsetonhead");
					userPresence = headsetOnHead.GetState(SteamVR_Input_Sources.Head);
#endif
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