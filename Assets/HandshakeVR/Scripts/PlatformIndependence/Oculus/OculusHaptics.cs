using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public class OculusHaptics : ControllerHaptics
	{
		public override void DoHaptics(float frequency, float amplitude, float duration)
		{
			OVRInput.Controller controllerMask = IsLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
			OVRInput.SetControllerVibration(frequency, amplitude, controllerMask);
		}
	}
}