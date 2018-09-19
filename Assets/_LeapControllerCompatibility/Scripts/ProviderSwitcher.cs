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

        bool isDefault = true;

        public bool IsDefault { get { return isDefault; } }

        [Tooltip("If you want to use the SteamVR hand mesh, enable this.")]
        [SerializeField]
        bool hideLeapHandsOnSwitch = false;

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

        [Header("Debugging")]
        [SerializeField]
        bool manualProviderSwitching = false;

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
                    //if(leftInteractionController) leftInteractionController.graspingEnabled = false; // used to be commented
                    leftInteractionHand.graspingEnabled = true;
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = defaultProvider;
                    //if(rightInteractionController) rightInteractionController.graspingEnabled = false; // used to be commented
                    rightInteractionHand.graspingEnabled = true;
                }
            }
            else
            {
                modelManager.leapProvider = customProvider;
                if (leftInteractionHand)
                {
                    leftInteractionHand.leapProvider = customProvider;
                    /*leftInteractionController.graspingEnabled = true; // used to be commented
                    leftInteractionHand.graspingEnabled = false; // used to be commented*/
                }
                if (rightInteractionHand)
                {
                    rightInteractionHand.leapProvider = customProvider;
                    /*rightInteractionController.graspingEnabled = true; // used to be commented
                    rightInteractionHand.graspingEnabled = false; // used to be commented*/
                }

                for (int i=0; i < modelManager.transform.childCount; i++) // this method will fail if the hand objects aren't children of the model manager
                {

                    modelManager.transform.GetChild(i).gameObject.SetActive(!hideLeapHandsOnSwitch);
                }
            }

            modelManager.GraphicsEnabled = isDefault || !hideLeapHandsOnSwitch;
            leftHandVisual.gameObject.SetActive(!modelManager.GraphicsEnabled);
            rightHandVisual.gameObject.SetActive(!modelManager.GraphicsEnabled);

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