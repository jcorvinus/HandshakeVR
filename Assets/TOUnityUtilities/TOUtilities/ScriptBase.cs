using UnityEngine;
using System.Collections;


public partial class ScriptBase : MonoBehaviour {

    void Start()
    {
        this.GetComponent<Renderer>(GameObjectExtensions.GetComponentSafety.None);
    }
}
