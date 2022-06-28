using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class Weapons : MonoBehaviour
{

    [SerializeField] public List<GameObject> WeaponsGO = new List<GameObject>();
    [SerializeField] public int Team;
    [SerializeField] public UPlayer _UPlayer;
    [SerializeField] public GameObject _CharacterGO;

    // Balance Weapon
    [Header("Assault Rifle")]
    [SerializeField] private bool ARReady = true;
    [SerializeField] private GameObject ARBullet;
    [SerializeField] private float ARRateOfFire;
    [SerializeField] public float ARReloadSpeed;

    [Header("Pistol")]
    [SerializeField] private bool PistolReady = true;
    [SerializeField] private GameObject PistolBullet;
    [SerializeField] private float PistolRateOfFire;
    [SerializeField] public float PistolReloadSpeed;

    [Header("SG")]
    [SerializeField] private bool SGReady = true;
    [SerializeField] private GameObject SGBullet;
    [SerializeField] private float SGRateOfFire;
    [SerializeField] public float SGReloadSpeed;
    [SerializeField] public float SGBulletsRange;
    [SerializeField] public float SGRandScale;
    
    public void Fire()
    {
        #region AR
        if (_UPlayer.WeaponInfo1[0] == 0 && ARReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            ARReady = false;
            _UPlayer.WeaponInfo1[1]--;

            GameObject ThisBulletGO = Instantiate(ARBullet, gameObject.transform.position, new Quaternion(0, 0, gameObject.transform.rotation.z, 0));
            NetworkServer.Spawn(ThisBulletGO);
            Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
            ThisBulletCS.DontDrendlyFire = Team;

            //addForce
            ThisBulletGO.transform.rotation = transform.rotation;
            Rigidbody2D BulletRB = ThisBulletGO.GetComponent<Rigidbody2D>();
            BulletRB.AddForce(new Vector2(_UPlayer.RJS.Horizontal, _UPlayer.RJS.Vertical) * ThisBulletCS.BulletSpeed, ForceMode2D.Impulse);

            StartCoroutine(ARWait(ARRateOfFire));
        }
        #endregion
        
        #region Pistol
        if (_UPlayer.WeaponInfo1[0] == 1 && PistolReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            PistolReady = false;
            _UPlayer.WeaponInfo1[1]--;

            GameObject ThisBulletGO = Instantiate(PistolBullet, gameObject.transform.position, new Quaternion(0, 0, gameObject.transform.rotation.z, 0));
            Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
            ThisBulletCS.DontDrendlyFire = Team;

            //addForce
            ThisBulletGO.transform.rotation = transform.rotation;
            Rigidbody2D BulletRB = ThisBulletGO.GetComponent<Rigidbody2D>();
            BulletRB.AddForce(new Vector2(_UPlayer.RJS.Horizontal, _UPlayer.RJS.Vertical) * ThisBulletCS.BulletSpeed, ForceMode2D.Impulse);

            StartCoroutine(PistolWait(PistolRateOfFire));
        }
        #endregion//to rebuild

        #region ShotGun
        if (_UPlayer.WeaponInfo1[0] == 2 && SGReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            SGReady = false;
            _UPlayer.WeaponInfo1[1]--;

            for (int i = 0; i < SGBulletsRange; i++)
            {
                GameObject ThisBulletGO = Instantiate(SGBullet, gameObject.transform.position, new Quaternion(0, 0, gameObject.transform.rotation.z, 0));
                NetworkServer.Spawn(ThisBulletGO);
                Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
                ThisBulletCS.DontDrendlyFire = Team;

                //addForce
                ThisBulletGO.transform.rotation = transform.rotation;
                Rigidbody2D BulletRB = ThisBulletGO.GetComponent<Rigidbody2D>();
                BulletRB.AddForce(new Vector2(_UPlayer.RJS.Horizontal + Random.Range(-SGRandScale, SGRandScale), _UPlayer.RJS.Vertical + Random.Range(-SGRandScale, SGRandScale)) * ThisBulletCS.BulletSpeed, ForceMode2D.Impulse);
            }

            StartCoroutine(SGWait(SGRateOfFire));
        }
        #endregion
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        #region Weapons Item
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

    private IEnumerator ARWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ARReady = true;
    }

    private IEnumerator PistolWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        PistolReady = true;
    }

    private IEnumerator SGWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SGReady = true;
    }
}
