using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ConnectinnoGames.SoundScripts
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource soundAudioSource;

        private AudioClip clickSound;
        private AudioClip backGroundMusic;
        private AudioClip coinsound;
        void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            clickSound = Resources.Load<AudioClip>("AudioResources/Sounds/DM-CGS-32");
            coinsound = Resources.Load<AudioClip>("AudioResources/Sounds/DM-CGS-26"); 
            backGroundMusic = Resources.Load<AudioClip>("AudioResources/Music/carefree_loop");
            musicAudioSource.clip = backGroundMusic;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }

        public void PlayClick()
        {   
            soundAudioSource.PlayOneShot(clickSound);
        }

        public void PlayCoin()
        {
            soundAudioSource.PlayOneShot(coinsound);
        }
    }

}
