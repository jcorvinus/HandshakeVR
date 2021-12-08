using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

namespace HandshakeVR
{
	public class HandDisabler : MonoBehaviour
	{
		[SerializeField] UserRig userRig;

		[ExposeMethodInEditor]
		void DisableLeftHand()
		{
			userRig.LeftHand.HandEnabled = false;
		}

		[ExposeMethodInEditor]
		void DisableRightHand()
		{
			userRig.RightHand.HandEnabled = false;
		}

		[ExposeMethodInEditor]
		void EnableLeftHand()
		{
			userRig.LeftHand.HandEnabled = true;
		}

		[ExposeMethodInEditor]
		void EnableRightHand()
		{
			userRig.RightHand.HandEnabled = true;
		}
	}
}