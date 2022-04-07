using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

namespace HandshakeVR
{
	public class HandModelManager : MonoBehaviour
	{
		private LeapProvider _leapProvider;
		public LeapProvider leapProvider
		{
			get
			{
				return _leapProvider;
			}

			set
			{
				_leapProvider = value;
				AssignHandsToProvider();
			}
		}

		[System.Serializable]
		public struct ModelGroup
		{
			public string GroupName;
			public HandModelBase LeftModel;
			public OverridableHandEnableDisable LeftEnabler;

			public HandModelBase RightModel;
			public OverridableHandEnableDisable RightEnabler;
		}

		[SerializeField]
		List<ModelGroup> modelGroups;

		void AssignHandsToProvider()
		{
			// update all of the hands 
			foreach (ModelGroup modelGroup in modelGroups)
			{
				modelGroup.LeftModel.leapProvider = _leapProvider;
				modelGroup.RightModel.leapProvider = _leapProvider;
			}
		}

		public void AddNewGroup(ModelGroup group)
		{
			modelGroups.Add(group);
		}

		public void RemoveGroup(ModelGroup group)
		{
			modelGroups.Remove(group);
		}

		public int GetNumberOfGroups()
		{
			return modelGroups.Count;
		}

		public ModelGroup GetModelGroupAtIndex(int i)
		{
			return modelGroups[i];
		}

		private void Awake()
		{
			AssignHandsToProvider();
		}
	}
}