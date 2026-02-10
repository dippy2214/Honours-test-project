using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Vivox;

public class VivoxAutoTapManager : MonoBehaviour
{
    // Optionally assign a parent Transform for all participant taps
    [SerializeField]
    private Transform audioTapParent;

    // Track created taps by participant ID
    private readonly Dictionary<string, GameObject> _participantTaps = new();

    System.Collections.IEnumerator Start()
    {
        // Wait until VivoxService is initialized
        while (VivoxService.Instance == null)
        {
            yield return null;
        }

        Debug.Log("VivoxService ready, subscribing to participant events");
        VivoxService.Instance.ChannelJoined += OnChannelJoined;
    }

    private void OnDisable()
    {
        VivoxService.Instance.ChannelJoined -= OnChannelJoined;
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel -= OnParticipantRemoved;
    }

    private void OnChannelJoined(string channelName)
    {
        Debug.Log($"Channel joined (audio connected): {channelName}");

        // Now subscribe to participant events — media is now ready
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAdded;
        VivoxService.Instance.ParticipantRemovedFromChannel += OnParticipantRemoved;
    }

    private void OnParticipantAdded(VivoxParticipant participant)
    {
        Debug.Log($"Vivox participant joined: {participant.PlayerId}");
        if (participant == null || participant.IsSelf) 
            return; 

        

        // Create a VivoxParticipant audio tap GameObject
        GameObject tapObj = participant.CreateVivoxParticipantTap($"{participant.PlayerId}_Tap");

        // Parent it if a container is assigned
        if (audioTapParent != null)
            tapObj.transform.SetParent(audioTapParent, false);

        // Optionally configure the AudioSource on the tap
        AudioSource audioSource = tapObj.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // 0 = 2D audio, 1 = 3D
        }
        Debug.Log("found valid audio source");

        // Store it so we can remove it later
        _participantTaps[participant.PlayerId] = tapObj;
    }

    private void OnParticipantRemoved(VivoxParticipant participant)
    {
        if (participant == null) return;

        Debug.Log($"Vivox participant left: {participant.PlayerId}");

        if (_participantTaps.TryGetValue(participant.PlayerId, out GameObject tapObj))
        {
            Destroy(tapObj);
            _participantTaps.Remove(participant.PlayerId);
        }
    }
}