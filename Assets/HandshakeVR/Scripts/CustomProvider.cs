using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap;
using Leap.Unity;
using System;

using CatchCo;

namespace HandshakeVR
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

		List<Hand> hands = new List<Hand>(2);
		Frame currentFrame;

		[UnityEngine.Serialization.FormerlySerializedAs("rightHand")]
		[SerializeField] SkeletalControllerHand rightControllerHand;
		//Leap.Hand rightLeapHand;

		[UnityEngine.Serialization.FormerlySerializedAs("leftHand")]
		[SerializeField] SkeletalControllerHand leftControllerHand;
		//Leap.Hand leftLeapHand;		

		[SerializeField]
		bool isActive = false;
		int frameID = 0;
		long timeStamp = 0;

		public bool IsActive { get { return isActive; } set { isActive = value; } }
		public int FrameID { get { return frameID; } }

		private void Awake()
		{
			currentFrame = new Frame(0, timeStamp, 60, hands);
			rightControllerHand.LeapProvider = this;
			leftControllerHand.LeapProvider = this;
		}

		// Use this for initialization
		//IEnumerator Start()
		//{
		//	yield return new WaitForSeconds(0.1f);

		//	isActive = true;
		//}

		// Update is called once per frame
		void Update()
		{
			if (isActive)
			{
				//GenerateFrame();
				DispatchUpdateFrameEvent(currentFrame);
			}
		}

		private void FixedUpdate()
		{
			if (isActive)
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

			/*if (rightControllerHand != null && rightControllerHand.IsActive) hands.Add(rightControllerHand.GenerateHandData(frameID).Transform(leapTransform));
			if (leftControllerHand != null && leftControllerHand.IsActive) hands.Add(leftControllerHand.GenerateHandData(frameID).Transform(leapTransform));*/

			if (rightControllerHand != null && rightControllerHand.IsActive) hands.Add(rightControllerHand.LeapHand.Transform(leapTransform));
			if (leftControllerHand != null && leftControllerHand.IsActive) hands.Add(leftControllerHand.LeapHand.Transform(leapTransform));

			//currentFrame = new Leap.Frame(frameID, timeStamp, 60, hands);
			currentFrame.Id = frameID;
			currentFrame.Timestamp = timeStamp;
			//frame.Hands = hands;

			frameID++;
			timeStamp++;
		}
	}

}