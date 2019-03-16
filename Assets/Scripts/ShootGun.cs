using UnityEngine;
using System.Collections;

public class ShootGun : MonoBehaviour {
	RaycastHit hit;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Physics.Raycast (transform.position, Vector3.forward, out hit)) {
			//Debug.Log (hit.transform.gameObject.tag);
		
			}
		}
	public void shootBullet(){
		if (Physics.Raycast (transform.position, Vector3.forward, out hit)) {
			Debug.Log (hit.transform.gameObject.tag);
			hit.transform.gameObject.GetComponent<v_AICompanion> ().currentHealth -= 20;
		}
	}
}
