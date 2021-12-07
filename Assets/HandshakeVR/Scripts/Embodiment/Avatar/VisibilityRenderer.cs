using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR.Avatar
{
	public class VisibilityRenderer : AvatarVisibility
	{
		[SerializeField] Renderer[] renderers;

		public override void SetVisibility(bool visible)
		{
			foreach (Renderer targetRenderer in renderers) targetRenderer.enabled = visible;
		}
	}
}