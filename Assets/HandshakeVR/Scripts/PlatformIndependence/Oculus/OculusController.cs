using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
    public class OculusController : TrackerValidity
    {
        [SerializeField] OVRHand hand;
        [SerializeField] OVRControllerHelper controller;

		// Start is called before the first frame update
		void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            OVRInput.Controller ovrController = OVRInput.GetActiveController();
            if (ovrController == OVRInput.Controller.Touch) ovrController = (IsLeft) ? OVRInput.Controller.LTouch :
                     OVRInput.Controller.RTouch;
            else if (ovrController == OVRInput.Controller.Hands) ovrController = (IsLeft) ? OVRInput.Controller.LHand :
                    OVRInput.Controller.RHand;


            /*bool isTracking = false
            isValid = isTracking;*/

            if(ovrController == OVRInput.Controller.RHand ||
                ovrController == OVRInput.Controller.LHand)
			{
                isValid = hand.IsTracked;
			}
            else if (ovrController == OVRInput.Controller.RTouch ||
                ovrController == OVRInput.Controller.LTouch)
			{
                isValid = OVRInput.IsControllerConnected(ovrController);
			}

            transform.localPosition = OVRInput.GetLocalControllerPosition(ovrController);
            transform.localRotation = OVRInput.GetLocalControllerRotation(ovrController);
        }
    }
}