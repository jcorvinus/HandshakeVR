using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public enum PlatformID { None = 0, SteamVR = 1, Oculus = 2 }

	public class PlatformManager : MonoBehaviour
	{
		// this stuff might get handled by the ProviderSwitcher instead
		// although the [SteamVR] and OVRManager objects would do well here.
		// shit how do we handle the case of someone who doesn't even want OVR installed?
		[System.Serializable]
		public struct PlatformReferences
		{
			public PlatformID ID;
			public GameObject[] ObjectsToEnable;
			public GameObject[] ObjectsToDisable;
			public MonoBehaviour[] BehavioursToEnable;
			public MonoBehaviour[] BehavioursToDisable;
			public TrackerValidity[] TrackerValidity;

			[Header("Visuals")]
			[Tooltip("Use this to show the platform's hand model, instead of the Leap one.")]
			public bool HideLeapHandsOnSwitch;
			public Avatar.AvatarVisibility LeftPlatformHandVisual;
			public Avatar.AvatarVisibility RightPlatformHandVisual;

			[Header("Haptics")]
			public ControllerHaptics[] Haptics;
		}

		// reference our current platform info asset here
		//[SerializeField] PlatformInfo currentPlatform;
		PlatformID currentPlatformID = PlatformID.None;
		[SerializeField] PlatformReferences[] perPlatformData;

		PlatformReferences currentPlatformReferences;
		public PlatformID Platform { get { return currentPlatformID; } }

		private static PlatformManager instance;
		public static PlatformManager Instance { get { return instance; } }

		private void Awake()
		{
			instance = this;

			string deviceName = UnityEngine.XR.XRSettings.loadedDeviceName;

			Debug.Log(deviceName);

			switch (deviceName)
			{
				case ("Oculus"):
					currentPlatformID = PlatformID.Oculus;
					break;

				case ("OpenVR"):
					currentPlatformID = PlatformID.SteamVR;
					break;

				default:
					currentPlatformID = PlatformID.None;
					break;
			}


			// set up our platform properly.
			// find our platform info
			PlatformReferences platformData = perPlatformData.First(item => item.ID == Platform);
			InitPlatform(platformData);
		}

		void InitPlatform(PlatformReferences platformData)
		{
			// disable behaviours
			if (platformData.BehavioursToDisable != null)
			{
				foreach (Behaviour behaviour in platformData.BehavioursToDisable) behaviour.enabled = false;
			}

			// disable game objects
			if (platformData.ObjectsToDisable != null)
			{
				foreach (GameObject objectToDisable in platformData.ObjectsToDisable) objectToDisable.SetActive(false);
			}

			// enable behaviours
			if (platformData.BehavioursToEnable != null)
			{
				foreach (Behaviour behaviour in platformData.BehavioursToEnable) behaviour.enabled = true;
			}

			// enable game objects (this order was chosen because behaviours to enable might be
			// on disabled game objects, and making sure they're available at Awake() is a good idea)
			if (platformData.ObjectsToEnable != null)
			{
				foreach (GameObject objectToEnable in platformData.ObjectsToEnable) objectToEnable.SetActive(true);
			}

			currentPlatformReferences = platformData;
		}
		
		public bool HideLeapHandsOnSwitch()
		{
			return currentPlatformReferences.HideLeapHandsOnSwitch;
		}

		public void SetPlatformVisualHands(bool leftEnabled, bool rightEnabled)
		{
			if(currentPlatformReferences.LeftPlatformHandVisual) currentPlatformReferences.LeftPlatformHandVisual.SetVisibility(leftEnabled);
			if(currentPlatformReferences.RightPlatformHandVisual) currentPlatformReferences.RightPlatformHandVisual.SetVisibility(rightEnabled);
		}

		public void SetPlatformVisualLeftHandEnable(bool leftEnable)
		{
			if (currentPlatformReferences.LeftPlatformHandVisual) currentPlatformReferences.LeftPlatformHandVisual.SetVisibility(leftEnable);
		}

		public void SetPlatformVisualRightHandEnable(bool rightEnable)
		{
			if (currentPlatformReferences.RightPlatformHandVisual) currentPlatformReferences.RightPlatformHandVisual.SetVisibility(rightEnable);
		}

		public Avatar.AvatarVisibility GetPlatformVisualHand(bool left)
		{
			return (left) ? currentPlatformReferences.LeftPlatformHandVisual : currentPlatformReferences.RightPlatformHandVisual;
		}

		public bool GetControllerTrackingValidity(bool isLeft)
		{
			for(int i=0; i < currentPlatformReferences.TrackerValidity.Length; i++)
			{
				TrackerValidity validity = currentPlatformReferences.TrackerValidity[i];

				if (validity.IsLeft == isLeft)
				{
					return validity.IsValid;
				}
			}

			return false;
		}

		public void DoHapticsForCurrentPlatform(float frequency, float amplitude, float duration,
			bool isLeftController)
		{
			// get our haptics component
			ControllerHaptics haptics=null;

			for(int i=0; i < currentPlatformReferences.Haptics.Length; i++)
			{
				ControllerHaptics currentHaptics = currentPlatformReferences.Haptics[i];
				if(currentHaptics.IsLeft == isLeftController)
				{
					haptics = currentHaptics;
					break;
				}
			}

			if(haptics != null) haptics.DoHaptics(frequency, amplitude, duration);
		}
	}
}