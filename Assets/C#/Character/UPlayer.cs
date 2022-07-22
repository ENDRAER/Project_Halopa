using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using Mirror;

public class UPlayer : NetworkBehaviour
{
    #region Other Links
    [Header("Other Links")]
    [SerializeField][SyncVar] public int TeamID;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private GameObject LegsGO;
    [SerializeField] private PlayerTexture _playerTexture;
    [SerializeField] private GameObject WeaponsEmpty;
    [SerializeField] private GameObject FGrenade;
    [SerializeField] private GameObject PGrenade;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] public IEnumerator ReloadCor;
    [SerializeField] public bool IsReloaded;
    [SerializeField] public GameObject[] WeaponsTextures;
    [SerializeField] public List<GameObject> Spawns = new List<GameObject>();
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField] public GameObject TakeWeaponButton;
    [SerializeField] public Text HPBar;
    [SerializeField] public Text AmmoBar;
    [SerializeField] public Text ConsoleText;
    [SerializeField] private bl_Joystick LJS;
    [SerializeField] public bl_Joystick RJS;
    [SerializeField] public GameObject DeadPanel;
    #endregion

    #region Grenate
    [Header("Grenate")]
    [SerializeField] private int TotalForceThrowGrenate;
    [SerializeField] private float ForceThrowGrenate = 10;
    [SerializeField] private int FactorForceThrowGrenate;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField] public bool IsDead;
    [SerializeField] public float HealthMax;
    [SerializeField][SyncVar] public float HealthNow;
    [SerializeField] public float ShieldMax;
    [SerializeField][SyncVar] public float ShieldNow;
    [SerializeField] public float MaxTimeToRegShield;
    [SerializeField] public float NowTimeToRegShield;
    [SerializeField] public Coroutine CorRegShield;
    #endregion

    #region Weapon
    [Header("PlayerWeaponInfo")]
    [SerializeField] public bool WeaponReady = true;
    // 0-index; 1-2-vallue grenades; 3-max grenades
    [SerializeField] public int[] GrenadeInfo = { 1, 2, 0, 2};
    // 0-index; 1-rounds in the magazine; 2-max rounds in the magazine; 3-total ammunition; 4-max ammunition // THIS IS THE DEFAULT FOR AR + PISTOL
    [SerializeField] public int[] WeaponInfo1 = { 0, 30, 30, 90 , 120};
    [SerializeField] public int[] WeaponInfo2 = { 1, 6, 6, 18 , 24};

    [Header("Assault Rifle")]
    [SerializeField] private GameObject ARBullet;
    [SerializeField] private float ARRateOfFire;
    [SerializeField] public float ARReloadSpeed;

    [Header("Pistol")]
    [SerializeField] private GameObject PistolBullet;
    [SerializeField] private float PistolRateOfFire;
    [SerializeField] public float PistolReloadSpeed;

    [Header("SG")]
    [SerializeField] private GameObject SGBullet;
    [SerializeField] private float SGRateOfFire;
    [SerializeField] public float SGReloadSpeed;
    [SerializeField] public int SGBulletsRange;
    [SerializeField] public float SGRandScale;
    #endregion


    private void Start()
    {
        if (!isLocalPlayer) return;

        #region FindButtons
        LJS = GameObject.Find("LJS").GetComponent<bl_Joystick>();
        RJS = GameObject.Find("RJS").GetComponent<bl_Joystick>();
        AmmoBar = GameObject.Find("AmmoBarText").GetComponent<Text>();
        ConsoleText = GameObject.Find("Console").GetComponent<Text>();
        DeadPanel = GameObject.Find("DeadPanel");
        GameObject.Find("YouNeverDie").GetComponent<Button>().onClick.AddListener(revive);
        DeadPanel.SetActive(false);
        GameObject.Find("Camera").transform.SetParent(gameObject.transform);
        for (int a = 0; a <= 11;)
        {
            print("Spawn " + a);
            Spawns.Add(GameObject.Find("Spawn " + a));
            a++;
        }
        gameObject.transform.position = Spawns[UnityEngine.Random.Range(0,11)].transform.position;
        #endregion
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        #region This NEED to rebuild ***************************
        AmmoBar.text = (WeaponInfo1[1] + " | " + WeaponInfo1[3]);

        for (int a = 0; WeaponsTextures.Length > a; a++)
        {
            if (WeaponInfo1[0] == a)
            {
                WeaponsTextures[a].SetActive(true);
            }
            else
            {
                WeaponsTextures[a].SetActive(false);
            }
        }

        if (WeaponInfo1[1] < 1 && IsReloaded == false && WeaponInfo1[3] != 0)
        {
            StartCoroutine(ReloadCor = RealoadIE());
        }
        #endregion 

        #region Katets of stiks
        double KatetLJS = Math.Sqrt((LJS.Vertical * LJS.Vertical) + (LJS.Horizontal * LJS.Horizontal));
        double KatetRJS = Math.Sqrt((RJS.Vertical * RJS.Vertical) + (RJS.Horizontal * RJS.Horizontal));
        #endregion

        #region run and aim

        PlayerRB.AddForce(new Vector2(LJS.Horizontal * speed * Time.deltaTime, LJS.Vertical * speed * Time.deltaTime)); //move

        if (KatetLJS > 0.1) 
        {
            LegsGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(LJS.Vertical, LJS.Horizontal) * Mathf.Rad2Deg);
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(LJS.Vertical, LJS.Horizontal) * Mathf.Rad2Deg);
        } // rotate Player

        if (KatetRJS > 0.2)
        {
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(RJS.Vertical, RJS.Horizontal) * Mathf.Rad2Deg);
        } // rotate

        if (KatetRJS > 4.8)
        {
            Fire();
        } // fire Weapon

        #endregion
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        ForceThrowGrenate += ForceThrowGrenate > 30 ? 0 : 0.2f * FactorForceThrowGrenate;
    }

    public void SwapWeaponClass()
    {
        StopCoroutine(ReloadCor);
        IsReloaded = false;

        int[] WeaponCashe = WeaponInfo2;
        WeaponInfo2 = WeaponInfo1;
        WeaponInfo1 = WeaponCashe;
    }
    
    public void SwapGrenadeClass()
    {
        if (GrenadeInfo[0] == 1 && GrenadeInfo[2] != 0)
        {
            GrenadeInfo[0] = 2;
        }
        else if (GrenadeInfo[0] == 2 && GrenadeInfo[1] != 0)
        {
            GrenadeInfo[0] = 1;
        }
    }

    public void TakeWeaponClass()
    {
        float MinDistance = 600;
        int nearestWeapon=0;
        for (int i = 0; i < _playerTexture.WeaponsGO.Count; )
        {
            if (MinDistance > Vector2.Distance(gameObject.transform.position, _playerTexture.WeaponsGO[i].transform.position))
            {
                MinDistance = Vector2.Distance(gameObject.transform.position, _playerTexture.WeaponsGO[i].transform.position);
                nearestWeapon = i;
            }
            i++;
        }

        //set values
        int[] InfoCache = WeaponInfo1;
        UniversalBridge _UniversalBridge = _playerTexture.WeaponsGO[nearestWeapon].GetComponent<UniversalBridge>();
        WeaponInfo1 = _UniversalBridge._weaponInfo.ItemInfo;
        _UniversalBridge._weaponInfo.PreDestroy();
        //create new Item
        GameObject NewWeaponItem = Instantiate(WeaponItemPrefab, transform.position, Quaternion.identity);
        WeaponItemInfo _weaponInfoCache = NewWeaponItem.GetComponent<WeaponItemInfo>();
        _weaponInfoCache.CustomStart(InfoCache);
    }

    public void revive()
    {
        gameObject.transform.position = Spawns[UnityEngine.Random.Range(0, 11)].transform.position;
        DeadPanel.SetActive(false);
        IsDead = false;
    }

    #region reload
    public void ReloadStartClass()
    {
        if (IsReloaded == false && WeaponInfo1[1] != WeaponInfo1[2] && WeaponInfo1[3] != 0)
        {
            StartCoroutine(ReloadCor = RealoadIE());
        }
    }

    private IEnumerator RealoadIE()
    {
        IsReloaded = true;
        Debug.Log("Reloading...");
        #region Reloading speed
        switch (WeaponInfo1[0])
        {
            case 0:
                yield return new WaitForSeconds(ARReloadSpeed);
                break;
            case 1:
                yield return new WaitForSeconds(PistolReloadSpeed);
                break;
            case 2:
                yield return new WaitForSeconds(SGReloadSpeed);
                break;
        }
        #endregion

        if (WeaponInfo1[3] >= WeaponInfo1[2] - WeaponInfo1[1])
        {
            WeaponInfo1[3] -= WeaponInfo1[2] - WeaponInfo1[1];
            WeaponInfo1[1] = WeaponInfo1[2];
        }
        else
        {
            WeaponInfo1[1] += WeaponInfo1[3];
            WeaponInfo1[3] = 0;
        }
        Debug.Log("%Reloaded%");
        IsReloaded = false;
    }

    private IEnumerator WeaponWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        WeaponReady = true;
    }
    #endregion

    #region Grenate
    public void ForceGrenateClass()
    {
        ForceThrowGrenate = 10;
        FactorForceThrowGrenate = 1;
    }
    public void ThrowGrenateClass()
    {
        if (GrenadeInfo[GrenadeInfo[0]]==0) { return; }
        GameObject createdGrenate = Instantiate(GrenadeInfo[0] == 1? FGrenade : PGrenade, transform.position, Quaternion.identity);
        createdGrenate.GetComponent<Grenade>().Sender = WeaponsEmpty;
        Rigidbody2D createdGrenateRB = createdGrenate.GetComponent<Rigidbody2D>();
        createdGrenateRB.AddForce(new Vector2(RJS.Horizontal * ForceThrowGrenate * TotalForceThrowGrenate, RJS.Vertical * ForceThrowGrenate * TotalForceThrowGrenate));
        Debug.Log(new Vector2(RJS.Vertical, RJS.Vertical));
    }
    #endregion

    #region Weapons
    [Command]
    public void Fire()
    {
        #region AR
        if (WeaponInfo1[0] == 0 && WeaponReady == true && WeaponInfo1[1] > 0 && IsReloaded == false)
        {
            WeaponReady = false;
            WeaponInfo1[1]--;

            GameObject ThisBulletGO = Instantiate(ARBullet, gameObject.transform.position, Quaternion.Euler(0, 0, WeaponsEmpty.transform.localRotation.eulerAngles.z));
            NetworkServer.Spawn(ThisBulletGO);

            Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
            ThisBulletCS.DontFrendlyFire = TeamID;

            StartCoroutine(WeaponWait(ARRateOfFire));
        }
        #endregion

        #region Pistol //to rebuild
        if (WeaponInfo1[0] == 1 && WeaponReady == true && WeaponInfo1[1] > 0 && IsReloaded == false)
        {
            WeaponReady = false;
            WeaponInfo1[1]--;

            GameObject ThisBulletGO = Instantiate(PistolBullet, gameObject.transform.position, new Quaternion(0, 0, gameObject.transform.rotation.z, 0));
            Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
            ThisBulletCS.DontFrendlyFire = TeamID;

            //addForce
            ThisBulletGO.transform.rotation = transform.rotation;
            Rigidbody2D BulletRB = ThisBulletGO.GetComponent<Rigidbody2D>();
            BulletRB.AddForce(new Vector2(RJS.Horizontal, RJS.Vertical) * ThisBulletCS.BulletSpeed, ForceMode2D.Impulse);

            StartCoroutine(WeaponWait(PistolRateOfFire));
        }
        #endregion

        #region ShotGun
        if (WeaponInfo1[0] == 2 && WeaponReady == true && WeaponInfo1[1] > 0 && IsReloaded == false)
        {
            WeaponReady = false;
            WeaponInfo1[1]--;

            for (int i = 0; i < SGBulletsRange; i++)
            {
                GameObject ThisBulletGO = Instantiate(SGBullet, gameObject.transform.position, Quaternion.Euler(0, 0, WeaponsEmpty.transform.localRotation.eulerAngles.z + UnityEngine.Random.Range(-SGRandScale,SGRandScale)));
                NetworkServer.Spawn(ThisBulletGO);
                
                Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
                ThisBulletCS.DontFrendlyFire = TeamID;
            }

            StartCoroutine(WeaponWait(SGRateOfFire));
        }
        #endregion
    }
    #endregion
}

/*
ConsoleText.text= ConsoleText.text + ";" + gg;
*/