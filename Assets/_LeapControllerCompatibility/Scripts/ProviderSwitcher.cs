using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
using Leap.Unity.Query;
using Leap.Unity.Interaction;

using Valve.VR;

using CatchCo;

namespace CoordinateSpaceConversion
{
    public class ProviderSwitcher : MonoBehaviour
    {
        [SerializeField]
        LeapServiceProvider defaultProvider;

        [SerializeField]
        CustomProvider customProvider;

        bool isDefault = false;

        [SerializeField]
        HandModelManager modelManager;

        InteractionManager interactionManager;
        InteractionHand leftInteractionHand;
        InteractionHand rightInteractionHand;

        [Header("SteamVR Variables")]
        [SerializeField]
        SteamVR_Behaviour_Pose leftHandPose;

        [SerializeField]
        GameObject leftHandVisual;
        SteamVRInteractionController leftInteractionController;

        [SerializeField]
        SteamVR_Behaviour_Pose rightHandPose;

        [SerializeField]
        GameObject rightHandVisual;
        SteamVRInteractionController rightInteractionController;

        private void Awake()
        {
            interactionManager = InteractionManager.instance;

            if(interactionManager)
            { 
                foreach(InteractionController controller in interactionManager.interactionControllers)
                {
                    if(controller is InteractionHand)
                    {
                        InteractionHand hand = (InteractionHand)controller;

                        if (hand.isLeft) leftInteractionHand = hand;
                        else rightInteractionHand = hand;
                    }
                    else if (controller is SteamVRInteractionController)
                    {
                        SteamVRInteractionController interactionController = (SteamVRInteractionController)controller;

                        if (interactionController.isLeft) leftInteractionController = interactionController;
                        else rightInteractionController = interactionController;
                    }
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            SetProvider();
        }

        // Update is called once per frame
        void Update()
        {
            //if (Input.GetKeyUp(KeyCode.Space)) SwitchProviders();

            if(isDefault)
            {
                if (ShouldUseControllers()) SwitchProviders();
            }
            else
            {
                if (!ShouldUseControllers()) SwitchProviders();
            }
        }

        private bool ShouldUseControllers()
        {
            return leftHandPose.isValid || rightHandPose.isValid;
        }

        void SetProvider()
        {
            if (isDefault)
            {
                modelManager.leapProvider = defaultProvider;
                if (leftInteractionHand)
                {
                    leftInteractionHand.leapProvider = defaultProvider;
                    leftInteractionController.graspingEnabled = false;
                    leftInteractionHand.graspingEnabled = true;
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = defaultProvider;
                    rightInteractionController.graspingEnabled = false;
                    rightInteractionHand.graspingEnabled = true;
                }
            }
            else
            {
                modelManager.leapProvider = customProvider;
                if (leftInteractionHand)
                {
                    leftInteractionHand.leapProvider = customProvider;
                    leftInteractionController.graspingEnabled = true;
                    leftInteractionHand.graspingEnabled = false;
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = customProvider;
                    rightInteractionController.graspingEnabled = true;
                    rightInteractionHand.graspingEnabled = false;
                }

                for(int i=0; i < modelManager.transform.childCount; i++) // this method will fail if the hand objects aren't direct children of the 
                {
                    modelManager.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            modelManager.GraphicsEnabled = isDefault;
            leftHandVisual.gameObject.SetActive(!isDefault);
            rightHandVisual.gameObject.SetActive(!isDefault);

            Hands.Provider = (isDefault) ? defaultProvider : (LeapProvider)customProvider;
        }

        [ExposeMethodInEditor]
        void SwitchProviders()
        {
            isDefault = !isDefault;

            SetProvider();
        }
    }
}