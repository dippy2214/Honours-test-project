using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerAudio : NetworkBehaviour
{
    public AudioSource audioSource;
    public AudioClip[] gunShots;
    public AudioClip[] footsteps;
    private float footVolume = 0.05f;
    private float gunVolume = 0.1f;
    // Start is called before the first frame update

    public void playGunshot()
    {
        int clipIndex = Random.Range(0, gunShots.Length);
        if (IsOwner) {
            PlayGunshotSoundServerRpc(clipIndex);  
            audioSource.PlayOneShot(gunShots[clipIndex], gunVolume);
        }
        
    }

    public void playFootstep(bool isCrouching)
    {
        float volume = footVolume;
        int clipIndex = Random.Range(0, footsteps.Length);
        if (isCrouching) volume /= 2;

        if (IsOwner) {
            PlayFootstepSoundServerRpc(clipIndex);  
            audioSource.PlayOneShot(footsteps[clipIndex], volume);
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayGunshotSoundServerRpc(int clipIndex)
    {
        PlayGunshotSoundClientRpc(clipIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayFootstepSoundServerRpc(int clipIndex)
    {
        PlayFootstepSoundClientRpc(clipIndex);
    }

    [ClientRpc]
    private void PlayGunshotSoundClientRpc(int clipIndex)
    {
        if (IsOwner) return;
        audioSource.PlayOneShot(gunShots[clipIndex], gunVolume);
    }
    
    [ClientRpc]
    private void PlayFootstepSoundClientRpc(int clipIndex)
    {
        if (IsOwner) return;
        audioSource.PlayOneShot(footsteps[clipIndex], footVolume);
    }
}
