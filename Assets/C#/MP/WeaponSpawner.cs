using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponSpawner : NetworkBehaviour
{
    [SerializeField] private int TimePassed;
    [SerializeField] private float TimeToSpawn;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] private int[] ItemInfo;
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
            SpawnWeapon();
            WeaponItemInfo _weaponInfoCache = Weapon.GetComponent<WeaponItemInfo>();
            _weaponInfoCache.CustomStart(ItemInfo);
            _weaponInfoCache.MustBeDestroyed = false;
        }
    }

    [Command]
    public void SpawnWeapon()
    {
        NetworkServer.Spawn(Weapon);
    }
}