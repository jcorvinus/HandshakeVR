using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandshakeVR
{
	public class HandPenetration : MonoBehaviour
	{
		BoxCollider boxCollider;
		CapsuleCollider capsuleCollider;

		Rigidbody rigidBody;

		private Dictionary<int, Collider> overlappingColliders = new Dictionary<int, Collider>(10);
		List<int> removeColliders = new List<int>(10);

		[SerializeField]
		int overlappingColliderLength;

		[SerializeField]
		float penetrationDepth;

		public float MaxPenetrationDepth { get { return penetrationDepth; } }

		public IEnumerable<Collider> Colliders { get { return overlappingColliders.Values; } }

		// Use this for initialization
		void Start()
		{
			boxCollider = GetComponent<BoxCollider>();
			capsuleCollider = GetComponent<CapsuleCollider>();
			rigidBody = GetComponent<Rigidbody>();
		}

		public Vector3 GetGlobalCenter()
		{
			return GetCollider().transform.TransformPoint(GetCenter());
		}

		Vector3 GetCenter()
		{
			if (boxCollider) return boxCollider.center;
			else if (capsuleCollider) return capsuleCollider.center;
			else return Vector3.zero;
		}

		Collider GetCollider()
		{
			if (boxCollider) return boxCollider;
			else if (capsuleCollider) return capsuleCollider;
			else return null;
		}

		// Update is called once per frame
		void FixedUpdate()
		{
			penetrationDepth = 0;

			removeColliders.Clear();

			foreach (int colliderKey in overlappingColliders.Keys)
			{
				if (overlappingColliders[colliderKey] == null) removeColliders.Add(colliderKey);
			}

			foreach (int colliderKey in removeColliders) overlappingColliders.Remove(colliderKey);

			overlappingColliderLength = overlappingColliders.Count;

			foreach (int colliderKey in overlappingColliders.Keys)
			{
				Collider otherCollider = overlappingColliders[colliderKey];

				Vector3 direction = Vector3.zero;
				float distance = 0;

				bool isPenetrating = Physics.ComputePenetration(GetCollider(),
					GetCollider().transform.TransformPoint(GetCenter()), GetCollider().transform.rotation,
					otherCollider, otherCollider.transform.position, otherCollider.transform.rotation,
					out direction, out distance);

				// if ispenetrating && distance > penetrationdepth
				if (isPenetrating && distance > penetrationDepth) penetrationDepth = distance;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.isTrigger) return;
			if (other.tag.Equals("NoTouchSound")) return;

			// todo: make sure we're not adding interaction contact colliders to this list!
			if (!overlappingColliders.ContainsKey(other.GetInstanceID()))
			{
				overlappingColliders.Add(other.GetInstanceID(), other);
			}
		}

		private void OnTriggerExit(Collider other)
		{
			overlappingColliders.Remove(other.GetInstanceID());
		}
	}
}