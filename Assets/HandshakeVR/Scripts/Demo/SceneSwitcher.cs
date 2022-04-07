using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_STANDALONE
using Valve.VR;
#endif

namespace HandshakeVR
{
    public class SceneSwitcher : MonoBehaviour
    {
        static SceneSwitcher instance;
        public static SceneSwitcher Instance { get { return instance; } }

		string sceneChangeActionName = "actions/default/in/SceneChange";
#if UNITY_STANDALONE
		SteamVR_Action_Boolean sceneChangeAction;
#endif

		string[] sceneList = new string[]
        {
            "SteamVRTest",
            "HandshakeVR/Scenes/Interaction/Basic Interactions",
			"HandshakeVR/Scenes/Interaction/Basic UI",
			"HandshakeVR/Scenes/GraphicRenderer/CurvedUI",
			"HandshakeVR/Scenes/Interaction/Dynamic UI",
			"HandshakeVR/Scenes/Interaction/Hand UI",
			"HandshakeVR/Scenes/Interaction/Anchors",
			"HandshakeVR/Scenes/Interaction/Interaction Callbacks",
			"HandshakeVR/Scenes/Interaction/Moving Reference Frame",
			"HandshakeVR/Scenes/Interaction/SwapGrasp"
		};

        int currentSceneIndex = -1;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);

#if UNITY_STANDALONE
				sceneChangeAction = SteamVR_Input.GetBooleanAction(sceneChangeActionName);
#endif
			}
			else Destroy(gameObject);
        }

        // Use this for initialization
        void Start()
        {
            Scene scene = SceneManager.GetActiveScene();

            currentSceneIndex = scene.buildIndex;

#if UNITY_STANDALONE
			sceneChangeAction.onStateDown += SceneChangeAction_Invoke;
#endif
		}

#if UNITY_STANDALONE
		void SceneChangeAction_Invoke(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
		{
			if (fromSource == SteamVR_Input_Sources.RightHand) NextScene();
			else PreviousScene();
		}
#endif

		void NextScene()
		{
			currentSceneIndex++;
			currentSceneIndex = (int)Mathf.Repeat(currentSceneIndex, sceneList.Length);
			SceneManager.LoadScene(currentSceneIndex, LoadSceneMode.Single);
		}

		void PreviousScene()
		{
			currentSceneIndex--;
			currentSceneIndex = (int)Mathf.Repeat(currentSceneIndex, sceneList.Length);
			SceneManager.LoadScene(currentSceneIndex, LoadSceneMode.Single);
		}

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
				PreviousScene();
			}
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
				NextScene();
            }
        }
    }
}