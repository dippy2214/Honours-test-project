using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using SteamAudio;

public class AudioSourceUpdater : MonoBehaviour
{
    void Awake()
    {
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        StartCoroutine(AttachSteamAudioWhenReady(participant));
    }

    IEnumerator AttachSteamAudioWhenReady(VivoxParticipant participant)
    {
        while (participant.ParticipantTapAudioSource == null)
            yield return null;

        AudioSource src = participant.ParticipantTapAudioSource;

        src.spatialize = true;
        src.spatialBlend = 1.0f;
        
        src.rolloffMode = AudioRolloffMode.Custom;
        src.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
                            AnimationCurve.Linear(0, 1, 1, 1));

        src.dopplerLevel = 0f;

        src.gameObject.AddComponent<SteamAudioSource>();

        SteamAudioSource steamAudioSource = src.gameObject.GetComponent<SteamAudioSource>();
        steamAudioSource.occlusion = true;
        steamAudioSource.occlusionType = OcclusionType.Raycast;
        steamAudioSource.occlusionInput = OcclusionInput.SimulationDefined;

        steamAudioSource.reflections = true;
    }
}
