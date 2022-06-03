using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using CatchCo;
#endif

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

#if ODIN_INSPECTOR
		[Button]
#else
		[ExposeMethodInEditor]
#endif
		void AssignHandsToProvider()
		{
			// update all of the hands 
			foreach (ModelGroup modelGroup in modelGroups)
			{
				modelGroup.LeftModel.leapProvider = _leapProvider;
				modelGroup.RightModel.leapProvider = _leapProvider;
			}
		}

#if ODIN_INSPECTOR
		[Button]
#else
		[ExposeMethodInEditor]
#endif
		void NullProvider()
		{
			foreach(ModelGroup modelGroup in modelGroups)
			{
				modelGroup.LeftModel.leapProvider = null;
				modelGroup.RightModel.leapProvider = null;
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