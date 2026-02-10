using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;

public class VivoxAutoTapManager : MonoBehaviour
{
    [Header("Optional parent for participant taps")]
    [SerializeField] private Transform audioTapParent;

    // Track active participant taps by PlayerId
    private readonly Dictionary<string, GameObject> _participantTaps = new();

    [Header("Channel settings")]
    [SerializeField] private string channelName = "MyChannel"; // Set your channel name here


    private IEnumerator Start()
    {
        // Wait for VivoxService and local login
        while (VivoxService.Instance == null || !VivoxService.Instance.IsLoggedIn)
        {
            yield return null;
        }

        Debug.Log($"Joined channel {channelName}, subscribing to participant events.");

        while(VivoxService.Instance.ActiveChannels.Count == 0)
            yield return null;

        // Subscribe to channel-specific participant events
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;

        // Add already-present participants
        foreach (var participant in VivoxService.Instance.ActiveChannels[VivoxVoiceManager.worldProximityChannel])
        {
            OnParticipantAdded(participant);
        }
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        if (participant == null || participant.IsSelf)
            return;

        Debug.Log($"Participant joined: {participant.PlayerId}");

        // Create a VivoxParticipant tap GameObject
        GameObject tapObj = participant.CreateVivoxParticipantTap($"{participant.PlayerId}_Tap");

        // Parent it if a container is assigned
        if (audioTapParent != null)
            tapObj.transform.SetParent(audioTapParent, false);

        // Optionally configure AudioSource
        AudioSource audioSource = tapObj.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // 0 = 2D audio
        }

        // Store tap for cleanup
        _participantTaps[participant.PlayerId] = tapObj;
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (participant == null)
            return;

        Debug.Log($"Participant left: {participant.PlayerId}");

        if (_participantTaps.TryGetValue(participant.PlayerId, out GameObject tapObj))
        {
            Destroy(tapObj);
            _participantTaps.Remove(participant.PlayerId);
        }
    }

    private void OnDestroy()
    {

        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;

    }
}