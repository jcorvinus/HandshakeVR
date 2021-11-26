using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

namespace HandshakeVR
{
	public class OverridableHandEnableDisable : HandTransitionBehavior
	{
		private HandModelBase handBase;
		private bool isDisabled = false;

		public bool IsDisabled
		{
			get { return isDisabled; }
			set
			{
				isDisabled = value;
				if (!isDisabled && handModelBase.IsTracked)
				{ gameObject.SetActive(true); }
				else { gameObject.SetActive(false); }
			}
		}

		protected override void Awake()
		{
			base.Awake();
			handModelBase = GetComponent<HandModelBase>();

			gameObject.SetActive(false);
		}

		protected override void HandReset()
		{
			if(!isDisabled) gameObject.SetActive(true);
		}

		protected override void HandFinish()
		{
			gameObject.SetActive(false);
		}
	}
}