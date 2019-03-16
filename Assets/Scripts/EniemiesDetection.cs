using UnityEngine;
using System.Collections;
namespace Vuforia
{
	public class  EniemiesDetection : MonoBehaviour,ITrackableEventHandler {
		private TrackableBehaviour mTrackableBehaviour;
		public GameObject Eniemiesr;

		// Use this for initialization
		void Start () {
			
			Eniemiesr.SetActive (false);
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


				Eniemiesr.SetActive (true);
				Debug.Log ("Eniemies Created");




			}
			else
			{


			}
		}
	}
}
