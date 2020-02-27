using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

namespace HandshakeVR
{
	public class GenericControllerValidity : TrackerValidity
	{
		[SerializeField] XRNode trackingNode;

		List<XRNodeState> states = new List<XRNodeState>();

		bool StatesContainsNode(out int foundID)
		{
			for(int i=0; i < states.Count; i++)
			{
				if (states[i].nodeType == trackingNode)
				{
					foundID = i;
					return true;
				}
			}

			foundID = -1;
			return false;
		}

		private void Update()
		{
			InputTracking.GetNodeStates(states);

			int nodeID = -1;
			// find our node
			if (!StatesContainsNode(out nodeID)) isValid = false;
			else
			{
				isValid = states[nodeID].tracked;
			}
		}
	}
}