using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace HandshakeVR
{
	public class PlatformPreferences : MonoBehaviour
	{
		private static bool prefsLoaded = false;

		private static int currentPlatform=0;

		[PreferenceItem("HandshakeVR")]
		public static void PreferencesGUI()
		{
			// load the preferences
			if(!prefsLoaded)
			{
				// get all the values
				currentPlatform = EditorPrefs.GetInt("HandshakePlatformID");
			}

			// show the GUI
			PlatformID platform = (PlatformID)EditorGUILayout.EnumPopup((PlatformID)currentPlatform);
			if(currentPlatform != (int) platform)
			{
				// we have a change on our hands
				currentPlatform = (int)platform;
			}

			// get our warnings
			switch (EditorUserBuildSettings.activeBuildTarget)
			{
				case BuildTarget.StandaloneOSX:
					EditorGUILayout.HelpBox(string.Format("Handshake does not support build target {0}", EditorUserBuildSettings.activeBuildTarget.ToString()),
						MessageType.Error);
					break;

				case BuildTarget.StandaloneWindows:
					// check to see what our 
					break;

				case BuildTarget.iOS:
					break;

				case BuildTarget.Android:
					break;

				case BuildTarget.NoTarget:
					EditorGUILayout.HelpBox("No build target set. Are you debugging?", MessageType.Warning);
					break;

				default:
					EditorGUILayout.HelpBox(string.Format("Handshake does not support build target {0}", EditorUserBuildSettings.activeBuildTarget.ToString()),
						MessageType.Error);
					break;
			}
		}
	}
}