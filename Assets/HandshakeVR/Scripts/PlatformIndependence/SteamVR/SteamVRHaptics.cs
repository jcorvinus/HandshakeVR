using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class SteamVRHaptics : ControllerHaptics
	{
		[SerializeField] SteamVR_Action_Vibration vibration;

		public override void DoHaptics(float frequency, float amplitude, float duration)
		{
			SteamVR_Input_Sources inputSource = IsLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;

			vibration.Execute(0, duration, frequency, amplitude, inputSource);
		}
	}
}