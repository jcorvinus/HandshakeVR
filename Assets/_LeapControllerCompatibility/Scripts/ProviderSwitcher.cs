using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;
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

        [SerializeField]
        InteractionHand leftInteractionHand;

        [SerializeField]
        InteractionHand rightInteractionHand;

        [Header("SteamVR Variables")]
        [SerializeField]
        SteamVR_Behaviour_Pose leftHandPose;

        [SerializeField]
        GameObject leftHandVisual;

        [SerializeField]
        SteamVR_Behaviour_Pose rightHandPose;

        [SerializeField]
        GameObject rightHandVisual;

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
                if (leftInteractionHand) leftInteractionHand.leapProvider = defaultProvider;
                if (rightInteractionHand) rightInteractionHand.leapProvider = defaultProvider;
            }
            else
            {
                modelManager.leapProvider = customProvider;
                if (leftInteractionHand) leftInteractionHand.leapProvider = customProvider;
                if (rightInteractionHand) rightInteractionHand.leapProvider = customProvider;
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