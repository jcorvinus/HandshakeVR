using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR.Avatar
{
	public class VisibilityGameObject : AvatarVisibility
	{
		[SerializeField] GameObject[] gameObjects;

		public override void SetVisibility(bool visible)
		{
			foreach(GameObject targetObject in gameObjects) targetObject.SetActive(visible);
		}
	}
}