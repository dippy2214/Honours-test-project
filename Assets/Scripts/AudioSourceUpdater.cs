using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using SteamAudio;
using UnityEngine.Audio;
using Unity.Netcode;
using Unity.Services.Vivox.AudioTaps;

public class AudioSourceUpdater : MonoBehaviour
{
    public AudioMixerGroup voiceChatMixer;
    IEnumerator Start()
    {
        // Wait until VivoxService is initialized
        while (VivoxService.Instance == null)
        {
            yield return null;
        }

        Debug.Log("VivoxService ready, subscribing to participant events");
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        StartCoroutine(AttachSteamAudioWhenReady(participant));
    }

    IEnumerator AttachSteamAudioWhenReady(VivoxParticipant participant)
    {
        Debug.Log($"participant added: {participant.DisplayName}");
        while (participant.ParticipantTapAudioSource == null)
            yield return null;

        Debug.Log("tap found");
        AudioSource src = participant.ParticipantTapAudioSource;

        src.spatialize = true;
        src.spatialBlend = 1.0f;
        
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 6;
        src.maxDistance = 20;
       

        src.dopplerLevel = 0f;
        src.outputAudioMixerGroup = voiceChatMixer;

        src.gameObject.AddComponent<SteamAudioSource>();

        SteamAudioSource steamAudioSource = src.gameObject.GetComponent<SteamAudioSource>();
        steamAudioSource.occlusion = true;
        steamAudioSource.occlusionType = OcclusionType.Raycast;
        steamAudioSource.occlusionInput = OcclusionInput.SimulationDefined;

        steamAudioSource.transmission = true;
        steamAudioSource.transmissionType = TransmissionType.FrequencyIndependent;
        steamAudioSource.transmissionInput = TransmissionInput.SimulationDefined;
        
        steamAudioSource.reflections = true;

        steamAudioSource.distanceAttenuation = true;
        steamAudioSource.distanceAttenuationInput = DistanceAttenuationInput.CurveDriven;

        steamAudioSource.directivity = true;
    }

    
}
