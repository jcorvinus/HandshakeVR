﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if XR_PLUGIN_MANAGEMENT
using UnityEngine.XR.Management;
#endif

namespace HandshakeVR
{
	public enum PlatformID { None = 0, SteamVR = 1, Oculus = 2, PicoXR = 3 }

	[DefaultExecutionOrder(-30)]
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
		}

		private void Start()
		{
#if XR_PLUGIN_MANAGEMENT
			string deviceName = ""; //UnityEngine.XR.XRSettings.loadedDeviceName;
			XRGeneralSettings xrSettings = XRGeneralSettings.Instance;
			XRLoader activeLoader = xrSettings.Manager.activeLoader;

			deviceName = activeLoader.GetType().ToString();
			string[] deviceSplit = deviceName.Split('.');
			deviceName = deviceSplit[deviceSplit.Length - 1];

			Debug.Log(deviceName);

			switch (deviceName)
			{
				case ("OculusLoader"):
					currentPlatformID = PlatformID.Oculus;
					break;

				case ("OpenVRLoader"):
					currentPlatformID = PlatformID.SteamVR;
					break;

				case ("PXR_Loader"):
					currentPlatformID = PlatformID.PicoXR;
					break;

				default:
					currentPlatformID = PlatformID.None;
					break;
			}
#else
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
#endif

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

			if(haptics != null && 
				GetControllerTrackingValidity(isLeftController)) haptics.DoHaptics(frequency, amplitude, duration);
		}
	}
}