using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
	public class BreakAction : MonoBehaviour
	{
		[SerializeField] string breakActionName= "actions/default/in/Break";
#if UNITY_STANDALONE
		SteamVR_Action_Boolean breakAction;
#endif

		private void Awake()
		{
#if UNITY_STANDALONE
			breakAction = SteamVR_Input.GetBooleanAction(breakActionName);
			breakAction.onStateDown += (SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) => { Debug.Break(); };
#endif
		}
	}
}