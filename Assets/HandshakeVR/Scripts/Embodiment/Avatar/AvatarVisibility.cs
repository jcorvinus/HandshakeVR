using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR.Avatar
{
	public abstract class AvatarVisibility : MonoBehaviour
	{
		[SerializeField] protected Renderer skeletalRenderer;
		public Renderer SkeletalRenderer { get { return skeletalRenderer; } }
		public abstract void SetVisibility(bool visible);
	}
}