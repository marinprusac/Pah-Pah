using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

namespace Arena.Sound
{
    public class RandomMusicPlayer : NetworkBehaviour
    {
        
        
        

        [SerializeField]
        private List<AudioClip> musicClips = new();

        private int ClipCount => musicClips.Count;
        private int _playingClipIndex = -1;

        private AudioSource AudioSource => GetComponent<AudioSource>();

        public static RandomMusicPlayer Instance;
    
        public override void OnNetworkSpawn()
        {
            Instance = this;
        }

        public void OnDisable()
        {
            Instance = null;
        }

        public void SyncNextSong()
        {
            PlayRpc(new Random().Next(0, ClipCount - 1));
        }
    
        private void PlayClip(int clipIndex = -1)
        {
            if (clipIndex >= ClipCount || clipIndex < 0) throw new IndexOutOfRangeException("Index " + clipIndex + " is out of bound for the list of music clips with the size of " + ClipCount);
            AudioSource.clip = musicClips[clipIndex];
            AudioSource.Play();
        }

        public void StopClip()
        {
            AudioSource.Stop();
        }

        public void FadeOutVolume()
        {
            StartCoroutine(FadeOutVolumeCoroutine(3));
        }

        private IEnumerator FadeOutVolumeCoroutine(float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                AudioSource.volume = 1 - t / time;
                yield return null;
            }
            StopClip();
        }
    
        [Rpc(SendTo.ClientsAndHost)]
        private void PlayRpc(int clipIndex)
        {
            _playingClipIndex = clipIndex;
            PlayClip(_playingClipIndex);
        }
    
    
    }
}
