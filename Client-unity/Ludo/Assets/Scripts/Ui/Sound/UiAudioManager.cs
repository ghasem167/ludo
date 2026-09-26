using System;
using System.Collections.Generic;
using UnityEngine;

public class UiAudioManager : MonoBehaviour
{

    [SerializeField] private AudioSource UiSFXAudioSource;
    [SerializeField] private UiSFXAudios uiSFXAudios;


    public void PlayClick()
    {
        UiSFXAudioSource.PlayOneShot(uiSFXAudios.OnClick);
    }

    public void PlayBack()
    {
        UiSFXAudioSource.PlayOneShot(uiSFXAudios.OnBack);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
