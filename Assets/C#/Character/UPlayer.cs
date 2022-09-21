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
    [SerializeField] private NetworkAnimator PlayerNetworkAnimator;
    [SerializeField] private Animator PlayerAnimator;
    [SerializeField] private Animator LegsAnimator;
    [SerializeField] private GameObject WeaponsEmpty;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject LegsGO;
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField] private bl_Joystick LJS;
    [SerializeField] public bl_Joystick RJS;
    [NonSerialized] public Text AmmoBar;
    [NonSerialized] public Text HPBar;
    [NonSerialized] public GameObject TakeWeaponButton;
    [NonSerialized] public GameObject DeadPanel;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField][SyncVar] public bool IsDead;
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

    [SerializeField] private GameObject[] Bullets;

    // 0 - weapon index ; 1 - ammo in magazine ; 2 - max ammo in mag. ; 3 - total ammo ; 4 - max total ammo ; 5 - reload type (0 mag. ; 1 RoundPerRound ; 2 OverHeat)
    [SerializeField] public SyncList<int[]> WeaponInfo = new SyncList<int[]> { new int[] { 1, 6, 6, 99, 99, 1 }, new int[] { 0, 30, 30, 999, 999, 0 } };
    [SerializeField][SyncVar] public int WeaponUseIndex;

    [Header("Assault Rifle")]
    [SerializeField] private float ARRateOfFire;
    [SerializeField] public float ARReloadSpeed;

    [Header("Pistol")]
    [SerializeField] private float PistolRateOfFire;
    [SerializeField] public float PistolReloadSpeed;

    [Header("SG")]
    [SerializeField] private float SGRateOfFire;
    [SerializeField] public float SGReloadSpeed;
    [SerializeField] public int SGBulletsRange;
    [SerializeField] public float SGRandScale;
    #endregion

    #region Grenate
    /*
    [Header("Grenate")]
    [NonSerialized] private GameObject FGrenade;
    [NonSerialized] private GameObject PGrenade;
    [NonSerialized] private int TotalForceThrowGrenate;
    [NonSerialized] private float ForceThrowGrenate = 10;
    [NonSerialized] private int FactorForceThrowGrenate;
    */
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

        TeamID = UnityEngine.Random.Range(0,999);

        PlayerAnimator.SetInteger("WeaponID", WeaponInfo[WeaponUseIndex][0]);

        #region FindButtons
        //Button Links
        LJS = GameObject.Find("LJS").GetComponent<bl_Joystick>();
        RJS = GameObject.Find("RJS").GetComponent<bl_Joystick>();
        AmmoBar = GameObject.Find("AmmoBarText").GetComponent<Text>();
        DeadPanel = GameObject.Find("DeadPanel");

        //buttons
        Camera = GameObject.Find("Camera");
        GameObject.Find("SwapWeapon").GetComponent<Button>().onClick.AddListener(SwapWeaponButton);
        GameObject.Find("ReloadButton").GetComponent<Button>().onClick.AddListener(ReloadButtonClass);
        for (int a = 0; a <= 11;)
        {
            Spawns.Add(GameObject.Find("Spawn " + a));
            a++;
        }

        gameObject.transform.position = Spawns[UnityEngine.Random.Range(0,11)].transform.position;
        GameObject.Find("YouNeverDie").GetComponent<Button>().onClick.AddListener(revive);
        DeadPanel.SetActive(false);
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

        if (Input.GetButton("SwapWeapon"))
        {
            SwapWeaponButton();
        }
        #endregion

        #region Katets of stiks
        double KatetLJS = Math.Sqrt((VStickLV * VStickLV) + (VStickLH * VStickLH));
        double KatetRJS = Math.Sqrt((VStickRV * VStickRV) + (VStickRH * VStickRH));
        #endregion

        #region This NEED to rebuild ***************************
        AmmoBar.text = (WeaponInfo[WeaponUseIndex][1] + " | " + WeaponInfo[WeaponUseIndex][3]);

        if (WeaponInfo[WeaponUseIndex][1] == 0 && WeaponInfo[WeaponUseIndex][3] != 0)
        {
            PlayerNetworkAnimator.SetTrigger("Reload");
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

        Camera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,-10);
        
        if (KatetLJS > 1) 
        {
            LegsGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
        } // rotate Player

        if (KatetRJS > 1)
            WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickRV, VStickRH) * Mathf.Rad2Deg);
        // rotate

        if (KatetRJS > 4 && WeaponInfo[WeaponUseIndex][1] != 0) 
        {
            PlayerAnimator.SetBool("AutoFire",true);
        }
        else
            PlayerAnimator.SetBool("AutoFire", false);
        // fire Weapon

        #endregion
    }

    #region touch screen Buttons
    public void ReloadButtonClass()
    {
        if (WeaponInfo[WeaponUseIndex][1] != WeaponInfo[WeaponUseIndex][2] && WeaponInfo[WeaponUseIndex][3] != 0)
        {
            PlayerNetworkAnimator.SetTrigger("Reload");
        }
    }

    public void SwapWeaponButton()
    {
        PlayerNetworkAnimator.SetTrigger("SwapWeapon");
    }
    public void SwapWeaponEvent()
    {
        PlayerNetworkAnimator.ResetTrigger("SwapWeapon");
        WeaponUseIndex = WeaponUseIndex == 0 ? 1 : 0;
        PlayerAnimator.SetInteger("WeaponID", WeaponInfo[WeaponUseIndex][0]);
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

        ////set values
        //int[] InfoCache = WeaponInfo[WeaponUseIndex];
        //UniversalBridge _UniversalBridge = PlayerTexture.WeaponsGO[nearestWeapon].GetComponent<UniversalBridge>();
        //WeaponInfo[WeaponUseIndex] = _UniversalBridge._WeaponInfo[WeaponUseIndex].ItemInfo;
        //_UniversalBridge._WeaponInfo[WeaponUseIndex].PreDestroy();
        ////create new Item
        //GameObject NewWeaponItem = Instantiate(WeaponItemPrefab, transform.position, Quaternion.identity);
        //WeaponItemInfo _WeaponInfo[WeaponUseIndex]Cache = NewWeaponItem.GetComponent<WeaponItemInfo>();
        //_WeaponInfo[WeaponUseIndex]Cache.CustomStart(InfoCache);
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
        PlayerNetworkAnimator.ResetTrigger("Reload");
        switch (WeaponInfo[WeaponUseIndex][5])
        {
            #region Magazine
            case 0:
                if (WeaponInfo[WeaponUseIndex][3] >= WeaponInfo[WeaponUseIndex][2] - WeaponInfo[WeaponUseIndex][1])
                {
                    WeaponInfo[WeaponUseIndex][3] -= WeaponInfo[WeaponUseIndex][2] - WeaponInfo[WeaponUseIndex][1];
                    WeaponInfo[WeaponUseIndex][1] = WeaponInfo[WeaponUseIndex][2];
                }
                else
                {
                    WeaponInfo[WeaponUseIndex][1] += WeaponInfo[WeaponUseIndex][3];
                    WeaponInfo[WeaponUseIndex][3] = 0;
                }
                break;
            #endregion

            #region RoundPerRound
            case 1:
                WeaponInfo[WeaponUseIndex][1]++;
                WeaponInfo[WeaponUseIndex][3]--;

                if (WeaponInfo[WeaponUseIndex][1] != WeaponInfo[WeaponUseIndex][2] && WeaponInfo[WeaponUseIndex][3] != 0)
                {
                    PlayerNetworkAnimator.ResetTrigger("EndReload");
                    PlayerNetworkAnimator.SetTrigger("Reload");
                }
                else
                {
                    PlayerNetworkAnimator.ResetTrigger("Reload");
                    PlayerNetworkAnimator.SetTrigger("EndReload");
                }
                break;
            #endregion
        }
    }
    #endregion

    #region Grenate
    public void ForceGrenateClass()
    {
        //ForceThrowGrenate = 10;
        //FactorForceThrowGrenate = 1;
    }
    public void ThrowGrenateClass()
    {
        //if (GrenadeInfo[GrenadeInfo[0]]==0) { return; }
        //GameObject createdGrenate = Instantiate(GrenadeInfo[0] == 1? FGrenade : PGrenade, transform.position, Quaternion.identity);
        //createdGrenate.GetComponent<Grenade>().Sender = WeaponsEmpty;
        //Rigidbody2D createdGrenateRB = createdGrenate.GetComponent<Rigidbody2D>();
        //createdGrenateRB.AddForce(new Vector2(VStickRH * ForceThrowGrenate * TotalForceThrowGrenate, VStickRV * ForceThrowGrenate * TotalForceThrowGrenate));
    }
    #endregion

    #region Weapons
    public void Fire()
    {
        switch (WeaponInfo[WeaponUseIndex][0])
        {
            #region AR
            case 0:
                WeaponInfo[WeaponUseIndex][1]--;

                SpawnBullets();
                break;
            #endregion

            #region Pisotl
            case 1:
                WeaponInfo[WeaponUseIndex][1]--;

                SpawnBullets();
                break;
            #endregion

            #region ShotGun
            case 2:
                WeaponInfo[WeaponUseIndex][1]--;

                for (int i = 0; i < SGBulletsRange; i++)
                {
                    SpawnBullets();
                }
                break;
            #endregion
        }
    }

    [Command]
    public void SpawnBullets()
    {
        GameObject ThisBulletGO = Instantiate(Bullets[WeaponInfo[WeaponUseIndex][0]], gameObject.transform.position, Quaternion.Euler(0, 0, WeaponsEmpty.transform.localRotation.eulerAngles.z + UnityEngine.Random.Range(-SGRandScale, SGRandScale)));
        NetworkServer.Spawn(ThisBulletGO);
    }
    #endregion
}
