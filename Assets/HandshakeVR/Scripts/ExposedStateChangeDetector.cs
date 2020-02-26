using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Leap.Unity;

namespace HandshakeVR
{
	/// <summary>
	/// Simple extension of the Detector class that exposes 
	/// whether or not a state change happened this frame.
	/// </summary>
	public class ExposedStateChangeDetector : Detector
	{
		private bool activatedPreviousFrame = false;
		private bool activatedThisFrame = false;

		private bool deactivatedPreviousFrame = false;
		private bool deactivatedThisFrame = false;

		public bool ActivatedThisFrame { get { return activatedThisFrame; } }
		public bool DeactivatedThisFrame { get { return deactivatedThisFrame; } }

		private void Start()
		{
			OnActivate.AddListener(() => { activatedThisFrame = true; });
			OnDeactivate.AddListener(() => { deactivatedThisFrame = true; });
		}

		private void Update()
		{
			activatedPreviousFrame = IsActive;
			deactivatedPreviousFrame = !IsActive;
		}

		private void LateUpdate()
		{
			activatedPreviousFrame = (IsActive && !activatedPreviousFrame);
			deactivatedPreviousFrame = (!IsActive && deactivatedPreviousFrame);
		}
	}
}