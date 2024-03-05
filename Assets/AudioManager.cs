using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
	public AudioSource audioSource;
	public AudioClip[] songClips;
	public float volume;
	[SerializeField] public float multipliedVolume = 1f; //For if I want to have temporary volume changes in the future
	private int currentSongIndex = 0;
    private int newSongIndex = 0;
    private float trackTimer = 0f;
    public float spatialBlend = 0f;

    new void Awake() {
        base.Awake();
        multipliedVolume = 1f;
		audioSource = GetComponent<AudioSource>();
        if (songClips.Length != 0) {
		    audioSource.clip = songClips[0];
            audioSource.loop = true;
		    audioSource.Play();
        }
    }

    void Update() {
        audioSource.volume = volume * multipliedVolume;
        audioSource.spatialBlend = spatialBlend;

        if (audioSource.isPlaying) {
            trackTimer += Time.deltaTime;
        }
        
        if (songClips.Length != 0) {
            if (!audioSource.isPlaying || trackTimer >= audioSource.clip.length) {
                //Loop
            }
        }
    }

	public void NextSong() {
        trackTimer = 0;
		currentSongIndex++;
		if (currentSongIndex == songClips.Length) {
			currentSongIndex = 0;
		}

		audioSource.clip = songClips[currentSongIndex];
		audioSource.Play();
	}

	public void ChangeSong(int songIndex) {
        if (currentSongIndex != songIndex) {
            FadeOut();
            newSongIndex = songIndex;
            Invoke("FadeIn", 5f);
        }
	}

	public void Stop() {
		audioSource.Stop();
	}    

    public void FadeIn() {
        if (currentSongIndex != newSongIndex) {
            trackTimer = 0;
            currentSongIndex = newSongIndex;

            audioSource.clip = songClips[currentSongIndex];
            audioSource.Play();
        }
        StartCoroutine(FadeEffect(true));
    }

    public void FadeOut() {
        StartCoroutine(FadeEffect(false));
    }

    public IEnumerator FadeEffect(bool fadeIn) {
        //Fade out then Fade in again
        float startTime = Time.time;
        while (Time.time-startTime < 5f) {      //Done in 5 seconds
            if (fadeIn) {
                multipliedVolume = 1 - (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
            }
            else {
                multipliedVolume = (1f + Mathf.Cos(Mathf.PI * (Time.time-startTime) / 5f)) / 2f;
            }
            yield return null;
        }
        if (fadeIn) {
            multipliedVolume = 1f;
        }
        else {
            multipliedVolume = 0f;
        }
    }
}