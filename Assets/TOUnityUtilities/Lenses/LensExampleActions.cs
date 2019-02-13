using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LensExampleActions : MonoBehaviour {
  
	enum PAction{
		Move,
		Attack,
		OpenInventory,
		
	}
	
	LensedValue<List<PAction>> allowedActions = new LensedValue<List<PAction>>(
		new List<PAction>(new []{
			PAction.Move,
			PAction.Attack,
			PAction.OpenInventory
		})
	);
	

	void Start () {
		var conversationDisables = new List<PAction>(new []{
			PAction.Attack
		});
		
		var cutsceneDisables = new List<PAction>(new []{
			PAction.Attack,
			PAction.OpenInventory
		});
		
		var tiedDownPuzzleDisables = new List<PAction>(new []{
			PAction.Move,
			PAction.Attack
		});
		
		Debug.Log(AllowedAction(PAction.Move) + ", "+AllowedAction(PAction.Attack)+","+AllowedAction(PAction.OpenInventory));
		var tiedPuzDisablesToken = allowedActions.AddLens(new Lens<List<PAction>>(
			0,
			(actions) => actions.Except(tiedDownPuzzleDisables).ToList()
			));
			
		Debug.Log(AllowedAction(PAction.Move) + ", "+AllowedAction(PAction.Attack)+","+AllowedAction(PAction.OpenInventory));
		var cutsceneDisablesToken = allowedActions.AddLens(new Lens<List<PAction>>(
			0,
			(actions) => actions.Except(cutsceneDisables).ToList()
			));
			
		Debug.Log(AllowedAction(PAction.Move) + ", "+AllowedAction(PAction.Attack)+","+AllowedAction(PAction.OpenInventory));
		cutsceneDisablesToken.Remove();
		Debug.Log(AllowedAction(PAction.Move) + ", "+AllowedAction(PAction.Attack)+","+AllowedAction(PAction.OpenInventory));
		tiedPuzDisablesToken.Remove();
		Debug.Log(AllowedAction(PAction.Move) + ", "+AllowedAction(PAction.Attack)+","+AllowedAction(PAction.OpenInventory));
	}
	
	
	bool AllowedAction(PAction action){
		return allowedActions.GetValue().Contains(action);
	}
}
