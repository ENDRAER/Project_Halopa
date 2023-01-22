using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using Mirror;
using TMPro;
using System.Security.Principal;


public class UPlayer : NetworkBehaviour
{
    #region Player
    [Header("Player")]
    [SerializeField][SyncVar] public byte TeamID;
    [SerializeField] private float speed;
    [SerializeField] private InteractiveZone InteractiveZone;
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private NetworkAnimator PlayerNetworkAnimator;
    [SerializeField] private Animator PlayerAnimator;
    [SerializeField] private Animator LegsAnimator;
    [SerializeField] private GameObject PlayerTextureGO;
    [SerializeField] private GameObject LegsGO;
    [SerializeField] private GameObject DyingDoll;
    [SerializeField] private GameObject _DyingDoll;
    [NonSerialized] private GameObject Camera;
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField] private bl_Joystick LJS;
    [SerializeField] public bl_Joystick RJS;
    [NonSerialized] public Text AmmoBar;
    [NonSerialized] public Text GrenAmmoBar;
    [NonSerialized] public GameObject GetWeaponButton;
    [NonSerialized] public Image ImageGetWeaponButton;
    [NonSerialized] public GameObject DeadPanel;
    #endregion

    #region Health
    [Header("Health")]
    [SerializeField] public bool IsDead;
    [SerializeField] public float HealthMax;
    [SerializeField] public float HealthNow;
    [SerializeField] public float ShieldMax;
    [SerializeField] public float ShieldNow;
    [SerializeField] public float MaxTimeToRegShield;
    [SerializeField] public float NowTimeToRegShield;
    [SerializeField] public Coroutine CorRegShield;
    #endregion

    #region Weapon
    [Header("PlayerWeaponInfo")]

    [SerializeField] private GameObject[] Bullets;
    [SerializeField] private Sprite[] WeaponTextures;
    [SerializeField] private GameObject WeaponItemPrefab;
    [NonSerialized] private GameObject SpawnWeaponItem;

    // 0 - weapon index ; 1 - ammo in magazine ; 2 - max ammo in mag. ; 3 - total ammo ; 4 - max total ammo ; 5 - reload type (0 mag. ; 1 RoundPerRound ; 2 OverHeat)
    [NonSerialized] public SyncList<ushort[]> WeaponInfo = new SyncList<ushort[]> { new ushort[] { 0, 30, 30, 999, 999, 0 }, new ushort[] { 2, 6, 6, 30, 30, 1 } };
    [NonSerialized] public SyncList<ushort[]> StartWeaponInfo = new SyncList<ushort[]> { new ushort[] { 0, 30, 30, 999, 999, 0 }, new ushort[] { 2, 6, 6, 30, 30, 1 } };
    [SerializeField][SyncVar] public int WeaponUseIndex = 0;

    [Header("Assault Rifle")]

    [Header("Pistol")]

    [Header("SG")]
    [SerializeField] public int SGBulletsRange;
    [SerializeField] public float SGRandScale;
    #endregion

    #region Grenate
    
    [Header("Grenate")]
    [SerializeField] private GameObject[] GrenadePrefab;
    [SerializeField] public GameObject GrenadeSlotGO;
    [NonSerialized] private GameObject createdGrenate;
    // 0 - grenade type ; 1 - grenades vlue ; 2 - max grenades vlue
    [SerializeField] public SyncList<byte[]> GrenadeInfo = new SyncList<byte[]> { new byte[] { 0, 2, 2 }, new byte[] { 1, 2, 2 } };
    [NonSerialized] public SyncList<byte[]> StartGrenadeInfo = new SyncList<byte[]> { new byte[] { 0, 2, 2 }, new byte[] { 1, 2, 2 } };
    [SerializeField][SyncVar] public int GrenadesSlotUsing = 0;
    [SerializeField] private float ScaleForceGreande = 0;
    [SerializeField] private float ForceThrowGrenate = 10;
    
    #endregion

    #region Other Links
    [Header("Other Links")]
    [NonSerialized] private float VStickRH;
    [NonSerialized] private float VStickRV;
    [NonSerialized] private float VStickLH;
    [NonSerialized] private float VStickLV;
    [NonSerialized] public List<GameObject> Spawns = new List<GameObject>();
    #endregion



    private void Start()
    {
        PlayerAnimator.SetInteger("WeaponID", WeaponInfo[WeaponUseIndex][0]);

        if (!isLocalPlayer) return;


        #region FindButtons
        //Button Links
        LJS = GameObject.Find("LJS").GetComponent<bl_Joystick>();
        RJS = GameObject.Find("RJS").GetComponent<bl_Joystick>();
        AmmoBar = GameObject.Find("AmmoBarText").GetComponent<Text>();
        GrenAmmoBar = GameObject.Find("GrenAmmoBarText").GetComponent<Text>();
        DeadPanel = GameObject.Find("DeadPanel");
        GetWeaponButton = GameObject.Find("GetWeapon");
        ImageGetWeaponButton = GetWeaponButton.GetComponent<Image>();
        GetWeaponButton.SetActive(false);


        //buttons
        Camera = GameObject.Find("Camera");
        GameObject.Find("YouNeverDie").GetComponent<Button>().onClick.AddListener(ReviveButton);
        GameObject.Find("SwapWeapon").GetComponent<Button>().onClick.AddListener(SwapWeaponButton);
        GameObject.Find("SwapGrenade").GetComponent<Button>().onClick.AddListener(SwapGrenadeButton); 
        GameObject.Find("ReloadButton").GetComponent<Button>().onClick.AddListener(ReloadButtonClass);
        GetWeaponButton.GetComponent<Button>().onClick.AddListener(TakeWeaponClass);

        EventTrigger ThrowGrenadeTrigger = GameObject.Find("ThrowGrenade").GetComponent<EventTrigger>();
        EventTrigger.Entry entryD = new EventTrigger.Entry(); entryD.eventID = EventTriggerType.PointerDown; entryD.callback.AddListener((data) => { GrenadeGet(); }); ThrowGrenadeTrigger.triggers.Add(entryD);
        EventTrigger.Entry entryU = new EventTrigger.Entry(); entryU.eventID = EventTriggerType.PointerUp; entryU.callback.AddListener((data) => { GrenadeThrow(); }); ThrowGrenadeTrigger.triggers.Add(entryU);


        for (int a = 0; a <= 11;)
        {
            Spawns.Add(GameObject.Find("Spawn " + a));
            a++;
        }

        gameObject.transform.position = Spawns[1].transform.position;
        DeadPanel.SetActive(false);
        #endregion
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        #region compatibility
        if (LJS.Vertical != 0 || LJS.Horizontal != 0)
        {
            VStickLH = LJS.Horizontal;
            VStickLV = LJS.Vertical;
        }
        else
        {
            VStickLH = Input.GetAxis("Horizontal") * 5;
            VStickLV = Input.GetAxis("Vertical") * 5;
        }//LS
        if (RJS.Vertical != 0 || RJS.Horizontal != 0)
        {
            VStickRH = RJS.Horizontal;
            VStickRV = RJS.Vertical;
        }
        else
        {
            VStickRH = Input.GetAxis("RS Horizontal") * 5;
            VStickRV = Input.GetAxis("RS Vertical") * 5;
        }//RS

        if (Input.GetButtonDown("Reload"))
        {
            ReloadButtonClass();
        }

        if (Input.GetButtonDown("SwapWeapon"))
        {
            SwapWeaponButton();
        }

        if (Input.GetButtonDown("Grenade"))
        {
            GrenadeGet();
        }
        if (Input.GetButtonUp("Grenade"))
        {
            GrenadeThrow();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            CmdReviveButton();
        }

        #endregion

        #region Katets of stiks
        double KatetLJS = Math.Sqrt((VStickLV * VStickLV) + (VStickLH * VStickLH));
        double KatetRJS = Math.Sqrt((VStickRV * VStickRV) + (VStickRH * VStickRH));
        #endregion

        #region run and aim

        if (KatetLJS > 1)
        {
            PlayerRB.AddForce(new Vector2(VStickLH * speed * Time.deltaTime, VStickLV * speed * Time.deltaTime)); 
            LegsAnimator.SetFloat("Speed", (100 / 5 * (float)KatetLJS)/100);
        }//move
        else
            LegsAnimator.SetFloat("Speed", 0);

        Camera.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,-10);
        
        if (KatetLJS > 1) 
        {
            LegsGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
            PlayerTextureGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickLV, VStickLH) * Mathf.Rad2Deg);
        } // rotate Player

        if (KatetRJS > 1)
            PlayerTextureGO.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(VStickRV, VStickRH) * Mathf.Rad2Deg);
        // rotate

        if (KatetRJS > 4 && WeaponInfo[WeaponUseIndex][1] != 0) 
        {
            PlayerAnimator.SetBool("AutoFire",true);
        }
        else
            PlayerAnimator.SetBool("AutoFire", false);
        // fire Weapon

        #endregion

        #region This NEED to rebuild ***************************
        AmmoBar.text = (WeaponInfo[WeaponUseIndex][1] + " | " + WeaponInfo[WeaponUseIndex][3]);
        GrenAmmoBar.text = (GrenadeInfo[0][1] + " | " + GrenadeInfo[1][1]);

        if (WeaponInfo[WeaponUseIndex][1] == 0 && WeaponInfo[WeaponUseIndex][3] != 0)
        {
            PlayerNetworkAnimator.SetTrigger("Reload");
        } // auto reload when u have 0 rounds
        #endregion

        #region other
        float MinDistance = 600;
        int nearestWeapon = 0;
        if (InteractiveZone.WeaponsGO.Count != 0)
        {
            for (int i = 0; i < InteractiveZone.WeaponsGO.Count;)
            {
                if (MinDistance > Vector2.Distance(gameObject.transform.position, InteractiveZone.WeaponsGO[i].transform.position))
                {
                    MinDistance = Vector2.Distance(gameObject.transform.position, InteractiveZone.WeaponsGO[i].transform.position);
                    nearestWeapon = i;
                }
                i++;
            }
            ImageGetWeaponButton.sprite = WeaponTextures[InteractiveZone.WeaponsGO[nearestWeapon].GetComponent<WeaponItemInfo>().ItemInfo[0][0]];
        }
        #endregion
    }

    #region touch screen Buttons
    public void ReloadButtonClass()
    {
        if (WeaponInfo[WeaponUseIndex][1] < WeaponInfo[WeaponUseIndex][2] && WeaponInfo[WeaponUseIndex][3] > 0)
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
        WeaponUseIndex = WeaponUseIndex == 0? 1 : 0;
        PlayerAnimator.SetInteger("WeaponID", WeaponInfo[WeaponUseIndex][0]);
    }

    public void ReviveButton()
    {
        transform.position = Spawns[1].transform.position;

        PlayerTextureGO.SetActive(true);
        LegsGO.SetActive(true);
        DeadPanel.SetActive(false);

        WeaponInfo = StartWeaponInfo;
        WeaponUseIndex = 0;

        GrenadeInfo = StartGrenadeInfo;
        GrenadesSlotUsing = 0;

        HealthNow = HealthMax;
        ShieldNow = ShieldMax;
        IsDead = false;
        //if (!isServer)
        {
            //CmdReviveButton();
        }
        //else
        {
            TargetReviveButton(this);
        }
    }
    [Command(requiresAuthority = false)]public void CmdReviveButton(NetworkConnectionToClient sender = null)
    {
        PlayerTextureGO.SetActive(true);
        LegsGO.SetActive(true);

        HealthNow = HealthMax;
        ShieldNow = ShieldMax;

        IsDead = false;
    }
    [TargetRpc]public void TargetReviveButton(UPlayer _UPlayer)
    {
        print("a");
        _UPlayer.PlayerTextureGO.SetActive(true);
        _UPlayer.LegsGO.SetActive(true);

        _UPlayer.HealthNow = _UPlayer.HealthMax;
        _UPlayer.ShieldNow = _UPlayer.ShieldMax;

        _UPlayer.IsDead = false;
    }

    [Client]
    public void SwapGrenadeButton()
    {
        GrenadesSlotUsing = GrenadesSlotUsing == 0 ? 1 : 0;
        PlayerAnimator.SetInteger("GrenadeID", WeaponInfo[GrenadesSlotUsing][0]);
    }

    public void TakeWeaponClass()
    {
        float MinDistance = 600;
        int nearestWeapon = 0;
        for (int i = 0; i < InteractiveZone.WeaponsGO.Count; )
        {
            if (MinDistance > Vector2.Distance(gameObject.transform.position, InteractiveZone.WeaponsGO[i].transform.position))
            {
                MinDistance = Vector2.Distance(gameObject.transform.position, InteractiveZone.WeaponsGO[i].transform.position);
                nearestWeapon = i;
            }
            i++;
        }

        //set values & delete original
        ushort[] InfoCache = WeaponInfo[WeaponUseIndex];
        WeaponInfo[WeaponUseIndex] = InteractiveZone.WeaponsGO[nearestWeapon].GetComponent<WeaponItemInfo>().ItemInfo[0];
        Destroy(InteractiveZone.WeaponsGO[nearestWeapon]);
        PlayerAnimator.SetInteger("WeaponID", WeaponInfo[WeaponUseIndex][0]);
        PlayerNetworkAnimator.SetTrigger("Exit");

        //create new Item
        SpawnWeaponItem = Instantiate(WeaponItemPrefab, transform.position, Quaternion.identity);
        SpawnWeapon();
        WeaponItemInfo _weaponInfoCache = SpawnWeaponItem.GetComponent<WeaponItemInfo>();
        _weaponInfoCache.CustomStart(InfoCache, 0);
        _weaponInfoCache.MustBeDestroyed = false;
    }

    public void GrenadeGet()
    {
        if (GrenadeInfo[GrenadesSlotUsing][1] > 0)
        {
            PlayerNetworkAnimator.ResetTrigger("GrenadeThrow");
            PlayerNetworkAnimator.SetTrigger("GrenadeGet");
        }
    }
    public void GrenadeThrow()
    {
        PlayerNetworkAnimator.ResetTrigger("GrenadeGet");
        PlayerNetworkAnimator.SetTrigger("GrenadeThrow");
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
                    WeaponInfo[WeaponUseIndex][3] -= (ushort)(WeaponInfo[WeaponUseIndex][2] - WeaponInfo[WeaponUseIndex][1]);
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

                if (WeaponInfo[WeaponUseIndex][1] < WeaponInfo[WeaponUseIndex][2] && WeaponInfo[WeaponUseIndex][3] > 0)
                {
                    PlayerNetworkAnimator.ResetTrigger("ReloadEnd");
                    PlayerNetworkAnimator.SetTrigger("Reload");
                }
                else
                {
                    PlayerNetworkAnimator.ResetTrigger("Reload");
                    PlayerNetworkAnimator.SetTrigger("ReloadEnd");
                }
                break;
            #endregion
        }
    }
    #endregion

    #region Grenate
    public void ScaleForceGreande_event()
    {
        ScaleForceGreande += 0.5f;
    }

    public void CreateGrenate_event()
    {
        GrenadeInfo[GrenadesSlotUsing][1]--;
        createdGrenate = Instantiate(GrenadePrefab[GrenadeInfo[GrenadesSlotUsing][0]], GrenadeSlotGO.transform.position, PlayerTextureGO.transform.rotation);
        createdGrenate.GetComponent<Grenade>().FollowFor = GrenadeSlotGO.transform;
    }

    public void ThrowGrenate_event()
    {
        if (createdGrenate != null)
        {
            Grenade createdGrenateCS = createdGrenate.GetComponent<Grenade>();
            createdGrenateCS.FollowFor = null; 
            createdGrenateCS.Time = createdGrenateCS.TimeToBoomAfterThrow;
            createdGrenateCS.sticky = createdGrenateCS.CanBeSticky? true : false;
            createdGrenateCS.Sender = PlayerTextureGO;
            createdGrenateCS.RestCor();
            Rigidbody2D createdGrenateRB = createdGrenate.GetComponent<Rigidbody2D>();
            createdGrenateRB.AddForce(createdGrenate.transform.right * ForceThrowGrenate * ScaleForceGreande, ForceMode2D.Impulse);
            createdGrenateRB.simulated = true;
        }
        ScaleForceGreande = 0;
    }
    #endregion

    #region Weapons
    public void Fire()
    {
        if (!isLocalPlayer) return;
        float RandScale;
        switch (WeaponInfo[WeaponUseIndex][0])
        {
            #region single bullet
            case 0:
            case 1:
                WeaponInfo[WeaponUseIndex][1]--;

                RandScale = UnityEngine.Random.Range(-SGRandScale, SGRandScale);
                if (isServer == true)
                    TargetSpawnBullets(RandScale);
                else 
                    CmdSpawnBullets(RandScale, TeamID);
                break;
            #endregion

            #region ShotGun
            case 2:
                WeaponInfo[WeaponUseIndex][1]--;

                for (int i = 0; i < SGBulletsRange; i++)
                {
                    RandScale = UnityEngine.Random.Range(-SGRandScale, SGRandScale);
                    //isServer == true? CmdSpawnBullets(RandScale) :  ;
                }
                break;
            #endregion
        }
    }
    [TargetRpc]public void TargetSpawnBullets(float RandScale)
    {
        GameObject ThisBulletGO = Instantiate(Bullets[WeaponInfo[WeaponUseIndex][0]], gameObject.transform.position, Quaternion.Euler(0, 0, PlayerTextureGO.transform.localRotation.eulerAngles.z + RandScale));
        Bullets ThisBulletGOCS = ThisBulletGO.GetComponent<Bullets>();
        ThisBulletGOCS.TeamId = TeamID;
        ThisBulletGO.GetComponent<Rigidbody2D>().AddForce(ThisBulletGO.transform.right * ThisBulletGOCS.BulletSpeed, ForceMode2D.Impulse);
        NetworkServer.Spawn(ThisBulletGO);
    }
    [Command(requiresAuthority = false)]public void CmdSpawnBullets(float RandScale, int Team, NetworkConnectionToClient sender = null)
    {
        GameObject ThisBulletGO = Instantiate(Bullets[WeaponInfo[WeaponUseIndex][0]], gameObject.transform.position, Quaternion.Euler(0, 0, PlayerTextureGO.transform.localRotation.eulerAngles.z + RandScale));
        Bullets ThisBulletGOCS = ThisBulletGO.GetComponent<Bullets>();
        ThisBulletGOCS.TeamId = Team;
        ThisBulletGO.GetComponent<Rigidbody2D>().AddForce(ThisBulletGO.transform.right * ThisBulletGOCS.BulletSpeed, ForceMode2D.Impulse);
        NetworkServer.Spawn(ThisBulletGO);
    }

    public void Damage(byte DamageModHealth, byte DamageModShield, float Damage, byte DieType, GameObject _GO, float ForceImpusle)
    {
        float DamagerAngle = _GO.transform.rotation.z;
        float HitAngle = Mathf.Atan2(PlayerTextureGO.transform.position.y - _GO.transform.position.y, PlayerTextureGO.transform.position.x - _GO.transform.position.x) * Mathf.Rad2Deg;
        Destroy(_GO);

        if (ShieldNow >= Damage * DamageModShield)
        {
            ShieldNow -= Damage * DamageModShield;
        }
        else if (HealthNow / DamageModHealth + ShieldNow / DamageModShield > Damage)
        {
            HealthNow -= (Damage * DamageModShield - ShieldNow) / DamageModShield * DamageModHealth;
            ShieldNow = 0;
        }
        else
        {
            // create doll
            if (_DyingDoll != null) Destroy(_DyingDoll);
            _DyingDoll = Instantiate(DyingDoll, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
            _DyingDoll.GetComponent<Animator>().SetInteger("DieType", DieType);

            // impulse
            if (DieType == 3)
            {
                for (var a = 1; a != _DyingDoll.transform.childCount; a++)
                {
                    _DyingDoll.transform.GetChild(a).gameObject.SetActive(true);
                    _DyingDoll.transform.GetChild(a).gameObject.transform.rotation = Quaternion.Euler(0, 0, HitAngle + UnityEngine.Random.Range(-15, 15));
                    Rigidbody2D _DyingDollRB = _DyingDoll.transform.GetChild(a).gameObject.GetComponent<Rigidbody2D>();
                    _DyingDollRB.AddForce(_DyingDoll.transform.GetChild(a).transform.right * (ForceImpusle + UnityEngine.Random.Range(-ForceImpusle / 100 * 20, ForceImpusle / 100 * 10)), ForceMode2D.Impulse);
                    _DyingDollRB.AddTorque(UnityEngine.Random.Range(-40, 40), ForceMode2D.Impulse);
                }
            }
            else
            {
                _DyingDoll.transform.GetChild(0).gameObject.SetActive(true);
                _DyingDoll.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, DamagerAngle);
                _DyingDoll.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(_DyingDoll.transform.right * ForceImpusle, ForceMode2D.Impulse);
            }

            // set dying
            ShieldNow = 0;
            HealthNow = 0;
            IsDead = true;
            if (DeadPanel != null)
            {
                DeadPanel.SetActive(true);
            }
            PlayerTextureGO.SetActive(false);
            LegsGO.SetActive(false);
        }
    }
    #endregion

    #region SpawnCommands
    public void SpawnWeapon()
    {
        NetworkServer.Spawn(SpawnWeaponItem);
    }
    #endregion

    #region Animator
    public void ExitAnimator_event()
    {
        PlayerNetworkAnimator.SetTrigger("Exit");
    }
    #endregion

    #region Other
    [Client]
    public void SetRandTean()
    {
        TeamID = (byte)UnityEngine.Random.Range(1, 999);
    }
    #endregion
}
