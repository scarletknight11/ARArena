using UnityEngine;
using System.Collections;
namespace Vuforia
{
	public class  PlayerDetection : MonoBehaviour,ITrackableEventHandler {
		private TrackableBehaviour mTrackableBehaviour;
		public GameObject Player;
		public GameObject UI;

		// Use this for initialization
		void Start () {

			Player.SetActive (false);
			UI.SetActive (false);
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


				Player.SetActive (true);
				UI.SetActive (true);
				Debug.Log ("Player Created");




			}
			else
			{


			}
		}
	}
}
