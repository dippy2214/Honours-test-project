using System.Collections;
using System.Collections.Generic;
using SteamAudio;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioListenerSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return; // only set up local client

        gameObject.AddComponent<AudioListener>();
        if (SceneManager.GetActiveScene().name == "RayVoiceLevel")
        {
            var steamListener = gameObject.AddComponent<SteamAudioListener>();
            steamListener.applyReverb = true;
            steamListener.reverbType = ReverbType.Realtime;
            SteamAudioManager.NotifyAudioListenerChanged();
        }
    }
}
