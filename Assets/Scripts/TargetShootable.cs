using UnityEngine;
using Unity.Netcode;

public class TargetShootable : ShootableBase
{
    public override void OnShot(float damage, ulong damagerId)
    {
        Debug.Log($"[Target] Hit for {damage} damage!");
    }
}