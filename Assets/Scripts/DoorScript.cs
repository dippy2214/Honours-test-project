using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    private Animator anim;
    private AudioSource source;
    public AudioClip doorSoundClip;
    private float volume = 0.3f;
    public void Start()
    {
        anim = GetComponent<Animator>();
        source = GetComponent<AudioSource>();
    }

    public void Close()
    {
        if (anim != null)
        {
            anim.SetBool("isOpen", false);
            DoorSoundClientRPC();
        }
    }

    public void Open()
    {
        if (anim != null)
        {
            anim.SetBool("isOpen", true);
            DoorSoundClientRPC();
        }
    }

    [ClientRpc]
    public void DoorSoundClientRPC()
    {
        source.PlayOneShot(doorSoundClip, volume);
    }
}
