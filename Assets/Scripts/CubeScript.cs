using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class CubeScript : Interactable
{
    public bool heal = false;

    protected override void Interact(NetworkObject player)
    {
        var health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            float amount = heal ? 10f : -10f;
            health.ModifyHealth(amount); // server only
        }
    }

    // No need to touch UI here — PlayerUI reacts automatically via NetworkVariable
    protected override void ApplyEffectToLocalUI(PlayerUI playerUI)
    {
        // Optional: can add local effects like sounds if needed
    }
}