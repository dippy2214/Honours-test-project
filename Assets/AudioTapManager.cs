using Unity.Services.Vivox;
using UnityEngine;

public class AudioTapManager : MonoBehaviour
{
    void Start()
    {
        VivoxService.Instance.ParticipantAddedToChannel += AddParticipantEffect;
    }

    void AddParticipantEffect(VivoxParticipant participant)
    {
        Debug.Log("trying to add participant tap");
        var gameObject = participant.CreateVivoxParticipantTap("MyNewGameObject");
    }
}
