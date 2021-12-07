using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public class UserHand : MonoBehaviour
	{
		HandInputProvider handInputProvider;
		SkeletalControllerHand skeletalControllerHand;

		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }

		private void Awake()
		{
			handInputProvider = GetComponent<HandInputProvider>();
			skeletalControllerHand = GetComponent<SkeletalControllerHand>();
		}

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}