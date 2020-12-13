using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Valve.VR;

namespace HandshakeVR
{
    public class SteamVRRemapper : MonoBehaviour
    {
		enum SteamVR_ControllerType { Unknown = -1, Vive = 0, Touch = 1, Knuckles = 2 }

        [Tooltip("This is our leap hand data generator")]
        [SerializeField]
        SkeletalControllerHand controllerHand;

        [Tooltip("These refer to our SteamVR skeleton, which will get retargeted onto the leap skeleton")]
        [Header("SteamVR skeleton variables")]
        [SerializeField]
        Transform wrist;

        [SerializeField]
        BoneBasis wristBasis;

        [SerializeField]
        Vector3 palmOffset;

		SteamVR_ControllerType steamVRControllerType = SteamVR_ControllerType.Unknown;

        // 0 finger_index_meta_r
        // 1 finger_middle_meta_r
        // 2 finger_ring_meta_r
        // 3 finger_pinky_meta_r
        // 4 finger_thumb_0_r
        Transform[] fingerMetacarpals;

        [SerializeField]
        BoneBasis fingerBasis;

		[SerializeField]
        SteamVR_Behaviour_Pose steamVRPose;
		[SerializeField]
		MaskedSteamVRSkeleton skeletonBehavior;

		#region SteamVR Pinch Pose
		Animator animator;
		[SerializeField] SteamVR_Action_Boolean trackpadTouch;
		[SerializeField] SteamVR_Action_Boolean aButtonTouch;
		[SerializeField] SteamVR_Action_Boolean bButtonTouch;
		[SerializeField] SteamVR_Action_Boolean triggerTouch;
		[SerializeField] SteamVR_Action_Single triggerPull;
		[SerializeField] SteamVR_Action_Boolean grabGrip;
		[SerializeField] SteamVR_Action_Vector2 viveThumbHoriz;

		[SerializeField] SteamVR_ActionSet assistActionset;

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

        private void Awake()
        {
			isPinchingHash = Animator.StringToHash("IsPinching");
			pinchAmtHash = Animator.StringToHash("PinchAmt");
			isGrabbedHash = Animator.StringToHash("IsGrabbed");
			pinchThumbHorizHash = Animator.StringToHash("ThumbHoriz");

            if(!controllerHand) controllerHand = GetComponent<SkeletalControllerHand>();

			animator = steamVRPose.GetComponentInChildren<Animator>();
		}

        private void Start()
        {
            GetMetacarpals();
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

		private void GetControllerType()
		{
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
		}

		bool isPinching;
		bool faceButtonTouch;
		bool isTriggerTouch;

		void UpdateAnimatorKnuckles()
		{
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
		}

		void UpdateAnimatorVive()
		{
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
		}

		private void Update()
        {
			if (controllerHand.IsActive)
			{
				if (steamVRControllerType == SteamVR_ControllerType.Unknown) GetControllerType();
				if (steamVRControllerType == SteamVR_ControllerType.Knuckles) UpdateAnimatorKnuckles();
				else if (steamVRControllerType == SteamVR_ControllerType.Vive) UpdateAnimatorVive();

				animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

				// set our wrist positions to match first
				controllerHand.Wrist.transform.position = wrist.transform.position;

				Quaternion wristBoneOrientation = controllerHand.GetLocalBasis();
				controllerHand.Wrist.rotation = GlobalRotationFromBasis(wrist, wristBasis) * wristBoneOrientation;

				for (int fingerIndx = 0; fingerIndx < fingerMetacarpals.Length; fingerIndx++)
				{
					Transform fingerRoot = null;

					if (fingerIndx == 0) fingerRoot = controllerHand.IndexMetacarpal;
					else if (fingerIndx == 1) fingerRoot = controllerHand.MiddleMetacarpal;
					else if (fingerIndx == 2) fingerRoot = controllerHand.RingMetacarpal;
					else if (fingerIndx == 3) fingerRoot = controllerHand.PinkyMetacarpal;
					else fingerRoot = controllerHand.ThumbMetacarpal;

					MatchBones(fingerMetacarpals[fingerIndx], fingerRoot, fingerBasis, wristBoneOrientation);
				}
			}
			else
			{
				animator.cullingMode = AnimatorCullingMode.CullCompletely;
			}
        }

        public bool IsTracking { get { return steamVRPose.isValid; } }

        void MatchBones(Transform steamVRBone, Transform leapBone, BoneBasis basis,
            Quaternion leapOrientation, int depth =0)
        {
            if (depth == 0) leapBone.transform.position = steamVRBone.transform.position;
            else
            {
                controllerHand.SetTransformWithConstraint(leapBone, steamVRBone.transform.position, GlobalRotationFromBasis(steamVRBone, basis) * leapOrientation);
            }

            if(steamVRBone.childCount == leapBone.childCount)
            {
                if(steamVRBone.childCount == 1)
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

        Quaternion GlobalRotationFromBasis(Transform bone, BoneBasis basis)
        {
            return Quaternion.LookRotation(bone.TransformDirection(basis.Forward),
                bone.TransformDirection(basis.Up));
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

        private void DrawBasis(Transform bone, BoneBasis basis)
        {
            Quaternion rotation = GlobalRotationFromBasis(bone, basis);

            Vector3 up, forward, right;

            up = rotation * Vector3.up;
            forward = rotation * Vector3.forward;
            right = rotation * Vector3.right;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.position, bone.position + (up * 0.025f));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.position, bone.position + (right * 0.025f));
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.position, bone.position + (forward * 0.025f));
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