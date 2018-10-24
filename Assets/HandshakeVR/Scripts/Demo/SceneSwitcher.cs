using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HandshakeVR
{
    public class SceneSwitcher : MonoBehaviour
    {
        static SceneSwitcher instance;
        public static SceneSwitcher Instance { get { return instance; } }

        string[] sceneList = new string[]
        {
            "SteamVRTest",
            "Basic Interactions",
            "Basic UI",
            "CurvedUI",
            "Dynamic UI",
            "Hand UI",
            "Anchors",
            "Interaction Callbacks",
            "Moving Reference Frame",
            "SwapGrasp"
        };

        int currentSceneIndex = -1;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else Destroy(gameObject);
        }

        // Use this for initialization
        void Start()
        {
            Scene scene = SceneManager.GetActiveScene();

            currentSceneIndex = scene.buildIndex;
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentSceneIndex--;
                currentSceneIndex = (int)Mathf.Repeat(currentSceneIndex, sceneList.Length);
                SceneManager.LoadScene(sceneList[currentSceneIndex], LoadSceneMode.Single);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentSceneIndex++;
                currentSceneIndex = (int)Mathf.Repeat(currentSceneIndex, sceneList.Length);
                SceneManager.LoadScene(sceneList[currentSceneIndex], LoadSceneMode.Single);
            }
        }
    }
}