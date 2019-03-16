using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {
	public GameObject explosive;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnCollisionEnter(Collision target)
	{
		if(target.gameObject.tag=="floor"){
			Debug.Log ("Hit Ground");
			Instantiate (explosive,transform.position,transform.rotation);
			Destroy (gameObject);
		}

	}
}
