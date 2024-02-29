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
        /*
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        */
    }

    /*
    public void LoadNextGame()
    {
        AudioManager.Instance.Fade();
        Invoke("SceneChangeNextGame", 0.7f);
    }
    private void SceneChangeNextGame()
    {
        SceneManager.LoadScene(sceneNames[currentGameIndex], LoadSceneMode.Single);
        AudioManager.Instance.ChangeSong(currentGameIndex + 3);
    }
    */
}