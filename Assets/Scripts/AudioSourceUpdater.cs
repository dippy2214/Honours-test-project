using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;

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

        // REQUIRED for Steam Audio
        src.spatialize = true;
        src.spatialBlend = 1.0f; // fully 3D

        // IMPORTANT: disable Unity distance rolloff
        src.rolloffMode = AudioRolloffMode.Custom;
        src.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
                            AnimationCurve.Linear(0, 1, 1, 1));

        // Optional (recommended for voice)
        src.dopplerLevel = 0f;
    }
}
