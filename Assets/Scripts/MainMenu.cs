using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour {
	public GameObject loading;
	// Use this for initialization
	void Start () {
		loading.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void Play(){
		loading.SetActive (true);
		SceneManager.LoadScene (1);
	}
}
