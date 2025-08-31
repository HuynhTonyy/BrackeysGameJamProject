using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
public enum SFXType {
    drawCard,
    
}
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource bgSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip bgMusicClip;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    private bool isOpening = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {

        PlayBackgroundMusic();
        musicSlider.value = bgSource.volume;
        sfxSlider.value = sfxSource.volume;
    }
    private void OnEnable()
    {
        EventManager.Instance.onPlaySFX += PlaySFX;
    }
    private void OnDisable()
    {
        EventManager.Instance.onPlaySFX -= PlaySFX;
    }
    private void OnDestroy()
    {
        EventManager.Instance.onPlaySFX -= PlaySFX;
    }
    public void ChangeBGVolume()
    {
        bgSource.volume = musicSlider.value;
    }
    public void ChangeSFXVolume()
    {
        sfxSource.volume = sfxSlider.value;
    }
    private void PlayBackgroundMusic()
    {
        bgSource.clip = bgMusicClip;
        bgSource.loop = true;
        bgSource.Play();
    }
    private void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    public void ToggleAudioMenu()
    {
        if (isOpening)
        {
            rectTransform.DOMoveX(rectTransform.position.x + 600f, 1f);
            isOpening = false;
        }
        else
        {
            isOpening = true;
            rectTransform.DOMoveX(rectTransform.position.x + -600f, 1f);
        }
    }
}
