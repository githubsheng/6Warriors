using UnityEngine;
using System.Collections;

public class ParticleEmiter : MonoBehaviour {


	public GameObject arrow;

	void  End (){
		var clone= Instantiate (arrow, transform.position, Quaternion.identity);
		Destroy (clone, 4.0f);
	}

}