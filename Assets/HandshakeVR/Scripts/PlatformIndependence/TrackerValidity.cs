using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public abstract class TrackerValidity : MonoBehaviour
	{
		[SerializeField] bool isLeft;

		protected bool isValid;
		public bool IsValid { get { return isValid; } }
		public bool IsLeft { get { return isLeft; } }
	}
}