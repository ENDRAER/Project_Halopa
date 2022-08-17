using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using Mirror;

public class UPlayer : NetworkBehaviour
{
    #region Player
    [Header("Player")]
    [SerializeField][SyncVar] public int TeamID;
    [SerializeField] private float speed;
    [SerializeField] private PlayerTexture PlayerTexture;
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private NetworkAnimator PlayerAnimator;
    [SerializeField] private Animator LegsAnimator;
    [SerializeField] private GameObject WeaponsEmpty;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject LegsGO;
    [SerializeField] public GameObject[] WeaponsTextures;
    #endregion

    #region UI
    [Header("UI")]
    [NonSerialized] private bl_Joystick LJS;
    [NonSerialized] public bl_Joystick RJS;
    [NonSerialized] public Text AmmoBar;
    [NonSerialized] public Text HPBar;
    [NonSerialized] public GameObject TakeWeaponButton;
    [NonSerialized] public GameObject DeadPanel;
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

    // 0-index; 1-rounds in the magazine; 2-max rounds in the magazine; 3-total ammunition; 4-max ammunition; 5-reload type(magazine, OneRound, overheat) // THIS IS THE DEFAULT FOR AR + PISTOL
    [SerializeField] public int[] WeaponInfo1 = { 0, 30, 30, 90 , 120, 0};
    [SerializeField] public int[] WeaponInfo2 = { 1, 6, 6, 18 , 24, 0};

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

    #region Grenate
    [Header("Grenate")]
    [NonSerialized] private GameObject FGrenade;
    [NonSerialized] private GameObject PGrenade;
    [NonSerialized] private int TotalForceThrowGrenate;
    [NonSerialized] private float ForceThrowGrenate = 10;
    [NonSerialized] private int FactorForceThrowGrenate;
    #endregion

    #region Other Links
    [Header("Other Links")]
    [SerializeField] private GameObject WeaponItemPrefab;
    [NonSerialized] private float VStickRH;
    [NonSerialized] private float VStickRV;
    [NonSerialized] private float VStickLH;
    [NonSerialized] private float VStickLV;
    [NonSerialized] public List<GameObject> Spawns = new List<GameObject>();
    #endregion


    private void Start()
    {
        if (!isLocalPlayer) return;

        #region FindButtons
        //Button Links
        LJS = GameObject.Find("LJS").GetComponent<bl_Joystick>();
        RJS = GameObject.Find("RJS").GetComponent<bl_Joystick>();
        AmmoBar = GameObject.Find("AmmoBarText").GetComponent<Text>();
        DeadPanel = GameObject.Find("DeadPanel");

        //buttons
        Camera = GameObject.Find("Camera");
        GameObject.Find("ReloadButton").GetComponent<Button>().onClick.AddListener(ReloadButtonClass);
        GameObject.Find("YouNeverDie").GetComponent<Button>().onClick.AddListener(revive);
        DeadPanel.SetActive(false);

        for (int a = 0; a <= 11;)
        {
            Spawns.Add(GameObject.Find("Spawn " + a));
            a++;
        }
        gameObject.transform.position = Spawns[UnityEngine.Random.Range(0,11)].transform.position;
        #endregion
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        #region compatibility
        //LS
        VStickLH = LJS.Vertical + LJS.Horizontal != 0 ? LJS.Horizontal : Input.GetAxis("Horizontal") * 5;
        VStickLV = LJS.Vertical + LJS.Horizontal != 0 ? LJS.Vertical : Input.GetAxis("Vertical") * 5;
        //RS
        VStickRH = RJS.Vertical + RJS.Horizontal != 0 ? RJS.Horizontal : Input.GetAxis("RS Horizontal") * 5;
        VStickRV = RJS.Vertical + RJS.Horizontal != 0 ? RJS.Vertical : Input.GetAxis("RS Vertical") * 5;

        if (Input.GetButton("Reload"))
        {
            ReloadButtonClass();
        }
        #endregion

        #region Katets of stiks
        double KatetLJS = Math.Sqrt((VStickLV * VStickLV) + (VStickLH * VStickLH));
        double KatetRJS = Math.Sqrt((VStickRV * VStickRV) + (VStickRH * VStickRH));
        #endregion

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
        }//weapon textures changer

        if (WeaponInfo1[1] == 0 && WeaponInfo1[3] != 0)
        {
            PlayerAnimator.SetTrigger("Reload");
        } // auto reload when u have 0 rounds
        #endregion

        #region run and aim

        if (KatetLJS > 1)
        {
            PlayerRB.AddForce(new Vector2(VStickLH * speed * Time.deltaTime, VStickLV * speed * Time.deltaTime)); //move
            LegsAnimator.SetFloat("Speed", (100 / 5 * (float)KatetLJS)/100);
        }
        else
            LegsAnimator.SetFloat("Speed", 0);

        if (KatetLJS > 1) 
        {
            LegsGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
        } // rotate Player

        if (KatetRJS > 1)
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickRV, VStickRH) * Mathf.Rad2Deg); 
        // rotate

        if (KatetRJS > 4)
            Fire();
        // fire Weapon

        #endregion

        Camera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,-10);
    }

    #region touch screen Buttons

    public void ReloadButtonClass()
    {
        if (WeaponInfo1[1] != WeaponInfo1[2] && WeaponInfo1[3] != 0)
        {
            PlayerAnimator.SetTrigger("Reload");
        }
    }

    public void SwapWeaponClass()
    {
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
        for (int i = 0; i < PlayerTexture.WeaponsGO.Count; )
        {
            if (MinDistance > Vector2.Distance(gameObject.transform.position, PlayerTexture.WeaponsGO[i].transform.position))
            {
                MinDistance = Vector2.Distance(gameObject.transform.position, PlayerTexture.WeaponsGO[i].transform.position);
                nearestWeapon = i;
            }
            i++;
        }

        //set values
        int[] InfoCache = WeaponInfo1;
        UniversalBridge _UniversalBridge = PlayerTexture.WeaponsGO[nearestWeapon].GetComponent<UniversalBridge>();
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
        ShieldNow = ShieldMax;
        HealthNow = HealthMax;
    }

    #endregion

    #region Reload
    public void ReloadEvent()
    {
        PlayerAnimator.ResetTrigger("Shoot");
        switch (WeaponInfo1[5])
        {
            case 0: // Magazine
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
                break;

            case 1: // OneRound
                WeaponInfo1[1]++;
                WeaponInfo1[3]--;

                if (WeaponInfo1[1] != WeaponInfo1[2] && WeaponInfo1[3] != 0)
                {
                    PlayerAnimator.SetTrigger("Reload");
                }
                else
                {
                    PlayerAnimator.ResetTrigger("Reload");
                    PlayerAnimator.SetTrigger("EndReload");
                }
                break;
        }
    }

    private void WeaponROF()
    {
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
        createdGrenateRB.AddForce(new Vector2(VStickRH * ForceThrowGrenate * TotalForceThrowGrenate, VStickRV * ForceThrowGrenate * TotalForceThrowGrenate));
    }
    #endregion

    #region Weapons
    [Command]
    public void Fire()
    {
        #region Animation
        if (WeaponInfo1[1] != 0 && WeaponReady == true)
        {
            PlayerAnimator.SetTrigger("Shoot");
        }
        #endregion

        #region AR
        if (WeaponInfo1[0] == 0 && WeaponReady == true && WeaponInfo1[1] > 0)
        {
            WeaponReady = false;
            WeaponInfo1[1]--;

            GameObject ThisBulletGO = Instantiate(ARBullet, gameObject.transform.position, Quaternion.Euler(0, 0, WeaponsEmpty.transform.localRotation.eulerAngles.z));
            NetworkServer.Spawn(ThisBulletGO);

            Bullets ThisBulletCS = ThisBulletGO.GetComponent<Bullets>();
            ThisBulletCS.DontFrendlyFire = TeamID;
        }
        #endregion

        #region Pistol //to rebuild
        if (WeaponInfo1[0] == 1 && WeaponReady == true && WeaponInfo1[1] > 0)
        {
            Debug.LogWarning("NEED TO REBUILD");
        }
        #endregion

        #region ShotGun
        if (WeaponInfo1[0] == 2 && WeaponReady == true && WeaponInfo1[1] > 0)
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
        }
        #endregion
    }
    #endregion
}