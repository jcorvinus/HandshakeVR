using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;

using Leap.Unity.Interaction;

namespace HandshakeVR
{
	public class JuicyObjectCreator : EditorWindow
	{
		bool singleObjectMode;
		GameObject selectedObject;

		bool showDefaultAssets = false;

		#region Audio
		AudioClip defaultThrowClip;
		AudioClip defaultImpactClip;
		AudioClip defaultGrabClip;
		AudioClip defaultSlideClip;
		AudioMixerGroup mixerGroup;
		#endregion

		#region Visual Effects
		GameObject smokePrefab;
		Material trailMaterial;
		GameObject impactMangerPrefab;
		ImpactManager impactManager;
		#endregion

		#region Current Object Components
		InteractionBehaviour interaction;
		InteractionSound interactionSound;
		SerializedObject interactionSoundObject;
		SerializedProperty throwSourceProperty;
		AudioSourceProperties throwSourceProperties;

		SerializedProperty impactSourceProperty;
		AudioSourceProperties impactSourceProperties;

		SerializedProperty grabSourceProperty;
		AudioSourceProperties grabSourceProperties;

		SerializedProperty slideSourceProperty;
		AudioSourceProperties slideSourceProperties;

		SlideEffect slideEffect;
		SerializedObject slideEffectObject;
		SerializedProperty slideMaxMagnitudeProperty;
		SerializedProperty slideMaxSoundMagnitudeProperty;
		SerializedProperty slideMaxEmissionRateProperty;
		SerializedProperty slideSoundVolumeProperty;
		SerializedProperty slideParticleProperty;

		GameObject smokeEffect;

		TrailRenderer currentTrailRenderer;
		SerializedObject trailRendererObject;
		SerializedProperty trailTimeProperty;
		SerializedProperty trailMaterialsProperty;
		SerializedProperty trailMinVertexDistProperty;
		SerializedProperty trailWidthDistProperty;
		SerializedProperty trailGradientProperty;
		SerializedProperty enableAirTrailProperty;

		SerializedProperty enableImpactVfxProperty;
		SerializedProperty enableImpactSfxProperty;
		SerializedProperty enableSlideSfx;

		#endregion

		[MenuItem("Window/HandshakeVR/Interactive Object Feedback Editor")]
		static void Init()
		{
			JuicyObjectCreator objectCreator = (JuicyObjectCreator)EditorWindow.GetWindow(typeof(JuicyObjectCreator));
			objectCreator.Show();

			// initialize our assets
			objectCreator.GetDefaultAssets();
		}

		private void GetDefaultAssets()
		{
			defaultThrowClip = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Sound/throw_sound.wav", typeof(AudioClip));
			defaultImpactClip = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Sound/soft_impact_3.wav", typeof(AudioClip));
			defaultGrabClip = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Sound/grab.wav", typeof(AudioClip));
			defaultSlideClip = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Sound/object_slide.wav", typeof(AudioClip));
			AudioMixer mixer = (AudioMixer)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/HandshakeMixer.mixer", typeof(AudioMixer));
			mixerGroup = mixer.FindMatchingGroups("Master")[0];

			smokePrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Prefabs/Smoke.prefab", typeof(GameObject));
			trailMaterial = (Material)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Models/Materials/WindTrail.mat", typeof(Material));
			impactMangerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/HandshakeVR/JuicyObjects/Prefabs/ImpactManager.prefab", typeof(GameObject));
		}

		private void OnGUI()
		{
			if (Selection.gameObjects.Length == 1) DoSingleObject();
			else if (Selection.gameObjects.Length > 1) DoMultiObject();
			else
			{
				EditorGUILayout.LabelField("Select scene gameObject(s) to continue!");
			}

			EditorGUILayout.Space();
			if(GUILayout.Button("Repaint"))
			{
				Repaint();
			}
		}

		private void OnSelectionChange()
		{
			Repaint();
		}

		private void OnFocus()
		{
			Repaint();
		}

		void ChangeSelection(GameObject newSelection)
		{ 
			interaction = newSelection.GetComponent<InteractionBehaviour>();

			// get our interaction sound
			interactionSound = newSelection.GetComponent<InteractionSound>();
			if (interactionSound) GetInteractionSoundObjectAndProperties();
			else
			{
				// clear interaction sound and object properties!
				interactionSoundObject = null;
				throwSourceProperty = null;
				impactSourceProperty = null;
				grabSourceProperty = null;
				slideSourceProperty = null;
				enableImpactSfxProperty = null;
				enableImpactVfxProperty = null;
			}

			// get trail renderer
			currentTrailRenderer = newSelection.GetComponent<TrailRenderer>();
			if (currentTrailRenderer)
			{
				GetTrailObjectAndProperties();
			}
			else
			{
				// clear trail renderer properties
				trailRendererObject = null;
				trailTimeProperty = null;
				trailMaterialsProperty = null;
				trailMinVertexDistProperty = null;
				trailWidthDistProperty = null;
				trailGradientProperty = null;
			}

			// get our sliding effect
			slideEffect = newSelection.GetComponent<SlideEffect>();
			if (slideEffect)
			{
				GetSlideEffectObjectAndProperties();
			}
			else
			{
				// clear slide effect stuff
				slideEffectObject = null;
				slideMaxMagnitudeProperty = null;
				slideMaxSoundMagnitudeProperty = null;
				slideMaxEmissionRateProperty = null;
				slideSoundVolumeProperty = null;
				slideParticleProperty = null;
				enableSlideSfx = null;
				enableAirTrailProperty = null;

			}

			selectedObject = newSelection;
		}

		void GetInteractionSoundObjectAndProperties()
		{
			interactionSoundObject = new SerializedObject(interactionSound);
			throwSourceProperty = interactionSoundObject.FindProperty("throwSource");
			impactSourceProperty = interactionSoundObject.FindProperty("impactSource");
			grabSourceProperty = interactionSoundObject.FindProperty("grabSource");
			slideSourceProperty = interactionSoundObject.FindProperty("slideSource");
			enableImpactSfxProperty = interactionSoundObject.FindProperty("enableImpactSfx");
			enableImpactVfxProperty = interactionSoundObject.FindProperty("enableImpactVfx");
		}

		void GetSlideEffectObjectAndProperties()
		{
			slideEffectObject = new SerializedObject(slideEffect);
			slideMaxMagnitudeProperty = slideEffectObject.FindProperty("maxMagnitude");
			slideMaxSoundMagnitudeProperty = slideEffectObject.FindProperty("maxSoundMagnitude");
			slideMaxEmissionRateProperty = slideEffectObject.FindProperty("maxEmissionRate");
			slideSoundVolumeProperty = slideEffectObject.FindProperty("slideSoundVolume");
			slideParticleProperty = slideEffectObject.FindProperty("particle");
			enableSlideSfx = slideEffectObject.FindProperty("enableSlideSound");
			enableAirTrailProperty = slideEffectObject.FindProperty("enableAirTrail");
		}

		void CreateNew()
		{
			interaction = selectedObject.AddComponent<InteractionBehaviour>();
			CreateInteractionSound();
			CreateSlideEffect();
			CreateTrailRenderer();
		}

		void CreateInteractionSound()
		{
			interactionSound = selectedObject.AddComponent<InteractionSound>();

			// init our serialized object and properties
			GetInteractionSoundObjectAndProperties();

			// create our audio source components. Not sure how we should handle if they exist already
			// for now assume they don't.
			AudioSource impactSource = interactionSound.gameObject.AddComponent<AudioSource>();
			impactSourceProperties = GetAudioSourceProperties(impactSource);

			AudioSource throwSource = interactionSound.gameObject.AddComponent<AudioSource>();
			throwSourceProperties = GetAudioSourceProperties(throwSource);

			AudioSource slideSource = interactionSound.gameObject.AddComponent<AudioSource>();
			slideSourceProperties = GetAudioSourceProperties(slideSource);

			AudioSource grabSource = interactionSound.gameObject.AddComponent<AudioSource>();
			grabSourceProperties = GetAudioSourceProperties(grabSource);

			interactionSoundObject.Update();
			impactSourceProperty.objectReferenceValue = impactSource;
			throwSourceProperty.objectReferenceValue = throwSource;
			slideSourceProperty.objectReferenceValue = slideSource;
			grabSourceProperty.objectReferenceValue = grabSource;
			interactionSoundObject.ApplyModifiedProperties();

			// set our default values
			// impact
			impactSourceProperties.AudioSourceObject.Update();
			impactSourceProperties.ClipProperty.objectReferenceValue = defaultImpactClip;
			impactSourceProperties.MixerGroupProperty.objectReferenceValue = mixerGroup;
			//impactSource.clip = defaultImpactClip;
			impactSourceProperties.LoopProperty.boolValue = false;
			impactSourceProperties.MinDistanceProperty.floatValue = 0.03f;
			impactSourceProperties.MaxDistanceProperty.floatValue = 0.75f;
			impactSourceProperties.PlayOnAwakeProperty.boolValue = false;
			impactSourceProperties.RolloffProperty.enumValueIndex = (int)AudioRolloffMode.Linear;
			impactSourceProperties.AudioSourceObject.ApplyModifiedProperties();
			impactSource.spatialBlend = 1;

			// throw
			throwSourceProperties.AudioSourceObject.Update();
			throwSourceProperties.ClipProperty.objectReferenceValue = defaultThrowClip;
			throwSourceProperties.MixerGroupProperty.objectReferenceValue = mixerGroup;
			throwSourceProperties.LoopProperty.boolValue = false;
			throwSourceProperties.MinDistanceProperty.floatValue = 0.03f;
			throwSourceProperties.MaxDistanceProperty.floatValue = 0.75f;
			throwSourceProperties.PlayOnAwakeProperty.boolValue = false;
			throwSourceProperties.RolloffProperty.enumValueIndex = (int)AudioRolloffMode.Linear;
			throwSourceProperties.AudioSourceObject.ApplyModifiedProperties();
			throwSource.spatialBlend = 0.16f;

			// slide
			slideSourceProperties.AudioSourceObject.Update();
			slideSourceProperties.ClipProperty.objectReferenceValue = defaultSlideClip;
			slideSourceProperties.MixerGroupProperty.objectReferenceValue = mixerGroup;
			slideSourceProperties.LoopProperty.boolValue = true;
			slideSourceProperties.MaxDistanceProperty.floatValue = 2.41f;
			slideSourceProperties.PlayOnAwakeProperty.boolValue = true;
			// do our custom curve keyframe
			slideSourceProperties.RolloffProperty.enumValueIndex = (int)AudioRolloffMode.Custom;
			Keyframe[] curveKeyframes = new Keyframe[2];
			curveKeyframes[0] = new Keyframe(0.0041493773f, 1, -2.6437113f, -2.6437113f);
			curveKeyframes[1] = new Keyframe(1, 0, 0.021675404f, 0.021675404f);
			AnimationCurve rolloffCurve = new AnimationCurve();
			rolloffCurve.AddKey(curveKeyframes[0]);
			rolloffCurve.AddKey(curveKeyframes[1]);
			slideSourceProperties.RolloffCurveProperty.animationCurveValue = rolloffCurve;
			slideSourceProperties.AudioSourceObject.ApplyModifiedProperties();
			slideSource.spatialBlend = 1;

			// grab
			grabSourceProperties.AudioSourceObject.Update();
			grabSourceProperties.ClipProperty.objectReferenceValue = defaultGrabClip;
			grabSourceProperties.MixerGroupProperty.objectReferenceValue = mixerGroup;
			grabSourceProperties.LoopProperty.boolValue = false;
			grabSourceProperties.MinDistanceProperty.floatValue = 0.03f;
			grabSourceProperties.MaxDistanceProperty.floatValue = 0.75f;
			grabSourceProperties.PlayOnAwakeProperty.boolValue = false;
			grabSourceProperties.RolloffProperty.enumValueIndex = (int)AudioRolloffMode.Linear;
			grabSourceProperties.AudioSourceObject.ApplyModifiedProperties();
			grabSource.spatialBlend = 0.354f;
		}

		void GetTrailObjectAndProperties()
		{
			trailRendererObject = new SerializedObject(currentTrailRenderer);
			trailTimeProperty = trailRendererObject.FindProperty("m_Time");
			trailMaterialsProperty = trailRendererObject.FindProperty("m_Materials");
			trailMinVertexDistProperty = trailRendererObject.FindProperty("m_MinVertexDistance");
			trailWidthDistProperty = trailRendererObject.FindProperty("widthMultiplier");
			trailGradientProperty = trailRendererObject.FindProperty("colorGradient");
		}

		void CreateTrailRenderer()
		{
			currentTrailRenderer = selectedObject.gameObject.AddComponent<TrailRenderer>();
			GetTrailObjectAndProperties();

			trailRendererObject.Update();
			trailTimeProperty.floatValue = 0.6f;
			trailMaterialsProperty.arraySize = 1;
			SerializedProperty materialProperty = trailMaterialsProperty.GetArrayElementAtIndex(0);
			materialProperty.objectReferenceValue = trailMaterial;
			trailMinVertexDistProperty.floatValue = 0.06f;

			trailRendererObject.ApplyModifiedProperties();

			currentTrailRenderer.widthMultiplier = 0.039f; // serialized property isn't working.

			Gradient gradient = new Gradient();
			GradientColorKey[] colorKeys = new GradientColorKey[]
			{
				new GradientColorKey(Color.white, 0f),
				new GradientColorKey(Color.white, 1f)
			};

			GradientAlphaKey[] alphaKeys = new GradientAlphaKey[]
			{
				new GradientAlphaKey(1, 0),
				new GradientAlphaKey(0,1)
			};

			gradient.colorKeys = colorKeys;
			gradient.alphaKeys = alphaKeys;

			currentTrailRenderer.colorGradient = gradient; // fuck it we'll do it without serialization backup.

			// let's do our trail width falloff
			currentTrailRenderer.widthCurve = AnimationCurve.Linear(0, 1, 1, 0);
		}

		struct AudioSourceProperties
		{
			public SerializedObject AudioSourceObject;
			public SerializedProperty ClipProperty;
			public SerializedProperty PlayOnAwakeProperty;
			public SerializedProperty MixerGroupProperty;
			public SerializedProperty RolloffProperty;
			public SerializedProperty RolloffCurveProperty;
			public SerializedProperty LoopProperty;
			public SerializedProperty MinDistanceProperty;
			public SerializedProperty MaxDistanceProperty;
		}

		AudioSourceProperties GetAudioSourceProperties(AudioSource source)
		{
			SerializedObject sourceObject = new SerializedObject(source);
			SerializedProperty ClipProperty = sourceObject.FindProperty("m_audioClip");
			SerializedProperty PlayOnAwakeProperty = sourceObject.FindProperty("m_PlayOnAwake");
			SerializedProperty MixerGroupProperty = sourceObject.FindProperty("OutputAudioMixerGroup");
			//SerializedProperty SpatialBlendProperty;
			SerializedProperty RolloffMode = sourceObject.FindProperty("rolloffMode");
			SerializedProperty RolloffCurve = sourceObject.FindProperty("rolloffCustomCurve");
			SerializedProperty LoopProperty = sourceObject.FindProperty("Loop");
			// curve type property goes here?
			SerializedProperty MinDistanceProperty = sourceObject.FindProperty("MinDistance");
			SerializedProperty MaxDistanceProperty = sourceObject.FindProperty("MaxDistance");

			return new AudioSourceProperties()
			{
				AudioSourceObject = sourceObject,
				ClipProperty = ClipProperty, 
				LoopProperty = LoopProperty,
				MaxDistanceProperty = MaxDistanceProperty, 
				MinDistanceProperty = MinDistanceProperty,
				MixerGroupProperty = MixerGroupProperty,
				PlayOnAwakeProperty = PlayOnAwakeProperty,
				RolloffProperty = RolloffMode,
				RolloffCurveProperty = RolloffCurve
			};

			#region Extra Properties
			/*m_AudioClip = ;
			m_PlayOnAwake = ;
			m_Volume = sourceObject.FindProperty("m_Volume");
			m_Pitch = sourceObject.FindProperty("m_Pitch");
			m_Loop = ;
			m_Mute = sourceObject.FindProperty("Mute");
			m_Spatialize = sourceObject.FindProperty("Spatialize");
			m_SpatializePostEffects = sourceObject.FindProperty("SpatializePostEffects");
			m_Priority = sourceObject.FindProperty("Priority");
			m_DopplerLevel = sourceObject.FindProperty("DopplerLevel");
			m_MinDistance = ;
			m_MaxDistance = ;
			m_Pan2D = sourceObject.FindProperty("Pan2D");
			m_RolloffMode = ;
			m_BypassEffects = sourceObject.FindProperty("BypassEffects");
			m_BypassListenerEffects = sourceObject.FindProperty("BypassListenerEffects");
			m_BypassReverbZones = sourceObject.FindProperty("BypassReverbZones");
			m_OutputAudioMixerGroup = ;*/
			#endregion
		}

		void CreateSlideEffect()
		{
			slideEffect = selectedObject.AddComponent<SlideEffect>();
			GetSlideEffectObjectAndProperties();

			// create smoke object
			smokeEffect = GameObject.Instantiate(smokePrefab, selectedObject.transform);

			// set values
			slideEffectObject.Update();
			slideParticleProperty.objectReferenceValue = smokeEffect;
			slideMaxMagnitudeProperty.floatValue = 0.5f;
			slideMaxSoundMagnitudeProperty.floatValue = 1f;
			slideMaxEmissionRateProperty.floatValue = 15;

			// create a normal curve, then tweak it to be exponential.
			AnimationCurve volumeCurve = AnimationCurve.Linear(0, 0, 1, 1);
			volumeCurve.keys[0].outTangent = 0;
			volumeCurve.keys[1].inTangent = -5; // an attempt was made.

			slideSoundVolumeProperty.animationCurveValue = volumeCurve;
			slideEffectObject.ApplyModifiedProperties();
		}

		void DrawInspectorsForDefaultAssets()
		{
			EditorGUILayout.LabelField("Default Assets:");
			EditorGUILayout.ObjectField("defaultThrowClip", defaultThrowClip, typeof(AudioClip), false);
			EditorGUILayout.ObjectField("defaultImpactClip", defaultImpactClip, typeof(AudioClip), false);
			EditorGUILayout.ObjectField("defaultGrabClip", defaultGrabClip, typeof(AudioClip), false);
			EditorGUILayout.ObjectField("defaultSlideClip", defaultSlideClip, typeof(AudioClip), false);

			EditorGUILayout.ObjectField("smoke Prefab", smokePrefab, typeof(GameObject), false);
			EditorGUILayout.ObjectField("trail material", trailMaterial, typeof(Material), false);
			EditorGUILayout.ObjectField("impact manager prefab", impactMangerPrefab, typeof(GameObject), false);

			EditorGUILayout.ObjectField("Mixer group", mixerGroup, typeof(AudioMixerGroup), false);

			if(GUILayout.Button("Re-initialize default assets!"))
			{
				GetDefaultAssets();
			}

			EditorGUILayout.LabelField("------------------------");
			EditorGUILayout.Space();
		}

		void DoSingleObject()
		{
			if (selectedObject == null) selectedObject = Selection.activeGameObject;
			else if (selectedObject.GetInstanceID() != Selection.activeGameObject.GetInstanceID())
			{
				ChangeSelection(Selection.activeGameObject);
			}

			showDefaultAssets = EditorGUILayout.Foldout(showDefaultAssets, "Show default assets");
			if (showDefaultAssets)
			{
				DrawInspectorsForDefaultAssets();
			}

			if (interaction == null)
			{
				if (selectedObject.isStatic)
				{
					EditorGUILayout.HelpBox("Static objects cannot be interaction objects!", MessageType.Error);
				}
				else
				{
					EditorGUILayout.HelpBox("Object does not have an Interaction Behavior. Is this a new object?", MessageType.Info);
					if (GUILayout.Button("Create new and set defaults"))
					{
						CreateNew();
					}
				}
			}
			else
			{

				// find out exactly which capabilities our object has:
				if (interactionSound == null)
				{
					EditorGUILayout.HelpBox("Object does not have an Interaction Sound component. This is needed for the slide, grab, and throw, and impact feedback", MessageType.Warning);

					if (GUILayout.Button("Add Interaction Sound"))
					{
						CreateInteractionSound();
					}
				}
				else
				{
					// slide
					EditorGUILayout.LabelField("Slide effect");
					if(slideEffect)
					{
						if (enableSlideSfx == null) GetSlideEffectObjectAndProperties();

						// enable haptics
						interactionSoundObject.Update();
						slideEffectObject.Update();
						EditorGUILayout.PropertyField(enableSlideSfx); // from slide effect component

						interactionSoundObject.ApplyModifiedProperties();
						slideEffectObject.ApplyModifiedProperties();
					}
					else
					{
						if (GUILayout.Button("Create slide effect"))
						{
							CreateSlideEffect();
						}
					}

					EditorGUILayout.Space();

					// hover/grab/feedback
					EditorGUILayout.LabelField("Hover & Grab Feedback");
					EditorGUILayout.Space();

					// motion trails
					EditorGUILayout.LabelField("In-Air trail effect");
					if(slideEffect)
					{
						// enable/disable in-air trail effect
						slideEffectObject.Update();
						EditorGUILayout.PropertyField(enableAirTrailProperty);
						slideEffectObject.ApplyModifiedProperties();
					}
					else
					{
						EditorGUILayout.HelpBox("Cannot have a motion trail effect without a slide effect.", MessageType.Warning);
						if (GUILayout.Button("Create slide effect"))
						{
							CreateSlideEffect();
						}
					}

					EditorGUILayout.Space();

					// impact
					if (slideEffect)
					{
						if (enableImpactVfxProperty == null)
						{
							GetInteractionSoundObjectAndProperties();
							Debug.Log("Getting Interaction sound properties");
						}
						if (enableSlideSfx == null)
						{
							GetSlideEffectObjectAndProperties();
							Debug.Log("Getting slide effect properties");
						}

						EditorGUILayout.LabelField("Impact effect");

						interactionSoundObject.Update();
						slideEffectObject.Update();
						EditorGUILayout.PropertyField(enableImpactVfxProperty); // from interaction sound component
						EditorGUILayout.PropertyField(enableImpactSfxProperty);
						interactionSoundObject.ApplyModifiedProperties();
						slideEffectObject.ApplyModifiedProperties();
					}
					else
					{
						EditorGUILayout.HelpBox("Cannot have an impact effect without a slide effect.", MessageType.Warning);
						if (GUILayout.Button("Create slide effect"))
						{
							CreateSlideEffect();
						}
					}
				}
			}
		}

		void DoMultiObject()
		{
			EditorGUILayout.HelpBox("ONLY SELECT THE ROOTS OF OBJECTS YOU WANT TO CONVERT.", MessageType.Warning);

			if (GUILayout.Button("Generate new for all eligible objects in selection"))
			{
				GetDefaultAssets();

				foreach (GameObject gameObject in Selection.gameObjects)
				{
					ChangeSelection(gameObject);
					if (interaction == null)
					{
						if (!selectedObject.isStatic)
						{
							CreateNew();
						}
					}
				}
			}
		}
	}
}