using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] public int TimePassed;
    [SerializeField] public float TimeToSpawn;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] public int[] ItemInfo = { 0, 30, 30, 90, 120 };
    [SerializeField] public GameObject Weapon;

    private void FixedUpdate()
    {
        if (Weapon == null)
        {
            TimePassed++;
        }
        if (TimePassed == TimeToSpawn * 200)
        {
            TimePassed = 0;
            Weapon = PhotonNetwork.Instantiate(WeaponItemPrefab.name, transform.position, Quaternion.identity);
            WeaponItemInfo _weaponInfoCache = Weapon.GetComponent<WeaponItemInfo>();
            _weaponInfoCache.CustomStart(ItemInfo);
            _weaponInfoCache.NeedToBeDestroy = false;
        }
    }
}
