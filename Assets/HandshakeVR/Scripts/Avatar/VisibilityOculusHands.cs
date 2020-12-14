using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Oculus;

namespace HandshakeVR.Avatar
{
	public class VisibilityOculusHands : AvatarVisibility
	{
		[SerializeField] Renderer skeletalRenderer;
		[SerializeField] OVRMeshRenderer ovrRenderer;

		public override void SetVisibility(bool visible)
		{
			skeletalRenderer.enabled = visible;
			ovrRenderer.enabled = visible;
		}
	}
}