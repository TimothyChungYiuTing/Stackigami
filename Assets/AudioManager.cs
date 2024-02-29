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
    private float trackTimer = 0f;

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
            trackTimer = 0;
            currentSongIndex = songIndex;

            audioSource.clip = songClips[currentSongIndex];
            audioSource.Play();
        }
	}

	public void Stop() {
		audioSource.Stop();
	}

    public void Fade() {
        StartCoroutine(FadeEffect());
    }

    public IEnumerator FadeEffect() {
        //Fade out then Fade in again
        float startTime = Time.time;
        while (Time.time-startTime < 0.7f) {      //Done in 5 seconds
            multipliedVolume = Mathf.Cos(2 * Mathf.PI * (Time.time-startTime) / 5f);
            yield return null;
        }
        multipliedVolume = 1f;
    }
}