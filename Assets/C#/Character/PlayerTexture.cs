using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class PlayerTexture : MonoBehaviour
{
    [SerializeField] public UPlayer _UPlayer;
    [SerializeField] public List<GameObject> WeaponsGO = new List<GameObject>();


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Damage>() != null)
        {
            _UPlayer.HealthNow -= other.gameObject.GetComponent<Damage>().damage;
            _UPlayer.HPBar.text = "HP: " + Math.Ceiling(_UPlayer.HealthNow) + "/100";
        }

        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponItemInfo _weaponItemInfo = other.gameObject.GetComponent<UniversalBridge>()._weaponInfo;
            if (_weaponItemInfo.ItemInfo[0] == _UPlayer.WeaponInfo1[0])
            {
                if (_weaponItemInfo.ItemInfo[3] >= _UPlayer.WeaponInfo1[4] - _UPlayer.WeaponInfo1[3])
                {
                    _weaponItemInfo.ItemInfo[3] -= _UPlayer.WeaponInfo1[4] - _UPlayer.WeaponInfo1[3];
                    _UPlayer.WeaponInfo1[3] = _UPlayer.WeaponInfo1[4];
                }
                else
                {
                    _UPlayer.WeaponInfo1[3] += _weaponItemInfo.ItemInfo[3];
                    _weaponItemInfo.PreDestroy();
                }
            } // weapon 1
            else if (_weaponItemInfo.ItemInfo[0] == _UPlayer.WeaponInfo2[0])
            {
                if (_weaponItemInfo.ItemInfo[3] >= _UPlayer.WeaponInfo2[4] - _UPlayer.WeaponInfo2[3])
                {
                    _weaponItemInfo.ItemInfo[3] -= _UPlayer.WeaponInfo2[4] - _UPlayer.WeaponInfo2[3];
                    _UPlayer.WeaponInfo2[3] = _UPlayer.WeaponInfo2[4];
                }
                else
                {
                    _UPlayer.WeaponInfo2[3] += _weaponItemInfo.ItemInfo[3];
                    _weaponItemInfo.PreDestroy();
                }
            } // weapon 2
            else
            {
                WeaponsGO.Add(other.gameObject);
                _UPlayer.TakeWeaponButton.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponsGO.Remove(other.gameObject);
            if (WeaponsGO.Count == 0)
            {
                _UPlayer.TakeWeaponButton.SetActive(false);
            }
        }
    }
}
