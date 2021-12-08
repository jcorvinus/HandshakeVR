using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

namespace HandshakeVR
{
	public class UserHand : MonoBehaviour
	{
		UserRig userRig;
		SkeletalControllerHand skeletalControllerHand;
		DataHand dataHand;
		RiggedHand riggedhand;
		RigidHand rigidHand;
		OverridableHandEnableDisable riggedEnabler, rigidEnabler;

		private bool handEnabled = true;
		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }

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

				bool graphicsEnabled = userRig.ProviderSwitcher.IsDefault || 
					userRig.PlatformManager.HideLeapHandsOnSwitch();

				if(IsLeft)
				{
					PlatformManager.Instance.SetPlatformVisualLeftHandEnable(!graphicsEnabled &&
						handEnabled);
				}
				else
				{
					PlatformManager.Instance.SetPlatformVisualRightHandEnable(!graphicsEnabled &&
						handEnabled);
				}
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

			RigidHand[] rigidHands = modelManager.GetComponentsInChildren<RigidHand>(true);
			rigidHand = rigidHands.First(item => item is RigidHand && item.Handedness == chirality);

			riggedEnabler = riggedhand.GetComponent<OverridableHandEnableDisable>();
			rigidEnabler = rigidHand.GetComponent<OverridableHandEnableDisable>();
		}
	}
}