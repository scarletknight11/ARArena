using UnityEngine;
using System.Collections;

public class GranadeFire : MonoBehaviour {
	
	public GameObject reference;
	GameObject gernade;
	public Transform spawnPosition;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void ThrowGernade(){
		gernade=(GameObject)Instantiate (reference,spawnPosition.position,spawnPosition.rotation);
		gernade.GetComponent<Rigidbody> ().AddForce(gernade.transform.forward*500);
		Debug.Log (Vector3.forward);
	}

	 
}
