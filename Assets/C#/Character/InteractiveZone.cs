using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class InteractiveZone : MonoBehaviour
{
    [SerializeField] private UPlayer _UPlayer;
    [SerializeField] public List<GameObject> WeaponsGO = new List<GameObject>();


    private void OnTriggerEnter2D(Collider2D other)
    {
        #region Weapons Item
        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponItemInfo _weaponItemInfo = other.gameObject.GetComponent<WeaponItemInfo>();

            switch (_weaponItemInfo.ItemType) {
                case 0:
                    int SimilarWeaponID = -1;
                    if (_weaponItemInfo.ItemInfo[0][0] == _UPlayer.WeaponInfo[0][0])
                        SimilarWeaponID = 0;
                    if (_weaponItemInfo.ItemInfo[0][0] == _UPlayer.WeaponInfo[1][0])
                        SimilarWeaponID = 1;

                    if (SimilarWeaponID != -1)
                    {
                        if (_weaponItemInfo.ItemInfo[0][3] >= _UPlayer.WeaponInfo[SimilarWeaponID][4] - _UPlayer.WeaponInfo[SimilarWeaponID][3])
                        {
                            _weaponItemInfo.ItemInfo[0][3] -= (ushort)(_UPlayer.WeaponInfo[SimilarWeaponID][4] - _UPlayer.WeaponInfo[SimilarWeaponID][3]);
                            _UPlayer.WeaponInfo[SimilarWeaponID][3] = _UPlayer.WeaponInfo[SimilarWeaponID][4];
                            if(_weaponItemInfo.ItemInfo[0][3] == 0)
                                _weaponItemInfo.PreDestroy();
                        }
                        else
                        {
                            _UPlayer.WeaponInfo[SimilarWeaponID][3] += _weaponItemInfo.ItemInfo[0][3];
                            _weaponItemInfo.PreDestroy();
                        }
                    }
                    else
                    {
                        WeaponsGO.Add(other.gameObject);
                        _UPlayer.GetWeaponButton.SetActive(true);
                    }
                    break;
                case 1:
                    int SimilarGrenadeID = -1;
                    if (_weaponItemInfo.ItemInfo[0][0] == _UPlayer.GrenadeInfo[0][0])
                        SimilarGrenadeID = 0;
                    if (_weaponItemInfo.ItemInfo[0][0] == _UPlayer.GrenadeInfo[1][0])
                        SimilarGrenadeID = 1;
                    if (SimilarGrenadeID != -1)
                    {
                        if (_UPlayer.GrenadeInfo[1][1] < _UPlayer.GrenadeInfo[1][2])
                        {
                            _UPlayer.GrenadeInfo[1][1]++;
                            _weaponItemInfo.PreDestroy();
                        }
                    }
                    break;
            }
        }
        #endregion
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        #region Weapons Item
        if (other.gameObject.tag == "WeaponItem")
        {
            WeaponItemInfo _weaponItemInfo = other.gameObject.GetComponent<WeaponItemInfo>();

            switch (_weaponItemInfo.ItemType)
            {
                case 0:
                    WeaponsGO.Remove(other.gameObject);
                    if (WeaponsGO.Count == 0)
                    {
                        _UPlayer.GetWeaponButton.SetActive(false);
                    }
                    break;
            }
        }
        #endregion
    }
}
