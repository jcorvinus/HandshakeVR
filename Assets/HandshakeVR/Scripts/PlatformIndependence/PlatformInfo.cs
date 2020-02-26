using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	[CreateAssetMenu(fileName = "PlatformInfo", menuName = "Handshake/PlatformInfo")]
	public class PlatformInfo : ScriptableObject
	{
		[SerializeField] PlatformID platformID;
		[SerializeField] bool useHandshakeMultiplatform;

		public const string HANDSHAKE_NONE = "HANDSHAKE_NONE";
		public const string HANDSHAKE_STEAMVR = "HANDSHAKE_STEAMVR";
		public const string HANDSHAKE_OCULUS = "HANDSHAKE_OCULUS";

		public PlatformID PlatformID { get { return platformID; } }
		public bool UseHandshakeMultiplatform { get { return useHandshakeMultiplatform; } }
	}
}