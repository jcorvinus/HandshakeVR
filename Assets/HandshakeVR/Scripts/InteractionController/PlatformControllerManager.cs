using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity.Interaction;

namespace HandshakeVR
{
	public class PlatformControllerManager : MonoBehaviour
	{
		[System.Serializable]
		public struct PlatformInfo
		{
			public PlatformID Platform;
			public InteractionController[] Controllers;			
		}

		[SerializeField] PlatformInfo[] platforms;
		PlatformInfo currentPlatform;

		[Tooltip("Specifies how long to disable the hand's contact ability after a grab. Prevents items from popping out of the user's hand.")]
		[SerializeField] float disableContactAfterGraspTime = 0.25f;
		float leftDisableContactTimer = 0;
		bool leftControllerGrasping;
		bool leftContactEnabled;

		float rightDisableContactTimer = 0;
		bool rightControllerGrasping;
		bool rightContactEnabled;

		InteractionHand leftHand;
		InteractionHand rightHand;

		InteractionController[] leftControllers;
		InteractionController[] rightControllers;

		[SerializeField] private bool controllersEnabled = false;
		public bool ControllersEnabled { get { return controllersEnabled; }
			set
			{
				if(controllersEnabled != value)
				{
					controllersEnabled = value;

					if(controllersEnabled)
					{
						foreach(InteractionController controller in leftControllers)
						{
							controller.OnGraspBegin += SetLeftControllerStates;
							controller.OnGraspEnd += ClearLeftControllerStates;
						}

						foreach (InteractionController controller in rightControllers)
						{
							controller.OnGraspBegin += SetRightControllerStates;
							controller.OnGraspEnd += ClearRightControllerStates;
						}

						leftHand.graspingEnabled = false;
						rightHand.graspingEnabled = false;
					}
					else
					{
						foreach (InteractionController controller in leftControllers)
						{
							controller.OnGraspBegin -= SetLeftControllerStates;
							controller.OnGraspEnd -= ClearLeftControllerStates;
						}

						foreach (InteractionController controller in rightControllers)
						{
							controller.OnGraspBegin -= SetRightControllerStates;
							controller.OnGraspEnd -= ClearRightControllerStates;
						}

						if (!leftHand.graspingEnabled) leftHand.graspingEnabled = true;
						if (!leftHand.contactEnabled) leftHand.contactEnabled = true;

						if (!rightHand.graspingEnabled) rightHand.graspingEnabled = true;
						if (!rightHand.contactEnabled) rightHand.contactEnabled = true;
					}
				}
			}
		}

		public InteractionController[] LeftControllers { get { return leftControllers; } }
		public InteractionController[] RightControllers { get { return rightControllers; } }

		private static PlatformControllerManager instance;
		public static PlatformControllerManager Instance { get { return instance; } }

		private void Awake()
		{
			PlatformInfo platform = platforms.First(item => item.Platform == PlatformID.SteamVR);
			currentPlatform = platform;

			leftControllers = platform.Controllers.Where(item => item.isLeft).ToArray();
			rightControllers = platform.Controllers.Where(item => item.isRight).ToArray();

			InteractionHand[] hands = GetComponentsInChildren<InteractionHand>(true);
			leftHand = hands.First(item => item.isLeft);
			rightHand = hands.First(item => item.isRight);

			leftDisableContactTimer = disableContactAfterGraspTime;
			rightDisableContactTimer = disableContactAfterGraspTime;

			instance = this;
		}

		void SetLeftControllerStates()
		{
			for(int i=0; i < leftControllers.Length; i++)
			{
				if (!leftControllers[i].isGraspingObject)
				{
					if (leftControllers[i].graspingEnabled) leftControllers[i].graspingEnabled = false;
				}
			}

			//if(leftHand.graspingEnabled) leftHand.graspingEnabled = false;
		}

		void ClearLeftControllerStates()
		{
			//if (!leftHand.graspingEnabled) leftHand.graspingEnabled = true;

			for(int i=0; i < leftControllers.Length; i++)
			{
				if (!leftControllers[i].graspingEnabled) leftControllers[i].graspingEnabled = true;
			}
		}

		void SetRightControllerStates()
		{
			for (int i = 0; i < rightControllers.Length; i++)
			{
				if (!rightControllers[i].isGraspingObject)
				{
					if (rightControllers[i].graspingEnabled) rightControllers[i].graspingEnabled = false;
				}
			}

			//if (rightHand.graspingEnabled) rightHand.graspingEnabled = false;
		}

		void ClearRightControllerStates()
		{
			//if (!rightHand.graspingEnabled) rightHand.graspingEnabled = true;

			for (int i = 0; i < rightControllers.Length; i++)
			{
				if (!rightControllers[i].graspingEnabled) rightControllers[i].graspingEnabled = true;
			}
		}

		bool AnyControllerGrasping(bool left)
		{
			InteractionController[] controllers = (left) ? leftControllers : rightControllers;

			for(int i=0; i< controllers.Length; i++)
			{
				if (controllers[i].isGraspingObject) return true;
			}

			return false;
		}

		void DoContactTimer(bool left)
		{
			if(left)
			{
				if (leftControllerGrasping = AnyControllerGrasping(true)) leftDisableContactTimer = 0;

				if(leftDisableContactTimer <= disableContactAfterGraspTime)
				{
					leftDisableContactTimer += Time.fixedDeltaTime;
				}

				if(leftHand.contactBones != null)
				{
					bool setContactEnabled = leftDisableContactTimer >= disableContactAfterGraspTime;
					leftContactEnabled = setContactEnabled;
					if (leftHand.contactEnabled != setContactEnabled) leftHand.contactEnabled = setContactEnabled;
				}
			}
			else
			{
				if (rightControllerGrasping = AnyControllerGrasping(false)) rightDisableContactTimer = 0;

				if(rightDisableContactTimer <= disableContactAfterGraspTime)
				{
					rightDisableContactTimer += Time.fixedDeltaTime;
				}

				if(rightHand.contactBones != null)
				{
					bool setContactEnabled = rightDisableContactTimer >= disableContactAfterGraspTime;
					rightContactEnabled = setContactEnabled;
					if (rightHand.contactEnabled != setContactEnabled) rightHand.contactEnabled = setContactEnabled;
				}
			}
		}

		private void FixedUpdate()
		{
			if (controllersEnabled)
			{
				DoContactTimer(true);
				DoContactTimer(false);
			}
		}
	}
}