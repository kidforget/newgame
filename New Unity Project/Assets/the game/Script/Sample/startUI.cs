using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class startUI : MonoBehaviour {
    //private GameController game;
    private Rect winRect = new Rect(10f, 10f, 300f, 100f);


    // Use this for initialization
    void Start()
    {
        //game = gameObject.GetComponent<GameController>();
    }
    void OnGUI()
    {
        if (GUI.Button(new Rect(Screen.width - 100, Screen.height - 150, 150f, 150f), "Start"))
        {
            SceneManager.LoadScene("game");
            //moved = true; Translate(Vector3.right * Time.deltaTime * speed * 10);
        };

    }

    // Update is called once per frame
    void Update () {
		
	}
}
