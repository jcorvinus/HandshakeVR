using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LensExamplePause : MonoBehaviour {
  void Start(){
		var c1 = new OtherClass();
		var c2 = new OtherClass();
		Debug.Log(Game.isPaused.GetValue());
		
		c1.SetPaused();
		Debug.Log(Game.isPaused.GetValue());
		
		c2.SetPaused();
		Debug.Log(Game.isPaused.GetValue());
		
		c1.UnsetPaused();
		Debug.Log(Game.isPaused.GetValue());
		
		c2.UnsetPaused();
		Debug.Log(Game.isPaused.GetValue());
	}
}

class OtherClass{
	LensToken t;
	
	public void SetPaused(){
		t = Game.isPaused.AddLens(new Lens<bool>(0,(paused) => true));
	}
	
	public void UnsetPaused(){
		t.Remove();
	}
}

class Game{
	public static LensedValue<bool> isPaused = new LensedValue<bool>(false);
}
