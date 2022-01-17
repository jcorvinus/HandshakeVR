using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
	public class SteamVRHaptics : ControllerHaptics
	{
		[SerializeField] string vibrationActionName = "/actions/default/out/Haptic";
#if UNITY_STANDALONE
		SteamVR_Action_Vibration vibration;
#endif

		private void Awake()
		{
#if UNITY_STANDALONE
			vibration = SteamVR_Input.GetVibrationAction(vibrationActionName);
#endif
		}

		public override void DoHaptics(float frequency, float amplitude, float duration)
		{
#if UNITY_STANDALONE
			SteamVR_Input_Sources inputSource = IsLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

			vibration.Execute(0, duration, frequency, amplitude, inputSource);
#endif
		}
	}
}