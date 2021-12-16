using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

namespace HandshakeVR
{
	// Just storing some hand-relevant definitions common to all sorts of UI stuff.
	// note: fingers point 'forward' on the z axis, the red axis points inwards on the hand (from index to thumb) and the green axis points away from the palm)

	public enum HandFilter { Left = 0, Right = 1, Either = 2, Both = 3, None = 4 }
	[System.Flags]
	public enum FingerFilter { none = 0, thumb = 0x1, index = 0x2, middle = 0x4, ring = 0x8, pinky = 0x0f }
	public enum HandLocation { finger, wrist, forearm, palm }

	/// <summary>
	/// Stores relevant information about a fingertip.
	/// </summary>
	public struct FingertipData
	{
		public CapsuleCollider Owner;
		public bool HandLeft { get { return HandModel.IsLeft; } }
		public FingerFilter finger;
		public Vector3 TipPosition { get { return Owner.transform.TransformPoint(Owner.center) + Forward * Owner.height; } }
		public Vector3 Forward { get { return (Owner.transform.forward * ((HandLeft) ? 1 : 1)); } }

		public UserHand HandModel;
	}

	public class UserHand : MonoBehaviour
	{
		public System.Action<UserHand> OnTrackingGained;
		public System.Action<UserHand> OnTrackingLost;
		public System.Action<UserHand, HandTrackingType, HandTrackingType> OnTrackingTypeChanged;

		bool previousWasTracking = false;
		HandTrackingType previousTrackingType = HandTrackingType.Skeletal;
		UserRig userRig;
		SkeletalControllerHand skeletalControllerHand;
		DataHand dataHand;
		RiggedHand riggedhand;
		Renderer riggedHandRenderer;
		RigidHand rigidHand;
		OverridableHandEnableDisable riggedEnabler, rigidEnabler;

		private bool handEnabled = true;
		bool disableUINonIndexFingertips = false;
		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }
		public bool IsTracked { get { return dataHand.IsTracked; } }
		public HandInputProvider ActiveInputProvider { get { return skeletalControllerHand.ActiveProvider; } }
		public bool DisableUINonIndexFingertips { get { return disableUINonIndexFingertips; } }

		// hands can be enabled or disabled so that you can replace them with embodied tools.
		// disabling a hand will not disable the Data Hand underneath it - this is so that you can
		// have your embodied tool continue to track the hand position, and use finger data to activate
		// and otherwise drive said tools.
		public bool HandEnabled
		{
			get { return handEnabled; }
			set
			{
				handEnabled = value;

				// do all of our enabling/disabling of the various sub-components
				rigidEnabler.IsDisabled = !value;
				riggedEnabler.IsDisabled = !value;

				PlatformControllerManager controllerManager = userRig.ProviderSwitcher.ControllerManager;

				if (controllerManager) controllerManager.SetInteractionEnable(value, isLeft);
			}
		}

		public DataHand DataHand { get { return dataHand; } }

		private void Awake()
		{
			userRig = GetComponentInParent<UserRig>();
			skeletalControllerHand = GetComponent<SkeletalControllerHand>();

			ProviderSwitcher providerSwitcher = userRig.ProviderSwitcher;

			// get all of our handed stuff
			HandModelManager modelManager = userRig.GetComponentInChildren<HandModelManager>();
			DataHand[] dataHands = modelManager.GetComponentsInChildren<DataHand>(true);
			Chirality chirality = (IsLeft) ? Chirality.Left : Chirality.Right;
			dataHand = dataHands.First(item => item is DataHand && item.Handedness == chirality);

			RiggedHand[] riggedHands = modelManager.GetComponentsInChildren<RiggedHand>(true);
			riggedhand = riggedHands.First(item => item is RiggedHand && item.Handedness == chirality);
			riggedHandRenderer = riggedhand.GetComponentsInChildren<Renderer>(true)[0];

			RigidHand[] rigidHands = modelManager.GetComponentsInChildren<RigidHand>(true);
			rigidHand = rigidHands.First(item => item is RigidHand && item.Handedness == chirality);

			riggedEnabler = riggedhand.GetComponent<OverridableHandEnableDisable>();
			rigidEnabler = rigidHand.GetComponent<OverridableHandEnableDisable>();
		}

		void Update()
		{
			if(dataHand.IsTracked != previousWasTracking) // we have a tracking event change
			{
				if(dataHand.IsTracked)
				{
					if (OnTrackingGained != null) OnTrackingGained(this);
				}
				else
				{
					if (OnTrackingLost != null) OnTrackingLost(this);
				}
				previousWasTracking = dataHand.IsTracked;
			}

			HandTrackingType currentTrackingType = ActiveInputProvider.TrackingType();
			if (currentTrackingType != previousTrackingType) // we have a tracking type change
			{
				if (OnTrackingTypeChanged != null) OnTrackingTypeChanged(this, previousTrackingType, currentTrackingType);
				previousTrackingType = currentTrackingType;
			}
		}
	}
}