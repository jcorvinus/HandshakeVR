using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;
using System;

using CatchCo;

namespace CoordinateSpaceConversion
{
    public class CustomProvider : LeapProvider
    {
        public override Frame CurrentFixedFrame
        {
            get
            {
                return currentFrame;
            }
        }

        public override Frame CurrentFrame
        {
            get
            {
                return currentFrame;
            }
        }

        List<Hand> hands = new List<Hand>();
        Frame currentFrame;

        [SerializeField]
        SkeletalControllerHand rightHand;

        [SerializeField]
        SkeletalControllerHand leftHand;

        bool framesActive = false;
        int frameID = 0;
        long timeStamp = 0;

        // Use this for initialization
        IEnumerator Start()
        {
            yield return new WaitForSeconds(0.1f);

            framesActive = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (framesActive)
            {
                GenerateFrame();
                DispatchUpdateFrameEvent(currentFrame);
            }
        }

        private void FixedUpdate()
        {
            if (framesActive)
            {
                GenerateFrame();
                DispatchFixedFrameEvent(currentFrame);
            }
        }

        [ExposeMethodInEditor]
        private void GenerateFrame()
        {
            hands.Clear();

            LeapTransform leapTransform = new LeapTransform(Vector.Zero, LeapQuaternion.Identity, Vector.Ones);

            if (rightHand != null) hands.Add(rightHand.GenerateHandData(frameID).Transform(leapTransform));
            if (leftHand != null) hands.Add(leftHand.GenerateHandData(frameID).Transform(leapTransform));
            currentFrame = new Leap.Frame(frameID, timeStamp, 60, hands);

            frameID++;
            timeStamp++;
        }
    }
}