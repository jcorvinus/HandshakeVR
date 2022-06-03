using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Interaction;

//using Valve.VR;

using CatchCo;

namespace HandshakeVR
{
    public class ProviderSwitcher : MonoBehaviour
    {
		public delegate void SwitchHandler(ProviderSwitcher sender, bool providerIsDefault);
		public event SwitchHandler ProviderSwitched;

        [SerializeField]
        CustomProvider customProvider;

		bool isDefault = true;

        public bool IsDefault { get { return isDefault; } }

		[SerializeField]
        HandModelManager modelManager;

        Leap.Unity.Interaction.InteractionManager interactionManager;
		PlatformControllerManager controllerManager;
		UserRig userRig;
		PlatformManager platformManager;
        InteractionHand leftInteractionHand;
        InteractionHand rightInteractionHand;

		public PlatformControllerManager ControllerManager { get { return controllerManager; } }

		SkeletalControllerHand leftSkeletalControllerHand;
		SkeletalControllerHand rightSkeletalControllerHand;

		public SkeletalControllerHand LeftControllerHand { get { return leftSkeletalControllerHand; } }
		public SkeletalControllerHand RightControllerHand { get { return rightSkeletalControllerHand; } }

        [Header("Debugging")]
        [SerializeField]
        bool manualProviderSwitching = false;

        private void Awake()
        {
			userRig = GetComponentInParent<UserRig>();
			platformManager = GetComponent<PlatformManager>();
            interactionManager = Leap.Unity.Interaction.InteractionManager.instance;
			if(interactionManager != null) controllerManager = interactionManager.GetComponent<PlatformControllerManager>();

			Hands.Provider = customProvider;
			modelManager.leapProvider = customProvider;

			if (interactionManager)
            { 
                foreach(InteractionController controller in interactionManager.interactionControllers)
                {
                    if(controller is InteractionHand)
                    {
                        InteractionHand hand = (InteractionHand)controller;

                        if (hand.isLeft) leftInteractionHand = hand;
                        else rightInteractionHand = hand;

						hand.leapProvider = customProvider;
                    }
                }
            }

			SkeletalControllerHand[] hands = transform.parent.GetComponentsInChildren<SkeletalControllerHand>(true);
			leftSkeletalControllerHand = hands.First(item => item.IsLeft);
			rightSkeletalControllerHand = hands.First(item => !item.IsLeft);
        }

        // Use this for initialization
        void Start()
        {
            SetProvider();
        }

        // Update is called once per frame
        void Update()
        {
            if (manualProviderSwitching)
            {
                if (Input.GetKeyUp(KeyCode.Space)) SwitchProviders();
            }
            else
            {
                if (isDefault)
                {
                    if (ShouldUseControllers()) SwitchProviders();
                }
                else
                {
                    if (!ShouldUseControllers()) SwitchProviders();
                }
            }

			if(!isDefault)
			{
				// update our tracking/active state properly.
				leftSkeletalControllerHand.IsActive = GetControllerValidity(true);
				rightSkeletalControllerHand.IsActive = GetControllerValidity(false);
			}
			else
			{
				leftSkeletalControllerHand.IsActive = false;
				rightSkeletalControllerHand.IsActive = false;
			}
        }

		bool GetControllerValidity(bool left)
		{
			return platformManager.GetControllerTrackingValidity(left);
		}

        private bool ShouldUseControllers()
        {
			if (customProvider.IsDefaultProviderConnected) return false;
            return GetControllerValidity(true) || GetControllerValidity(false);
        }

        void SetProvider()
        {
            if (isDefault)
            {
				if (controllerManager) controllerManager.ControllersEnabled = false;
            }
            else
            {
				if (controllerManager) controllerManager.ControllersEnabled = true;
            }

			customProvider.IsActive = !isDefault;

			if (ProviderSwitched != null)
			{
				ProviderSwitched(this, isDefault);
			}
		}

        [ExposeMethodInEditor]
        void SwitchProviders()
        {
            isDefault = !isDefault;

            SetProvider();
        }
    }
}