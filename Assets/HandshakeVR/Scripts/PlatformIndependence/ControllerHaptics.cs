using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public abstract class ControllerHaptics : MonoBehaviour
	{
		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }
		public abstract void DoHaptics(float frequency, float amplitude, float duration);
	}
}