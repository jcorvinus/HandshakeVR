using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

namespace HandshakeVR
{
	public class HandDisabler : MonoBehaviour
	{
		[SerializeField] ProviderSwitcher switcher;

		[ExposeMethodInEditor]
		void DisableLeftHand()
		{
			switcher.LeftHandEnabled = false;
		}

		[ExposeMethodInEditor]
		void DisableRightHand()
		{
			switcher.RightHandEnabled = false;
		}

		[ExposeMethodInEditor]
		void EnableLeftHand()
		{
			switcher.LeftHandEnabled = true;
		}

		[ExposeMethodInEditor]
		void EnableRightHand()
		{
			switcher.RightHandEnabled = true;
		}
	}
}