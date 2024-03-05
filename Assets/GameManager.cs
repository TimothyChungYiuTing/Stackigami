using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public enum State {
    TITLE, MENU, GAME, SETTINGS, END
}

public class GameManager : Singleton<GameManager>
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug
        if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.Return)) {
            Time.timeScale = 1f;
            AudioManager.Instance.ChangeSong(0);
            AudioManager.Instance.spatialBlend = 0f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}