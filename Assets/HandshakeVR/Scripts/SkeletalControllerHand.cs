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

        [SerializeField] float fingerWidth = 8;

        float forearmLength = 0.27f;

        [SerializeField] Vector3 modelPalmFacing;
        [SerializeField] Vector3 modelFingerPointing;

		float timeVisible = 0;
		int handID=0;

		[SerializeField] bool isLeft;
		public bool IsLeft { get { return isLeft; } }

		bool isActive = false;
		public bool IsActive { get { return isActive; } set { isActive = value; } }

		private Leap.Hand leapHand;
		public Leap.Hand LeapHand { get { return leapHand; } }

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

        protected Color[] colors = { Color.gray, Color.yellow, Color.cyan, Color.magenta };

        public static readonly float MM_TO_M = 1e-3f;

        float visibleTime = 0;

		private void Start()
		{
			handID = !isLeft ? 0 : 1;
			leapHand = TestHandFactory.MakeTestHand(IsLeft, 0, handID, TestHandFactory.UnitType.UnityUnits); //GenerateHandData(0);
			SetHandData(leapHand, 0);
		}

		// Update is called once per frame
		void Update()
        {
			if (isActive)
			{
				visibleTime += Time.deltaTime;
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
			for (int fingerIndex = 0; fingerIndex < hand.Fingers.Count; fingerIndex++)
			{
				Leap.Finger.FingerType fingerType = (Leap.Finger.FingerType)fingerIndex;
				float _fingerWidth = fingerWidth * MM_TO_M;

				Transform metaCarpalTransform = null;

				switch (fingerType)
				{
					case Finger.FingerType.TYPE_THUMB:
						metaCarpalTransform = thumbMetaCarpal;
						break;
					case Finger.FingerType.TYPE_INDEX:
						metaCarpalTransform = indexMetaCarpal;
						break;
					case Finger.FingerType.TYPE_MIDDLE:
						metaCarpalTransform = middleMetaCarpal;
						break;
					case Finger.FingerType.TYPE_RING:
						metaCarpalTransform = ringMetaCarpal;
						break;
					case Finger.FingerType.TYPE_PINKY:
						metaCarpalTransform = pinkyMetaCarpal;
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

				Finger finger = hand.Fingers[fingerIndex];
				SetBone(ref finger.bones[(int)Bone.BoneType.TYPE_METACARPAL], metaCarpalTransform, proximalTransform, Bone.BoneType.TYPE_METACARPAL, _fingerWidth);
				SetBone(ref finger.bones[(int)Bone.BoneType.TYPE_PROXIMAL], proximalTransform, intermediateTransform, Bone.BoneType.TYPE_PROXIMAL, _fingerWidth);
				SetBone(ref finger.bones[(int)Bone.BoneType.TYPE_INTERMEDIATE], intermediateTransform, distalTransform, Bone.BoneType.TYPE_INTERMEDIATE, _fingerWidth);
				SetBone(ref finger.bones[(int)Bone.BoneType.TYPE_DISTAL], distalTransform, tip, Bone.BoneType.TYPE_DISTAL, _fingerWidth);

				// update the rest of the finger values.
				Vector tipPosition = tip.transform.position.ToVector();
				Vector direction = new Vector(0, 0, 0);

				float fingerLength =
					Vector3.Distance(proximalTransform.position, intermediateTransform.position) +
					Vector3.Distance(intermediateTransform.position, distalTransform.position) +
					Vector3.Distance(distalTransform.position, tip.position); // add up joint lengths for this

				hand.Fingers[fingerIndex].Id = (int)fingerType;
				hand.Fingers[fingerIndex].HandId = handID;
				hand.Fingers[fingerIndex].TipPosition = tipPosition;
				hand.Fingers[fingerIndex].Direction = direction;
				hand.Fingers[fingerIndex].Length = fingerLength;
			}

			// fill out rest of hand
			// forearm length is 0.27
			// forearm width is 0.09

			Vector forearmStart, forearmEnd;

			forearmStart = GetForearmStart().ToVector();
			forearmEnd = GetForearmEnd().ToVector();

			Quaternion forearmRotation = GetForearmRotation();

			// might be possible to create this as a bone? Not sure what magic the constructor does.
			/*Arm arm = new Arm(forearmStart, forearmEnd, (forearmStart + forearmEnd) * 0.5f,
				(forearmEnd - forearmStart).Normalized, forearmLength, 0.09f,
				forearmRotation.ToLeapQuaternion());*/

			hand.Arm.PrevJoint = forearmStart;
			hand.Arm.NextJoint = forearmEnd;
			hand.Arm.Center = (forearmStart + forearmEnd) * 0.5f;
			hand.Arm.Direction = (forearmEnd - forearmStart).Normalized;
			hand.Arm.Length = forearmLength;
			hand.Arm.Width = 0.09f;
			hand.Arm.Rotation = forearmRotation.ToLeapQuaternion();

			Vector palmPosition = GetPalmPosition().ToVector();
			Vector palmNormal = GetPalmNormal().ToVector();
			Vector palmVelocity = new Vector(0, 0, 0);

			//palmWidth = 85f * MM_TO_M;

			LeapQuaternion rotation = GetHandRotation().ToLeapQuaternion();

			hand.FrameId = frameID;
			hand.PalmPosition = palmPosition;
			hand.PalmVelocity = palmVelocity;
			hand.PalmNormal = palmNormal;
			hand.Direction = wrist.TransformDirection(modelPalmFacing).ToVector();
			hand.Rotation = rotation;
			hand.PalmWidth = palmWidth;
			hand.StabilizedPalmPosition = palmPosition;
			hand.WristPosition = wrist.position.ToVector();
			hand.TimeVisible = timeVisible;
		}

		private void SetBone(ref Bone bone, Transform prev, Transform next, Bone.BoneType type, float fingerWidth)
		{
			Vector3 up, forward, right;
			GetBasis(prev, out right, out forward, out up);
			float metaDist = Vector3.Distance(prev.position, next.position);
			Vector3 metaCenter = (prev.position + next.position) * 0.5f;

			bone.PrevJoint = prev.position.ToVector();
			bone.NextJoint = next.position.ToVector();
			bone.Center = metaCenter.ToVector();
			bone.Direction = forward.ToVector();
			bone.Length = metaDist;
			bone.Width = fingerWidth;
			bone.Type = type;
			bone.Rotation = Quaternion.LookRotation(forward, up).ToLeapQuaternion();

			/*new Bone(prev.position.ToVector(), next.position.ToVector(),
				metaCenter.ToVector(), forward.ToVector(), metaDist, fingerWidth,
				type, Quaternion.LookRotation(forward, up).ToLeapQuaternion());*/
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

        public void SetTransformWithConstraint(Transform bone, Vector3 position, Quaternion globalRotation)
        {
            if(boneConstraints.Any(item => item.BoneToConstrain.GetInstanceID() == bone.GetInstanceID()))
            {
                // get our constraint object
                BoneConstraint constraint = boneConstraints.First(item => item.BoneToConstrain.GetInstanceID() ==
                bone.GetInstanceID());

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

            if(drawConstraints)
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