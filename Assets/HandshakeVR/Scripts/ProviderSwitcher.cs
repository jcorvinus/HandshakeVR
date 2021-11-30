﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Interaction;

using Valve.VR;

using CatchCo;

namespace HandshakeVR
{
    public class ProviderSwitcher : MonoBehaviour
    {
		public delegate void SwitchHandler(ProviderSwitcher sender, bool providerIsDefault);
		public event SwitchHandler ProviderSwitched;

		[SerializeField]
        LeapServiceProvider defaultProvider;

        [SerializeField]
        CustomProvider customProvider;

		// hands can be enabled/disabled so that you can replace them with embodied tools.
		private bool leftHandEnabled;
		private bool rightHandEnabled;
		public bool LeftHandEnabled { get { return leftHandEnabled; } set { leftHandEnabled = value; } }
		public bool RightHandEnabled { get { return rightHandEnabled; } set { rightHandEnabled = value; } }

		bool isDefault = true;

        public bool IsDefault { get { return isDefault; } }

		[SerializeField]
        HandModelManager modelManager;

        Leap.Unity.Interaction.InteractionManager interactionManager;
		PlatformControllerManager controllerManager;
		PlatformManager platformManager;
        InteractionHand leftInteractionHand;
        InteractionHand rightInteractionHand;

		DataHand leftAbstractHand;
		DataHand rightAbstractHand;

		public DataHand LeftAbstractHandModel { get { return leftAbstractHand; } }
		public DataHand RightAbstractHandModel { get { return rightAbstractHand; } }

		SkeletalControllerHand leftSkeletalControllerHand;
		SkeletalControllerHand rightSkeletalControllerHand;

		public SkeletalControllerHand LeftControllerHand { get { return leftSkeletalControllerHand; } }
		public SkeletalControllerHand RightControllerHand { get { return rightSkeletalControllerHand; } }

        [Header("Debugging")]
        [SerializeField]
        bool manualProviderSwitching = false;

        private void Awake()
        {
			platformManager = GetComponent<PlatformManager>();
            interactionManager = Leap.Unity.Interaction.InteractionManager.instance;
			if(interactionManager != null) controllerManager = interactionManager.GetComponent<PlatformControllerManager>();

			DataHand[] dataHands = modelManager.GetComponentsInChildren<DataHand>(true);

			leftAbstractHand = dataHands.First(item => item is DataHand && item.Handedness == Chirality.Left);
			rightAbstractHand = dataHands.First(item => item is DataHand && item.Handedness == Chirality.Right);

            if(interactionManager)
            { 
                foreach(InteractionController controller in interactionManager.interactionControllers)
                {
                    if(controller is InteractionHand)
                    {
                        InteractionHand hand = (InteractionHand)controller;

                        if (hand.isLeft) leftInteractionHand = hand;
                        else rightInteractionHand = hand;

						hand.leapProvider = defaultProvider;
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
            return GetControllerValidity(true) || GetControllerValidity(false);
        }

        void SetProvider()
        {
            if (isDefault)
            {
                modelManager.leapProvider = defaultProvider;
				if (leftInteractionHand)
                {
                    leftInteractionHand.leapProvider = defaultProvider;
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = defaultProvider;
                }
				if (controllerManager) controllerManager.ControllersEnabled = false;
            }
            else
            {
                modelManager.leapProvider = customProvider;
                if (leftInteractionHand)
                {
                    leftInteractionHand.leapProvider = customProvider;
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = customProvider;
                }
				if (controllerManager) controllerManager.ControllersEnabled = true;

				for (int i=0; i < modelManager.transform.childCount; i++) // this method will fail if the hand objects aren't children of the model manager
                {
					HandModel handModel = modelManager.transform.GetChild(i).GetComponent<HandModel>();

                    if(handModel != null && handModel.HandModelType == ModelType.Graphics) modelManager.transform.GetChild(i).gameObject.SetActive(!platformManager.HideLeapHandsOnSwitch());
                }
            }

            modelManager.GraphicsEnabled = isDefault || !platformManager.HideLeapHandsOnSwitch();
			PlatformManager.Instance.SetPlatformVisualHands(!modelManager.GraphicsEnabled);

            Hands.Provider = (isDefault) ? defaultProvider : (LeapProvider)customProvider;
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