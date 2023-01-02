using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private int TimePassed;
    [SerializeField] private float TimeToSpawn;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] public ushort ItemType;
    [SerializeField] private ushort[] ItemInfo;
    [SerializeField] private GameObject Weapon;

    private void FixedUpdate()
    {
        if (Weapon == null)
        {
            TimePassed++;
        }
        if (TimePassed == TimeToSpawn * 200)
        {
            TimePassed = 0;
            Weapon = Instantiate(WeaponItemPrefab, transform.position, Quaternion.identity);
            WeaponItemInfo _weaponInfoCache = Weapon.GetComponent<WeaponItemInfo>();
            _weaponInfoCache.CustomStart(ItemInfo, ItemType);
            _weaponInfoCache.MustBeDestroyed = false;
        }
    }
}