using UnityEngine;
using System.Collections;

public class DoorOpen : MonoBehaviour {
	public GameObject doors;
	bool doorState;
	// Use this for initialization
	void Start () {
		doorState = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void openDoors(){
		if (doorState) {
			doorState = false;
			doors.SetActive (false);
		} else {
			doorState = true;
			doors.SetActive (true);
		}
	}
}
