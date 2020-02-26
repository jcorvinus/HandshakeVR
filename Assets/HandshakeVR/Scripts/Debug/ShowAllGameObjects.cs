using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using CatchCo;

namespace HandshakeVR.Debugging
{
    public class ShowAllGameObjects : MonoBehaviour
    {
        void ShowObject(GameObject gameObjectToShow)
        {
            gameObjectToShow.hideFlags = HideFlags.None;

            for(int i=0; i < gameObjectToShow.transform.childCount; i++)
            {
                ShowObject(gameObjectToShow.transform.GetChild(i).gameObject);
            }
        }

        [ExposeMethodInEditor]
        void Show()
        {
            foreach(GameObject rootObject in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                ShowObject(rootObject);
            }
        }
    }
}