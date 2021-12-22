using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CatchCo;

using Leap;
using Leap.Unity;

namespace HandshakeVR
{
	[System.Serializable]
	public struct BoneBasis
	{
		public Vector3 Forward;
		public Vector3 Up;
	}

	[System.Serializable]
	public struct BoneData
	{
		public Vector3 Position;
		public Quaternion Rotation;
	}

	public class SkeletalControllerHand : MonoBehaviour
    {
        [System.Serializable]
        public struct BoneConstraint
        {
            public Transform BoneToConstrain;

            [Header("Green")]
            [Range(0, 360)]
            [UnityEngine.Serialization.FormerlySerializedAs("MinAngle")]
            public float StartAngle;

            [Header("Blue")]
            [Range(0, 360)]
            [UnityEngine.Serialization.FormerlySerializedAs("MaxAngle")]
            public float EndAngle;

            public float YHeightCorrection;
        }

		CustomProvider leapProvider;
		public CustomProvider LeapProvider { set { leapProvider = value; } }

        [SerializeField] float palmWidth;
        [SerializeField] float palmForwardOfffset = 0;
        [SerializeField] float palmNormalOffset = 0;

		//[SerializeField] float fingerWidth = 0.016f;
		float[] fingerWidth = new float[]
		{
			0.008f,
			0.008f,
			0.008f,
			0.008f,
			0.008f
		};

        float forearmLength = 0.27f;
		List<Finger> fingers = new List<Finger>(5);

        [SerializeField] Vector3 modelPalmFacing;
        [SerializeField] Vector3 modelFingerPointing;

		HandInputProvider[] inputProviders;
		HandInputProvider activeProvider;
		public HandInputProvider ActiveProvider { get { return activeProvider; } set { activeProvider = value; } }

		float confidence=1;
		float gripStrength=0;
		float pinchStrength = 0;
		int handID=0;

		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }
		public float Confidence { get { return confidence; } set { confidence = value; } }
		public float GripStrength { get { return gripStrength; } set { gripStrength = value; } }
		public float PinchStrength { get { return pinchStrength; } set { pinchStrength = value; } }
		public float[] FingerWidth { get { return fingerWidth; } set { fingerWidth = value; } }

		bool isActive = false;
		public bool IsActive { get { return isActive; } set { isActive = value; } }

		private Leap.Hand previousHand;
		private Leap.Hand leapHand;
		public Leap.Hand LeapHand { get { return leapHand; } }

		Arm arm;
		Bone[] thumbBones;
		Bone[] indexBones;
		Bone[] middleBones;
		Bone[] ringBones;
		Bone[] pinkyBones;

        #region Bone References
        [Header("Bones")]
        [SerializeField] Transform thumbMetaCarpal;
        [SerializeField] Transform indexMetaCarpal;
        [SerializeField] Transform middleMetaCarpal;
        [SerializeField] Transform ringMetaCarpal;
        [SerializeField] Transform pinkyMetaCarpal;

        [SerializeField] Transform wrist;

        public Transform Wrist { get { return wrist; } }

        public Transform IndexMetacarpal { get { return indexMetaCarpal; } }
        public Transform MiddleMetacarpal { get { return middleMetaCarpal; } }
        public Transform RingMetacarpal { get { return ringMetaCarpal; } }
        public Transform PinkyMetacarpal { get { return pinkyMetaCarpal; } }
        public Transform ThumbMetacarpal { get { return thumbMetaCarpal; } }
        #endregion

        [Header("Constraints")]
        [SerializeField] BoneConstraint[] boneConstraints;
        public int ConstraintCount { get { return boneConstraints.Length; } }

        [Header("Debug vars")]
        [SerializeField]
        bool drawBones = true;
        [SerializeField]
        bool drawBasis = true;
        [SerializeField]
        bool drawConstraints = true;
		bool visualizeDirections = false;

        protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };

        public static readonly float MM_TO_M = 1e-3f;

        float visibleTime = 0;
		float[] fingerDots;

		private void Awake()
		{
			fingerDots = new float[5];
			inputProviders = GetComponentsInChildren<HandInputProvider>(true);
		}

		private void Start()
		{
			handID = !isLeft ? 0 : 1;
			leapHand = TestHandFactory.MakeTestHand(IsLeft, 0, handID, TestHandFactory.UnitType.UnityUnits); //GenerateHandData(0);
			arm = leapHand.Arm;
			fingers = leapHand.Fingers;
			thumbBones = leapHand.Fingers[(int)Finger.FingerType.TYPE_THUMB].bones;
			indexBones = leapHand.Fingers[(int)Finger.FingerType.TYPE_INDEX].bones;
			middleBones = leapHand.Fingers[(int)Finger.FingerType.TYPE_MIDDLE].bones;
			ringBones = leapHand.Fingers[(int)Finger.FingerType.TYPE_RING].bones;
			pinkyBones = leapHand.Fingers[(int)Finger.FingerType.TYPE_PINKY].bones;
			previousHand = new Hand();
			previousHand.CopyFrom(leapHand);
			SetHandData(leapHand, 0);
		}

		// Update is called once per frame
		void Update()
        {
			if (isActive)
			{
				visibleTime += Time.deltaTime;
				previousHand.CopyFrom(leapHand);
				SetHandData(leapHand, leapProvider.FrameID);
			}
			else visibleTime = 0;
        }

        void GenerateBones()
        {
            if (thumbMetaCarpal != null) DestroyImmediate(thumbMetaCarpal.gameObject);
            if (indexMetaCarpal != null) DestroyImmediate(indexMetaCarpal.gameObject);
            if (middleMetaCarpal != null) DestroyImmediate(middleMetaCarpal.gameObject);
            if (ringMetaCarpal != null) DestroyImmediate(ringMetaCarpal.gameObject);
            if (pinkyMetaCarpal != null) DestroyImmediate(pinkyMetaCarpal.gameObject);

            #region Metacarpals
            // generate thumb metacarpal
            GameObject thumbObject = new GameObject("ThumbMeta");
            thumbObject.transform.SetParent(this.transform);
            thumbMetaCarpal = thumbObject.transform;

            // generate index metacarpal
            GameObject indexObject = new GameObject("IndexMeta");
            indexObject.transform.SetParent(this.transform);
            indexMetaCarpal = indexObject.transform;

            // generate middle metacarpal
            GameObject middleObject = new GameObject("MiddleMeta");
            middleObject.transform.SetParent(this.transform);
            middleMetaCarpal = middleObject.transform;

            // generate ring metacarpal
            GameObject ringObject = new GameObject("RingMeta");
            ringObject.transform.SetParent(this.transform);
            ringMetaCarpal = ringObject.transform;

            // generate pinky metacarpal
            GameObject pinkyObject = new GameObject("PinkyMeta");
            pinkyObject.transform.SetParent(this.transform);
            pinkyMetaCarpal = pinkyObject.transform;
            #endregion

            // generate child bones
            for(int i=0; i < 5; i++)
            {
                GenerateChildren(GetMetaCarpal((Leap.Finger.FingerType)i),
                    (i == 0)? 2 : 3);
            }
        }

		void SetHandData(Hand hand, int frameID)
		{
			// fill out palm and hand
			// forearm length is 0.27
			// forearm width is 0.09

			Vector forearmStart, forearmEnd;

			forearmStart = GetForearmStart().ToVector();
			forearmEnd = GetForearmEnd().ToVector();

			Quaternion forearmRotation = GetForearmRotation();

			arm.PrevJoint = forearmStart;
			arm.NextJoint = forearmEnd;
			arm.Center = (forearmStart + forearmEnd) * 0.5f;
			arm.Direction = (forearmEnd - forearmStart).Normalized;
			arm.Length = forearmLength;
			arm.Width = 0.09f;
			arm.Rotation = forearmRotation.ToLeapQuaternion();

			Vector palmPosition = GetPalmPosition().ToVector();
			Vector palmNormal = GetPalmNormal().ToVector();
			Vector palmVelocity = (visibleTime != 0 && 
				(visibleTime - previousHand.TimeVisible != 0)) ? (previousHand.PalmPosition - palmPosition).Normalized / (visibleTime - previousHand.TimeVisible) :
				new Vector(0, 0, 0);
			Vector handDirection = wrist.TransformDirection(modelFingerPointing).ToVector();

			LeapQuaternion rotation = GetHandRotation().ToLeapQuaternion();

			fingers = hand.Fingers;

			// fingers
			for (int fingerIndex = 0; fingerIndex < 5; fingerIndex++)
			{
				Leap.Finger.FingerType fingerType = (Leap.Finger.FingerType)fingerIndex;

				Transform metaCarpalTransform = null;

				// set up our bones
				Bone[] bones = null;

				switch (fingerType)
				{
					case Finger.FingerType.TYPE_THUMB:
						metaCarpalTransform = thumbMetaCarpal;
						bones = thumbBones;
						break;
					case Finger.FingerType.TYPE_INDEX:
						metaCarpalTransform = indexMetaCarpal;
						bones = indexBones;
						break;
					case Finger.FingerType.TYPE_MIDDLE:
						metaCarpalTransform = middleMetaCarpal;
						bones = middleBones;
						break;
					case Finger.FingerType.TYPE_RING:
						metaCarpalTransform = ringMetaCarpal;
						bones = ringBones;
						break;
					case Finger.FingerType.TYPE_PINKY:
						metaCarpalTransform = pinkyMetaCarpal;
						bones = pinkyBones;
						break;
					default:
						Debug.LogError("Invalid finger type for finger index: " + fingerIndex);
						break;
				}

				// NOTE: if our type is thumb, our 'meta carpal transform' is actually our proximal,
				// and we'll need to generate a zero-length metacarpal for it
				Transform proximalTransform = (fingerType == Finger.FingerType.TYPE_THUMB) ? metaCarpalTransform : metaCarpalTransform.GetChild(0);
				Transform intermediateTransform = proximalTransform.GetChild(0);
				Transform distalTransform = intermediateTransform.GetChild(0);
				Transform tip = distalTransform.GetChild(0);
				Vector tipPosition = tip.transform.position.ToVector();
				Vector direction = distalTransform.forward.ToVector();

				float fingerLength =
					Vector3.Distance(proximalTransform.position, intermediateTransform.position) +
					Vector3.Distance(intermediateTransform.position, distalTransform.position) +
					Vector3.Distance(distalTransform.position, tip.position); // add up joint lengths for this
				float fingerDot = Vector3.Dot(direction.ToVector3(), handDirection.ToVector3());
				fingerDots[fingerIndex] = fingerDot;
				bool isExtended = Mathf.Abs(fingerDot) > 0.5f;

				SetBone(bones[(int)Bone.BoneType.TYPE_METACARPAL], metaCarpalTransform, proximalTransform, Bone.BoneType.TYPE_METACARPAL, fingerWidth[fingerIndex]);
				SetBone(bones[(int)Bone.BoneType.TYPE_PROXIMAL], proximalTransform, intermediateTransform, Bone.BoneType.TYPE_PROXIMAL, fingerWidth[fingerIndex]);
				SetBone(bones[(int)Bone.BoneType.TYPE_INTERMEDIATE], intermediateTransform, distalTransform, Bone.BoneType.TYPE_INTERMEDIATE, fingerWidth[fingerIndex]);
				SetBone(bones[(int)Bone.BoneType.TYPE_DISTAL], distalTransform, tip, Bone.BoneType.TYPE_DISTAL, fingerWidth[fingerIndex]);

				fingers[fingerIndex].bones = bones;
				fingers[fingerIndex].HandId = handID;
				fingers[fingerIndex].TimeVisible = visibleTime;
				fingers[fingerIndex].TipPosition = tipPosition;
				fingers[fingerIndex].Direction = direction;
				fingers[fingerIndex].Width = fingerWidth[fingerIndex];
				fingers[fingerIndex].Length = fingerLength;
				fingers[fingerIndex].IsExtended = isExtended;
				fingers[fingerIndex].Type = (Finger.FingerType)fingerIndex;
				fingers[fingerIndex].Id = fingerIndex;
				fingers[fingerIndex].HandId = handID;
			}

			hand.FrameId = frameID;
			hand.PalmPosition = palmPosition;
			hand.PalmVelocity = palmVelocity;
			hand.PalmNormal = palmNormal;
			hand.Direction = handDirection;
			hand.Rotation = rotation;
			hand.PalmWidth = palmWidth;
			hand.StabilizedPalmPosition = palmPosition;
			hand.WristPosition = wrist.position.ToVector();
			hand.GrabAngle = 0;
			hand.GrabStrength = gripStrength;
			hand.TimeVisible = visibleTime;
			hand.Confidence = confidence;
			hand.PinchStrength = pinchStrength;
			hand.PinchDistance = (fingers[(int)Finger.FingerType.TYPE_INDEX].TipPosition.ToVector3() -
				fingers[(int)Finger.FingerType.TYPE_THUMB].TipPosition.ToVector3()).magnitude;
			hand.Arm = arm;
			hand.Fingers = fingers;
			hand.IsLeft = isLeft;
		}

		private void SetBone(Bone bone, Transform prev, Transform next, Bone.BoneType type, 
			float fingerWidth)
		{
			Vector3 up, forward, right;
			GetBasis(prev, out right, out forward, out up);
			float boneDist = Vector3.Distance(prev.position, next.position);
			Vector3 boneCenter = (prev.position + next.position) * 0.5f;

			bone.PrevJoint = prev.position.ToVector();
			bone.NextJoint = next.position.ToVector();
			bone.Center = boneCenter.ToVector();
			bone.Direction = forward.ToVector();
			bone.Length = boneDist;
			bone.Width = fingerWidth;
			bone.Type = type;
			bone.Rotation = Quaternion.LookRotation(forward, up).ToLeapQuaternion();
		}

		private void GetBasis(Transform reference, out Vector3 right, out Vector3 forward, out Vector3 up)
        {
            forward = modelFingerPointing;
            up = modelPalmFacing * -1;
            right = Vector3.Cross(forward, up);

            forward = reference.TransformDirection(forward);
            up = reference.TransformDirection(up);
            right = reference.TransformDirection(right);
        }

        public Quaternion GetLocalBasis()
        {
            Vector3 forward = modelFingerPointing;
            Vector3 up = modelPalmFacing * -1;
            return Quaternion.LookRotation(forward, up);
        }

        private void GenerateChildren(Transform parent, int childCount)
        {
            Transform prevTransform = parent;

            for(int i=0; i < parent.childCount;i++)
            {
                // clear any existing children
                DestroyImmediate(parent.GetChild(i).gameObject);
            }

            for(int i=0; i < childCount; i++)
            {
                GameObject newBone = new GameObject("Bone");
                newBone.transform.SetParent(prevTransform);
                prevTransform = newBone.transform;
            }
        }

        public Transform GetMetaCarpal(Leap.Finger.FingerType type)
        {
            switch (type)
            {
                case Finger.FingerType.TYPE_THUMB:
                    return thumbMetaCarpal;

                case Finger.FingerType.TYPE_INDEX:
                    return indexMetaCarpal;

                case Finger.FingerType.TYPE_MIDDLE:
                    return middleMetaCarpal;

                case Finger.FingerType.TYPE_RING:
                    return ringMetaCarpal;

                case Finger.FingerType.TYPE_PINKY:
                    return pinkyMetaCarpal;
                default:
                    return null;
            }
        }

        public Vector3 GetPalmPosition()
        {
            return wrist.position + (wrist.TransformDirection(modelPalmFacing) * palmNormalOffset) +
                (wrist.TransformDirection(modelFingerPointing) * palmForwardOfffset);
        }

        public Vector3 GetPalmNormal()
        {
            return wrist.transform.TransformDirection(modelPalmFacing);
        }

        private Vector3 GetForearmStart()
        {
            return (wrist.transform.position + wrist.TransformDirection(-modelFingerPointing) * forearmLength * 0.5f);
        }

        private Vector3 GetForearmEnd()
        {
            return wrist.transform.position;
        }

        public Quaternion GetHandRotation()
        {
            Vector3 upForearm, forwardForearm;

            upForearm = wrist.TransformDirection(-modelPalmFacing);
            forwardForearm = wrist.TransformDirection(modelFingerPointing);

            return Quaternion.LookRotation(forwardForearm, upForearm);
        }

        private Quaternion GetForearmRotation()
        {
            Vector3 upForearm, forwardForearm;

            upForearm = wrist.TransformDirection(-modelPalmFacing);
            forwardForearm = wrist.TransformDirection(modelFingerPointing);

            return Quaternion.LookRotation(forwardForearm, upForearm);
        }

        Quaternion InverseTransformQuaternion(Transform reference, Quaternion worldRoation)
        {
            return Quaternion.Inverse(reference.transform.rotation) * worldRoation;
        }

        public BoneConstraint GetConstraintAtIndex(int index)
        {
            return boneConstraints[index];
        }

		bool AnyBoneConstraint(int instanceID)
		{
			for(int i=0; i < boneConstraints.Length; i++)
			{
				BoneConstraint constraint = boneConstraints[i];
				if (constraint.BoneToConstrain.GetInstanceID() == instanceID) return true;
			}
			return false;
		}

		BoneConstraint First(int instanceID)
		{
			for (int i = 0; i < boneConstraints.Length; i++)
			{
				BoneConstraint constraint = boneConstraints[i];
				if (constraint.BoneToConstrain.GetInstanceID() == instanceID) return constraint;
			}
			return new BoneConstraint();
		}

        public void SetTransformWithConstraint(Transform bone, Vector3 position, Quaternion globalRotation)
        {
			if (AnyBoneConstraint(bone.GetInstanceID()))
            {
                // get our constraint object
                BoneConstraint constraint = First(bone.GetInstanceID());

                SetTransformWithConstraint(constraint, bone, position, globalRotation);
            }
            else
            {
                // just apply without constraint
                bone.transform.SetPositionAndRotation(position, globalRotation);
            }
        }

        private void SetTransformWithConstraint(BoneConstraint constraint, Transform bone, Vector3 position,
            Quaternion globalRotation)
        {
            constraint.BoneToConstrain.position = position;
            constraint.BoneToConstrain.localPosition = new Vector3(constraint.BoneToConstrain.localPosition.x, 
                constraint.YHeightCorrection, 0);

            Quaternion localRotation = InverseTransformQuaternion(bone.transform.parent,
                globalRotation);

            Vector3 localEuler = localRotation.eulerAngles;

            float startAngle = constraint.StartAngle, endAngle = constraint.EndAngle;
            float maxAngle, minAngle;

            maxAngle = (startAngle > endAngle) ? startAngle : endAngle;
            minAngle = (startAngle > endAngle) ? endAngle : startAngle;

            float distToMax = distToMax = Mathf.DeltaAngle(localEuler.z, maxAngle);
            float distToMin = distToMin = Mathf.DeltaAngle(localEuler.z, minAngle);

            if (localEuler.z > maxAngle || localEuler.z < minAngle)
            {
                // move euler.z to closest angle
                localEuler.z = (Mathf.Abs(distToMax) < Mathf.Abs(distToMin)) ? maxAngle : minAngle;
            }

            constraint.BoneToConstrain.localRotation = Quaternion.Euler(new Vector3(0, 0, localEuler.z));
        }

		void DrawBones(Transform parent, Transform child, int boneIndx)
        {
            Gizmos.DrawLine(parent.transform.position, child.transform.position);

            Gizmos.color = colors[boneIndx];

            if (drawBasis) DrawBasis(parent);

            if(child.childCount > 0)
            {
                DrawBones(child, child.GetChild(0), boneIndx + 1);
            }
        }

        void DrawBasis(Transform bone)
        {
            Vector3 forward = modelFingerPointing;
            Vector3 up = modelPalmFacing * -1;
            Vector3 right = Vector3.Cross(forward, up);

            Color storedColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(right) * 0.01f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(up) * 0.01f);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(bone.transform.position, bone.transform.position + bone.transform.TransformDirection(forward) * 0.01f);

            Gizmos.color = storedColor;
        }

        void DrawHand()
        {
            for (int fingerIndx = 0; fingerIndx < 5; fingerIndx++)
            {
                Transform metaCarpal = GetMetaCarpal((Finger.FingerType)fingerIndx);

                DrawBones(metaCarpal, metaCarpal.GetChild(0), 0);
            }
        }

        private void DrawConstraint(BoneConstraint constraint)
        {
            if (constraint.BoneToConstrain == null || constraint.BoneToConstrain.parent == null) return;

            Gizmos.matrix = constraint.BoneToConstrain.parent.localToWorldMatrix;

            // draw line to minAngle
            Gizmos.color = Color.green;
            Vector3 minPointRotated = Quaternion.AngleAxis(constraint.StartAngle, Vector3.forward) * Vector3.right;
            Gizmos.DrawLine(constraint.BoneToConstrain.localPosition, constraint.BoneToConstrain.localPosition + minPointRotated * 0.01f);

            // draw line to maxAngle
            Gizmos.color = Color.blue;
            Vector3 maxPointRotated = Quaternion.AngleAxis(constraint.EndAngle, Vector3.forward) * Vector3.right;
            Gizmos.DrawLine(constraint.BoneToConstrain.localPosition, constraint.BoneToConstrain.localPosition + maxPointRotated * 0.01f);

            Gizmos.matrix = Matrix4x4.identity;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(constraint.BoneToConstrain.position, constraint.BoneToConstrain.position + constraint.BoneToConstrain.right * 0.015f);
        }

        private void OnDrawGizmos()
        {
            // draw hand
            if (drawBones)
            {
                DrawHand();

                // draw palm width
                Gizmos.DrawWireCube(GetPalmPosition(), Vector3.right * palmWidth);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(wrist.transform.position, GetPalmPosition());
                Gizmos.color = Color.black;
                Gizmos.DrawLine(GetPalmPosition(), GetPalmPosition() + wrist.TransformDirection(modelPalmFacing) * 0.03f);

                // draw forearm
                Gizmos.color = Color.red;
                Gizmos.DrawLine(GetForearmStart(), GetForearmEnd());

                if(drawBasis)
                {
                    Quaternion forearmRotation = GetForearmRotation();
                    Vector3 forearmUp = forearmRotation * Vector3.up;
                    Vector3 forearmForward = forearmRotation * Vector3.forward;
                    Vector3 forearmRight = forearmRotation * Vector3.right;

                    Vector3 forearmCenter = (GetForearmStart() + GetForearmEnd()) * 0.5f;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmForward * 0.01f);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmUp * 0.01f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(forearmCenter, forearmCenter + forearmRight * 0.01f);
                }
            }

			if (visualizeDirections)
			{
				// draw the hand direction
				Debug.DrawLine(GetPalmPosition(),
					(GetPalmPosition() + (wrist.TransformVector(modelPalmFacing) * 0.1f)));

				for (int f = 0; f < 5; f++)
				{
					Vector3 handDirection = wrist.TransformDirection(modelFingerPointing);
					Finger.FingerType fingerType = (Finger.FingerType)f;
					Transform metacarpal = GetMetaCarpal(fingerType);

					Transform proximalTransform = (fingerType == Finger.FingerType.TYPE_THUMB) ? metacarpal : metacarpal.GetChild(0);
					Transform intermediateTransform = proximalTransform.GetChild(0);
					Transform distalTransform = intermediateTransform.GetChild(0);
					Transform tip = distalTransform.GetChild(0);
					Vector3 forward = tip.TransformVector(modelFingerPointing);

					float fingerDot = Vector3.Dot(forward, handDirection);
					if (fingerDots == null || fingerDots.Length != 5) fingerDots = new float[5];
					fingerDots[f] = fingerDot;
					bool isExtended = Mathf.Abs(fingerDot) > 0.5f;

					Debug.DrawLine(tip.position,
						tip.position + (forward * 0.1f), isExtended ? Color.green : Color.red);
				}
			}

			if (drawConstraints)
            {
                if (boneConstraints != null)
                {
                    foreach (BoneConstraint constraint in boneConstraints)
                    {
                        DrawConstraint(constraint);
                    }
                }
            }
        }
    }
}