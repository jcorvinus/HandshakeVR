using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
	public class SteamVRTrackerValidity : TrackerValidity
	{
		[SerializeField] string agnosticActionPath="/actions/default/in/Pose";
#if UNITY_STANDALONE
		SteamVR_Behaviour_Pose pose;

		private void Awake()
		{
			pose = GetComponent<SteamVR_Behaviour_Pose>();
			pose.poseAction = SteamVR_Input.GetPoseActionFromPath(agnosticActionPath);
		}

		private void Update()
		{
			isValid = pose.isValid;
		}
#endif
	}
}