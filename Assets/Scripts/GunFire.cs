using UnityEngine;
using System.Collections;

public class GunFire : MonoBehaviour {
	//public AudioClip fire;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ShootGun(){
		gameObject.GetComponent<Animator> ().Play ("Fire");
		gameObject.GetComponent<AudioSource> ().Play ();
	}
}
