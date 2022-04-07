using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Valve.VR;
#endif

public class SteamVRActionSetEnable : MonoBehaviour
{
#if UNITY_STANDALONE
	[SerializeField] SteamVR_ActionSet defaultActionSet;
	[SerializeField] SteamVR_ActionSet handAssistActionSet;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	[CatchCo.ExposeMethodInEditor]
	void EnableActionSets()
	{
		defaultActionSet.Activate();
		handAssistActionSet.Activate();
	}
#endif
}
