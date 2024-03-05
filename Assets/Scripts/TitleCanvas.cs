using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleCanvas : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayClicked()
    {
        audioSource.Play();
        Invoke("NextScene", 0.3f);
    }

    private void NextScene()
    {
        SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
        AudioManager.Instance.volume *= 0.7f;
    }
}
