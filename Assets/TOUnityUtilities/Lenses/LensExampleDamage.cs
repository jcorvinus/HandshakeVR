using UnityEngine;
using System.Collections;

public class LensExampleDamage : MonoBehaviour {

  enum DamageType{
		Base = 0,
		Multiplier = 1,
		Percentage = 2
	}

	void Start () {
		Debug.Log(Player.damage.GetValue());
		
		var multDmgLensToken = Player.damage.AddLens(
			new Lens<float>((int)DamageType.Multiplier, (dmg)=> dmg * 2)
			);
			
		Debug.Log(Player.damage.GetValue());
		
		var baseDmgLensToken = Player.damage.AddLens(
			new Lens<float>((int)DamageType.Base, (dmg)=> dmg + 5)
			);
			
		Debug.Log(Player.damage.GetValue());
		
		
		var multDmgLensToken2 = Player.damage.AddLens(
			new Lens<float>((int)DamageType.Multiplier, (dmg)=> dmg * 1.5f)
			);
			
		Debug.Log(Player.damage.GetValue());
		
	}
}

class Player{
	public static LensedValue<float> damage = new LensedValue<float>(10);
}
