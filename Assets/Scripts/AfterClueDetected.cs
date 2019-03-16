using UnityEngine;
using System.Collections;
namespace Vuforia
{
public class AfterClueDetected : MonoBehaviour,ITrackableEventHandler {
		private TrackableBehaviour mTrackableBehaviour;
		public GameObject target_Player;
		public GameObject target_Enimies;
		public GameObject Enviroment;
	// Use this for initialization
	void Start () {
			target_Player.SetActive (false);
			target_Enimies.SetActive (false);
			Enviroment.SetActive (false);
		mTrackableBehaviour = GetComponent<TrackableBehaviour>();
		if (mTrackableBehaviour)
		{
			mTrackableBehaviour.RegisterTrackableEventHandler(this);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTrackableStateChanged(
		TrackableBehaviour.Status previousStatus,
		TrackableBehaviour.Status newStatus)
	{
		if (newStatus == TrackableBehaviour.Status.DETECTED ||
			newStatus == TrackableBehaviour.Status.TRACKED ||
			newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
		{
			// Play audio when target is found
		
				target_Player.SetActive (true);
				target_Enimies.SetActive (true);
				Enviroment.SetActive (true);
				Debug.Log ("Enviroment Created");



				
		}
		else
		{


		}
	}
	}
}
