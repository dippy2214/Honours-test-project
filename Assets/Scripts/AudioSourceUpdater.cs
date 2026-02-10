using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;
using SteamAudio;
using UnityEngine.Audio;
using Unity.Netcode;

public class AudioSourceUpdater : MonoBehaviour
{
    public AudioMixerGroup voiceChatMixer;
    void Awake()
    {
        
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
    }

    void OnParticipantAdded(VivoxParticipant participant)
    {
        GameObject tapGO = participant.CreateVivoxParticipantTap(
            participant.PlayerId,
            true // spatialized
        );

        tapGO.transform.SetParent(NetworkManager.Singleton.LocalClient.PlayerObject.transform, false);
        tapGO.transform.localPosition = UnityEngine.Vector3.zero;

        StartCoroutine(AttachSteamAudioWhenReady(participant));
    }

    IEnumerator AttachSteamAudioWhenReady(VivoxParticipant participant)
    {
        while (participant.ParticipantTapAudioSource == null)
            yield return null;

        AudioSource src = participant.ParticipantTapAudioSource;

        src.spatialize = true;
        src.spatialBlend = 1.0f;
        
        src.spatialBlend = 1f;
        src.rolloffMode = AudioRolloffMode.Custom;
        src.SetCustomCurve(
            AudioSourceCurveType.CustomRolloff,
            AnimationCurve.Constant(0, 1000, 1f)
        );

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
    }
}
