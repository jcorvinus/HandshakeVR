using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class SteamVRTrackerValidity : TrackerValidity
	{
		SteamVR_Behaviour_Pose pose;

		private void Awake()
		{
			pose = GetComponent<SteamVR_Behaviour_Pose>();
		}

		private void Update()
		{
			isValid = pose.isValid;
		}
	}
}