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
			public GameObject LeftPlatformHandVisual;
			public GameObject RightPlatformHandVisual;
		}

		// reference our current platform info asset here
		[SerializeField] PlatformInfo currentPlatform;
		[SerializeField] PlatformReferences[] perPlatformData;

		PlatformReferences currentPlatformReferences;
		public PlatformID Platform { get { return currentPlatform.PlatformID; } }

		private void Awake()
		{
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

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}