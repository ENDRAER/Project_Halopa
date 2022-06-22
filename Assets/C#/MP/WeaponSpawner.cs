using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private int TimePassed;
    [SerializeField] private float TimeToSpawn;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] private int[] ItemInfo = { 0, 30, 30, 90, 120 };
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
            _weaponInfoCache.CustomStart(ItemInfo);
            _weaponInfoCache.NeedToBeDestroy = false;
        }
    }
}