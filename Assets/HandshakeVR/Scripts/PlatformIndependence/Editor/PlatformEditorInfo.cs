using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HandshakeVR
{
	[CustomEditor(typeof(PlatformInfo))]
	public class PlatformEditorInfo : Editor
	{
		PlatformInfo m_instance;
		SerializedProperty platformIDProperty;
		SerializedProperty useHandshakeProperty;

		private void OnEnable()
		{
			GetComponents();
		}

		private void Start()
		{
			GetComponents();
		}

		void GetComponents()
		{
			platformIDProperty = serializedObject.FindProperty("platformID");
			useHandshakeProperty = serializedObject.FindProperty("useHandshakeMultiplatform");
			m_instance = target as PlatformInfo;
		}

		public static void SetPlatformSettings(PlatformID platform)
		{
            // set our compile symbols properly
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
			List<string> allDefines = new List<string>(definesString.Split(';'));

			allDefines.Remove(PlatformInfo.HANDSHAKE_NONE);
			allDefines.Remove(PlatformInfo.HANDSHAKE_OCULUS);
			allDefines.Remove(PlatformInfo.HANDSHAKE_STEAMVR);

			switch (platform)
			{
				case PlatformID.None:
					allDefines.Add(PlatformInfo.HANDSHAKE_NONE);
					break;
				case PlatformID.SteamVR:
					allDefines.Add(PlatformInfo.HANDSHAKE_STEAMVR);
					break;
				case PlatformID.Oculus:
					allDefines.Add(PlatformInfo.HANDSHAKE_OCULUS);
					break;
				default:
					break;
			}

			PlayerSettings.SetScriptingDefineSymbolsForGroup(
				EditorUserBuildSettings.selectedBuildTargetGroup,
				string.Join(";", allDefines.ToArray()));
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			/*serializedObject.Update();

			// get our platform ID
			PlatformID platform = m_instance.PlatformID;

            EditorGUILayout.PropertyField(platformIDProperty);

            PlatformID newPlatform = m_instance.PlatformID;

            if(platform != newPlatform)
            {
                Debug.Log(newPlatform.ToString());
            }

            // if there are changes, and the flag says to use Handshake's multiplatform system,
            // set the compile flags accordingly.
            if (platform != newPlatform && useHandshakeProperty.boolValue)
			{
                SetPlatformSettings(newPlatform);
			}

            bool useHandshakeMultiplat = useHandshakeProperty.boolValue;

            EditorGUILayout.PropertyField(useHandshakeProperty);

            if(!useHandshakeMultiplat && useHandshakeProperty.boolValue)
            {
                SetPlatformSettings(newPlatform);
            }

			// check platform ID against player build target
			BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

			switch(buildTarget)
			{
				case (BuildTarget.Android):
					switch(newPlatform)
					{
						case (PlatformID.SteamVR):
							EditorGUILayout.HelpBox("SteamVR does not work on Android.", MessageType.Info);
							break;
					}
					break;

				case (BuildTarget.StandaloneWindows):
				case (BuildTarget.StandaloneWindows64):

					break;

				default:
					EditorGUILayout.HelpBox("Build Target not supported by Handshake", MessageType.Error);
					break;
			}

			serializedObject.ApplyModifiedProperties();*/
		}
	}
}