using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerTexture : MonoBehaviour
{

    [SerializeField] public List<GameObject> WeaponsGO = new List<GameObject>();
    [SerializeField] public UPlayer _UPlayer;
    [SerializeField] public GameObject _CharacterGO;

    private void OnTriggerEnter2D(Collider2D other)
    {
        #region Weapons Item
        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponItemInfo _weaponItemInfo = other.gameObject.GetComponent<UniversalBridge>()._weaponInfo;
            if (_weaponItemInfo.ItemInfo[0] == _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][0])
            {
                if (_weaponItemInfo.ItemInfo[3] >= _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4] - _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3])
                {
                    _weaponItemInfo.ItemInfo[3] -= _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4] - _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3];
                    _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3] = _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4];
                }
                else
                {
                    _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3] += _weaponItemInfo.ItemInfo[3];
                    _weaponItemInfo.PreDestroy();
                }
            } // weapon 1
            else if (_weaponItemInfo.ItemInfo[0] == _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][0])
            {
                if (_weaponItemInfo.ItemInfo[3] >= _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4] - _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3])
                {
                    _weaponItemInfo.ItemInfo[3] -= _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4] - _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3];
                    _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3] = _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][4];
                }
                else
                {
                    _UPlayer.WeaponInfo[_UPlayer.WeaponUseIndex][3] += _weaponItemInfo.ItemInfo[3];
                    _weaponItemInfo.PreDestroy();
                }
            } // weapon 2
            else
            {
                WeaponsGO.Add(other.gameObject);
                _UPlayer.TakeWeaponButton.SetActive(true);
            }
        }
        #endregion
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        #region Weapons Item
        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponsGO.Remove(other.gameObject);
            if (WeaponsGO.Count == 0)
            {
                _UPlayer.TakeWeaponButton.SetActive(false);
            }
        }
        #endregion
    }
}
