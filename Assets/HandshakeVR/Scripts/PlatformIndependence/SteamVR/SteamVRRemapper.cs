using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
    public class SteamVRRemapper : HandInputProvider
	{
		enum SteamVR_ControllerType { Unknown = -1, Vive = 0, Touch = 1, Knuckles = 2 }

        [Tooltip("These refer to our SteamVR skeleton, which will get retargeted onto the leap skeleton")]
        [Header("SteamVR skeleton variables")]
        [SerializeField]
        Transform wrist;

        [SerializeField]
        BoneBasis wristBasis;

        [SerializeField]
        Vector3 palmOffset;

		SteamVR_ControllerType steamVRControllerType = SteamVR_ControllerType.Unknown;
		EVRSkeletalTrackingLevel lastTrackingLevel;

        // 0 finger_index_meta_r
        // 1 finger_middle_meta_r
        // 2 finger_ring_meta_r
        // 3 finger_pinky_meta_r
        // 4 finger_thumb_0_r
        Transform[] fingerMetacarpals;

        [SerializeField]
        BoneBasis fingerBasis;

		[SerializeField] GameObject poseGameObject;
#if UNITY_STANDALONE
		SteamVR_Behaviour_Pose steamVRPose;
		MaskedSteamVRSkeleton skeletonBehavior;
#endif

#region SteamVR Pinch Pose
		Animator animator;
		string trackpadTouchName = "/actions/HandPoseAssist/in/TrackpadTouch";
		string aButtonTouchName = "/actions/HandPoseAssist/in/AButtonTouch";
		string bButtonTouchName = "/actions/HandPoseAssist/in/BButtonTouch";
		string triggerTouchName = "/actions/HandPoseAssist/in/TriggerTouch";
		string triggerPullName = "/actions/HandPoseAssist/in/TriggerPull";
		string grabGripName = "/actions/default/in/GrabGrip";
		string viveThumbHorizName = "/actions/HandPoseAssist/in/ThumbHorizontal";
		string assistActionSetName = "/actions/HandPoseAssist";

#if UNITY_STANDALONE
		SteamVR_Action_Boolean trackpadTouch;
		SteamVR_Action_Boolean aButtonTouch;
		SteamVR_Action_Boolean bButtonTouch;
		SteamVR_Action_Boolean triggerTouch;
		SteamVR_Action_Single triggerPull;
		SteamVR_Action_Boolean grabGrip;
		SteamVR_Action_Vector2 viveThumbHoriz;
		SteamVR_ActionSet assistActionset;
#endif

		int isPinchingHash;
		int pinchAmtHash;
		int isGrabbedHash;
		int pinchThumbHorizHash;

		float pinchTweenDuration = 0.025f;
		float pinchTweenTime = 0;
		float pinchTValue = 0;
		float pinchThumbHoriz = 0;
#endregion

		[Header("Debug Vars")]
        [SerializeField]
        bool drawSkeleton = false;

		protected override void Awake()
		{
			base.Awake();

#if UNITY_STANDALONE
			steamVRPose = poseGameObject.GetComponent<SteamVR_Behaviour_Pose>();
			skeletonBehavior = steamVRPose.GetComponentInChildren<MaskedSteamVRSkeleton>();
			trackpadTouch = SteamVR_Input.GetBooleanActionFromPath(trackpadTouchName);
			aButtonTouch = SteamVR_Input.GetBooleanActionFromPath(aButtonTouchName);
			bButtonTouch = SteamVR_Input.GetBooleanActionFromPath(bButtonTouchName);
			triggerTouch = SteamVR_Input.GetBooleanActionFromPath(triggerTouchName);
			triggerPull = SteamVR_Input.GetSingleActionFromPath(triggerPullName);
			grabGrip = SteamVR_Input.GetBooleanActionFromPath(grabGripName);
			viveThumbHoriz = SteamVR_Input.GetVector2ActionFromPath(viveThumbHorizName);
			assistActionset = SteamVR_Input.GetActionSet(assistActionSetName);
#endif

			isPinchingHash = Animator.StringToHash("IsPinching");
			pinchAmtHash = Animator.StringToHash("PinchAmt");
			isGrabbedHash = Animator.StringToHash("IsGrabbed");
			pinchThumbHorizHash = Animator.StringToHash("ThumbHoriz");

			animator = poseGameObject.GetComponentInChildren<Animator>();
		}

        private void Start()
        {
            GetMetacarpals();
			skeletonBehavior.onBoneTransformsUpdatedEvent += (SteamVR_Behaviour_Skeleton sender, 
				SteamVR_Input_Sources sources) =>
			{
				lastTrackingLevel = skeletonBehavior.skeletalTrackingLevel;
			};
        }

        void GetMetacarpals()
        {
            fingerMetacarpals = new Transform[5];

            fingerMetacarpals[0] = wrist.Find("finger_index_meta_r");
            fingerMetacarpals[1] = wrist.Find("finger_middle_meta_r");
            fingerMetacarpals[2] = wrist.Find("finger_ring_meta_r");
            fingerMetacarpals[3] = wrist.Find("finger_pinky_meta_r");
            fingerMetacarpals[4] = wrist.Find("finger_thumb_0_r");
        }

		public override HandTrackingType TrackingType()
		{
#if UNITY_STANDALONE
			if (!skeletonBehavior.skeletonAction.active) return HandTrackingType.None;

			switch (lastTrackingLevel)
			{
				case EVRSkeletalTrackingLevel.VRSkeletalTracking_Estimated:
				case EVRSkeletalTrackingLevel.VRSkeletalTracking_Partial:
					return HandTrackingType.Emulation;
				case EVRSkeletalTrackingLevel.VRSkeletalTracking_Full:
					return HandTrackingType.Skeletal;
			default:
					return HandTrackingType.Emulation;
			}
#else
			return HandTrackingType.Emulation;
#endif
		}

		private void GetControllerType()
		{
#if UNITY_STANDALONE
			// string property for?
			Valve.VR.ETrackedPropertyError error = Valve.VR.ETrackedPropertyError.TrackedProp_Success;
			Valve.VR.ETrackedDeviceProperty manufacturerNameProperty = Valve.VR.ETrackedDeviceProperty.Prop_ManufacturerName_String;
			uint manufacturerNameCapacity = SteamVR.instance.hmd.GetStringTrackedDeviceProperty((uint)steamVRPose.GetDeviceIndex(), manufacturerNameProperty,
				null, 0, ref error);

			string manufacturerResultAsString = "";
			if (manufacturerNameCapacity > 1)
			{
				var manufacturerResult = new System.Text.StringBuilder((int)manufacturerNameCapacity);
				SteamVR.instance.hmd.GetStringTrackedDeviceProperty((uint)steamVRPose.GetDeviceIndex(), manufacturerNameProperty, manufacturerResult, manufacturerNameCapacity, ref error);

				manufacturerResultAsString = manufacturerResult.ToString();

				if(manufacturerResultAsString.Equals("Oculus"))
				{
					steamVRControllerType = SteamVR_ControllerType.Touch;
				}
				else
				{
					// figure out if we're vive or touch
					error = ETrackedPropertyError.TrackedProp_Success;
					Valve.VR.ETrackedDeviceProperty renderModelName = ETrackedDeviceProperty.Prop_RenderModelName_String;

					uint renderModelStringCapacity = SteamVR.instance.hmd.GetStringTrackedDeviceProperty((uint)steamVRPose.GetDeviceIndex(), renderModelName, null, 0, ref error);

					if (renderModelStringCapacity > 1)
					{
						var renderModelResult = new System.Text.StringBuilder((int)renderModelStringCapacity);
						SteamVR.instance.hmd.GetStringTrackedDeviceProperty((uint)steamVRPose.GetDeviceIndex(), renderModelName, renderModelResult, renderModelStringCapacity, ref error);
						Debug.Log("Controller rendermodel name: " + renderModelResult.ToString());

						if (renderModelResult.ToString().ToLower().Contains("index")) steamVRControllerType = SteamVR_ControllerType.Knuckles;
						else steamVRControllerType = SteamVR_ControllerType.Vive;
					}
				}
				//steamVRControllerType = (manufacturerResultAsString.Equals("Oculus")) ? SteamVR_ControllerType.Touch : SteamVR_ControllerType.Vive;
				Debug.Log("controller manufacturer: " + manufacturerResultAsString);
				Debug.Log("Controller type detected: " + steamVRControllerType);
			}
			else
			{
				steamVRControllerType = SteamVR_ControllerType.Unknown;
			}
#endif
		}

		bool isPinching;
		bool faceButtonTouch;
		bool isTriggerTouch;

		void UpdateAnimatorKnuckles()
		{
#if UNITY_STANDALONE
			SteamVR_Input_Sources inputSource = (controllerHand.IsLeft) ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
			if (!assistActionset.IsActive()) assistActionset.Activate(inputSource);

			if (!skeletonBehavior.skeletonAction.active) return;

			animator.SetBool(isGrabbedHash, grabGrip.GetState(inputSource));

			faceButtonTouch = (aButtonTouch.GetState(inputSource) || bButtonTouch.GetState(inputSource) ||
				trackpadTouch.GetState(inputSource));

			isTriggerTouch = triggerTouch.GetState(inputSource);
			isPinching = faceButtonTouch;
			if (faceButtonTouch)
			{
				pinchThumbHoriz += (trackpadTouch.GetState(inputSource) ? -1 : 1) * Time.deltaTime * 6;
				pinchThumbHoriz = Mathf.Clamp01(pinchThumbHoriz);
			}

			animator.SetBool(isPinchingHash, isPinching);
			animator.SetFloat(pinchAmtHash, skeletonBehavior.fingerCurls[1]);
			animator.SetFloat(pinchThumbHorizHash, pinchThumbHoriz);

			pinchTweenTime += (isPinching) ? Time.deltaTime : -Time.deltaTime;
			pinchTweenTime = Mathf.Clamp(pinchTweenTime, 0, pinchTweenDuration);
			pinchTValue = Mathf.InverseLerp(0, pinchTweenDuration, pinchTweenTime);
			if (skeletonBehavior)
			{
				skeletonBehavior.indexSkeletonBlend = 1 - pinchTValue;
				skeletonBehavior.thumbSkeletonBlend = 1 - pinchTValue;
			}
#endif
		}

		void UpdateAnimatorVive()
		{
			#if UNITY_STANDALONE
			SteamVR_Input_Sources inputSource = (controllerHand.IsLeft) ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
			if (!assistActionset.IsActive()) assistActionset.Activate(inputSource);

			faceButtonTouch = trackpadTouch.GetState(inputSource);
			isTriggerTouch = triggerTouch.GetState(inputSource);
			isPinching = faceButtonTouch;

			if (faceButtonTouch)
			{
				float thumbRemap = viveThumbHoriz.axis.x;
				if (inputSource == SteamVR_Input_Sources.RightHand) thumbRemap = (1 - thumbRemap) - 0.5f;
				pinchThumbHoriz = Mathf.Clamp01(thumbRemap * 0.5f);
			}

			animator.SetBool(isGrabbedHash, grabGrip.GetState(inputSource) && !isPinching);

			animator.SetBool(isPinchingHash, isPinching);
			animator.SetFloat(pinchAmtHash, skeletonBehavior.fingerCurls[1]);
			animator.SetFloat(pinchThumbHorizHash, pinchThumbHoriz);

			pinchTweenTime += (isPinching) ? Time.deltaTime : -Time.deltaTime;
			pinchTweenTime = Mathf.Clamp(pinchTweenTime, 0, pinchTweenDuration);
			pinchTValue = Mathf.InverseLerp(0, pinchTweenDuration, pinchTweenTime);

			if (skeletonBehavior)
			{
				skeletonBehavior.indexSkeletonBlend = 1 - pinchTValue;
				skeletonBehavior.thumbSkeletonBlend = 1 - pinchTValue;
			}
#endif
		}

		void UpdateAnimatorTouch()
		{
			#if UNITY_STANDALONE
			SteamVR_Input_Sources inputSource = (controllerHand.IsLeft) ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
			if (!assistActionset.IsActive()) assistActionset.Activate(inputSource);

			animator.SetBool(isGrabbedHash, grabGrip.GetState(inputSource));

			faceButtonTouch = (aButtonTouch.GetState(inputSource) || bButtonTouch.GetState(inputSource) ||
				trackpadTouch.GetState(inputSource));

			isTriggerTouch = triggerTouch.GetState(inputSource);
			isPinching = faceButtonTouch;
			if (faceButtonTouch)
			{
				pinchThumbHoriz += (trackpadTouch.GetState(inputSource) ? -1 : 1) * Time.deltaTime * 6;
				pinchThumbHoriz = Mathf.Clamp01(pinchThumbHoriz);
			}

			animator.SetBool(isPinchingHash, isPinching);
			animator.SetFloat(pinchAmtHash, skeletonBehavior.fingerCurls[1]);
			animator.SetFloat(pinchThumbHorizHash, pinchThumbHoriz);

			pinchTweenTime += (isPinching) ? Time.deltaTime : -Time.deltaTime;
			pinchTweenTime = Mathf.Clamp(pinchTweenTime, 0, pinchTweenDuration);
			pinchTValue = Mathf.InverseLerp(0, pinchTweenDuration, pinchTweenTime);
			if (skeletonBehavior)
			{
				skeletonBehavior.indexSkeletonBlend = 1 - pinchTValue;
				skeletonBehavior.thumbSkeletonBlend = 1 - pinchTValue;
			}
#endif
		}

		private void Update()
        {
			#if UNITY_STANDALONE
			if (controllerHand.IsActive)
			{
				if (steamVRControllerType == SteamVR_ControllerType.Unknown) GetControllerType();
				if (steamVRControllerType == SteamVR_ControllerType.Knuckles) UpdateAnimatorKnuckles();
				else if (steamVRControllerType == SteamVR_ControllerType.Vive) UpdateAnimatorVive();
				else if (steamVRControllerType == SteamVR_ControllerType.Touch) UpdateAnimatorTouch();

				animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

				// set our wrist positions to match first
				controllerHand.Wrist.transform.position = wrist.transform.position;

				Quaternion wristBoneOrientation = controllerHand.GetLocalBasis();
				controllerHand.Wrist.rotation = GlobalRotationFromBasis(wrist, wristBasis) * wristBoneOrientation;

				for (int fingerIndx = 0; fingerIndx < fingerMetacarpals.Length; fingerIndx++)
				{
					Transform fingerRoot = null;

					switch (fingerIndx)
					{
						case (0):
							fingerRoot = controllerHand.IndexMetacarpal;
							break;
						case (1):
							fingerRoot = controllerHand.MiddleMetacarpal;
							break;
						case (2):
							fingerRoot = controllerHand.RingMetacarpal;
							break;
						case (3):
							fingerRoot = controllerHand.PinkyMetacarpal;
							break;
						default:
							fingerRoot = controllerHand.ThumbMetacarpal;
							break;
					}

					MatchBones(fingerMetacarpals[fingerIndx], fingerRoot, fingerBasis, wristBoneOrientation);

					Transform steamVRTip, controllerTip;
					float tipScale = 0.659f;

					switch (fingerIndx)
					{
						case (0):
							steamVRTip = skeletonBehavior.indexTip;
							controllerTip = fingerRoot.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
							break;
						case (1):
							steamVRTip = skeletonBehavior.middleTip;
							controllerTip = fingerRoot.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
							break;
						case (2):
							steamVRTip = skeletonBehavior.ringTip;
							controllerTip = fingerRoot.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
							break;
						case (3):
							steamVRTip = skeletonBehavior.pinkyTip;
							controllerTip = fingerRoot.GetChild(0).GetChild(0).GetChild(0).GetChild(0);
							break;
						default:
							steamVRTip = skeletonBehavior.thumbTip;
							controllerTip = fingerRoot.GetChild(0).GetChild(0).GetChild(0);
							tipScale *= 1.25f;
							break;
					}

					Vector3 scaleFactor = new Vector3(
					fingerBasis.Forward.x != 0 ? tipScale : 1,
					fingerBasis.Forward.y != 0 ? tipScale : 1,
					fingerBasis.Forward.z != 0 ? tipScale : 1);
					Vector3 tipLocal = steamVRTip.transform.parent.InverseTransformPoint(steamVRTip.position);
					controllerTip.transform.localPosition = Vector3.Scale(tipLocal, scaleFactor);
				}
			}
			else
			{
				animator.cullingMode = AnimatorCullingMode.CullCompletely;
			}
#endif
        }

        public bool IsTracking
		{
			get
			{
#if UNITY_STANDALONE
				return steamVRPose.isValid;
#else
				return false;
#endif
			}
		}

		void MatchBones(Transform steamVRBone, Transform leapBone, BoneBasis basis,
			Quaternion leapOrientation, int depth = 0)
		{
			if (depth == 0) leapBone.transform.position = steamVRBone.transform.position;
			else
			{
				controllerHand.SetTransformWithConstraint(leapBone, steamVRBone.transform.position, GlobalRotationFromBasis(steamVRBone, basis) * leapOrientation);
			}

			if (steamVRBone.childCount == leapBone.childCount)
			{
				if (steamVRBone.childCount == 1)
				{
					MatchBones(steamVRBone.GetChild(0), leapBone.GetChild(0), basis, leapOrientation, depth + 1);
				}
			}
			else
			{
				Debug.LogError("Mismatch between steamVR and leap child count. Steam Bone:" + steamVRBone + " leap bone: " + leapBone);
				Debug.Break();
			}
		}

        private void DrawBones(Transform parent, BoneBasis basis)
        {
            DrawBasis(parent, basis);

            for(int i=0; i < parent.childCount; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(parent.transform.position,
                    parent.GetChild(i).position);

                DrawBones(parent.GetChild(i), basis);
            }
        }

        private void DrawPalm(Transform bone, BoneBasis basis)
        {
            DrawBasis(bone, basis);

            Quaternion rotation = Quaternion.LookRotation(bone.transform.TransformDirection(basis.Forward),
                basis.Up);

            Vector3 up, forward, right;

            up = rotation * Vector3.up;
            forward = rotation * Vector3.forward;
            right = rotation * Vector3.right;

            Vector3 palmPosition = wrist.TransformPoint(palmOffset);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(palmPosition, palmPosition + forward * 0.04f);
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled || !drawSkeleton) return;

            if (fingerMetacarpals == null || fingerMetacarpals.Length == 0) GetMetacarpals();

            for (int i = 0; i < fingerMetacarpals.Length; i++)
            {
                DrawBones(fingerMetacarpals[i], fingerBasis);
            }

            DrawPalm(wrist, wristBasis);
        }
    }
}