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
		[SerializeField] LeapXRServiceProvider defaultProvider;

		public override Frame CurrentFixedFrame
		{
			get
			{
				return (IsActive) ? currentFrame : defaultProvider.CurrentFixedFrame;
			}
		}

		public override Frame CurrentFrame
		{
			get
			{
				return (IsActive) ? currentFrame : defaultProvider.CurrentFrame;
			}
		}

		List<Hand> hands = new List<Hand>(2);
		Frame currentFrame;

		[UnityEngine.Serialization.FormerlySerializedAs("rightHand")]
		[SerializeField] SkeletalControllerHand rightControllerHand;

		[UnityEngine.Serialization.FormerlySerializedAs("leftHand")]
		[SerializeField] SkeletalControllerHand leftControllerHand;

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

#if XR_PLUGIN_MANAGEMENT
			defaultProvider.updateHandInPrecull = true; // note this might cause problems
				// if there is no tracked pose driver. In fact, it might cause problems in general,
				// but I'm pretty sure the reason we turned it on in the first place was to account
				// for the presence of a tracked pose driver
#endif

			/*UserRig userRig = GetComponentInParent<UserRig>();
			defaultProvider.mainCamera = userRig.ViewCamera;*/

			defaultProvider.OnUpdateFrame += (Frame obj) =>
			{
				if(!IsActive) DispatchUpdateFrameEvent(obj);
			};

			defaultProvider.OnFixedFrame += (Frame obj) =>
			{
				if (!IsActive) DispatchFixedFrameEvent(obj);
			};
		}


		// Update is called once per frame
		void Update()
		{
			if (isActive)
			{
				// todo: it might be an OK idea here to generate a frame
				// and maybe smooth it out so that there's no jitter between fixed and visual updates?
				// actually I don't see any jitter so far, so idk.
				GenerateFrame();
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

			if (rightControllerHand != null && rightControllerHand.IsActive) hands.Add(rightControllerHand.LeapHand.Transform(leapTransform));
			if (leftControllerHand != null && leftControllerHand.IsActive) hands.Add(leftControllerHand.LeapHand.Transform(leapTransform));

			currentFrame.Id = frameID;
			currentFrame.Timestamp = timeStamp;

			frameID++;
			timeStamp++;
		}
	}

}