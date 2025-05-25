using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class RandomMusicPlayer : NetworkBehaviour
{

    [SerializeField]
    private List<AudioClip> musicClips = new();

    private int ClipCount => musicClips.Count;
    private int _playingClipIndex = -1;

    private AudioSource AudioSource => GetComponent<AudioSource>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            _playingClipIndex = new Random().Next(0, ClipCount - 1);
            PlayClip(_playingClipIndex);
        }
        else
        {
            RequestClipIndexRpc();
        }
    }
    
    private void PlayClip(int clipIndex)
    {
        print(clipIndex);
        if (clipIndex >= ClipCount || clipIndex < 0) throw new IndexOutOfRangeException("Index " + clipIndex + " is out of bound for the list of music clips with the size of " + ClipCount);
        AudioSource.clip = musicClips[clipIndex];
        AudioSource.Play();
    }
    
    [Rpc(SendTo.Server)]
    private void RequestClipIndexRpc(RpcParams rpcParams = default)
    {
        var clientId = rpcParams.Receive.SenderClientId;
        SetClipIndexRpc(_playingClipIndex, RpcTarget.Single(clientId, RpcTargetUse.Temp));
    }
    
    [Rpc(SendTo.SpecifiedInParams)]
    private void SetClipIndexRpc(int clipIndex, RpcParams rpcParams = default)
    {
        _playingClipIndex = clipIndex;
        PlayClip(_playingClipIndex);
    }
    
    
}
