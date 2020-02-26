using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
	public class BreakAction : MonoBehaviour
	{
		[SerializeField] SteamVR_Action_Boolean breakAction;

		private void Awake()
		{
			breakAction.onStateDown += (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) => { Debug.Break(); };
		}
	}
}