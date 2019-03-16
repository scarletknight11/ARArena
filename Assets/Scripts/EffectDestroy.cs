using UnityEngine;
using System.Collections;

public class EffectDestroy : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
		StartCoroutine(Wait());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator Wait()
	{
		
		yield return new WaitForSeconds(4.0f);
		Destroy (gameObject);
	}
}
