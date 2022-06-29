using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;
using Mirror;

public class UPlayer : NetworkBehaviour
{
    [Header("Links")]
    [SerializeField] private Weapons _Weapons;
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private GameObject CharacterGO;
    [SerializeField] private GameObject LegsGO;
    [SerializeField] private GameObject WeaponsEmpty;
    [SerializeField] public Text HPBar;
    [SerializeField] public Text AmmoBar;
    [SerializeField] public GameObject TakeWeaponButton;
    [SerializeField] private GameObject FGrenade;
    [SerializeField] private GameObject PGrenade;
    [SerializeField] private GameObject WeaponItemPrefab;
    [SerializeField] public GameObject[] WeaponsTextures;
    [SerializeField] public bool IsReloaded;
    [SerializeField] public IEnumerator ReloadCor;
    [Header("joystick")]
    [SerializeField] private bl_Joystick LJS;
    [SerializeField] public bl_Joystick RJS;
    [Header("values")]
    private float ForceThrowGrenate = 10;
    private int FactorForceThrowGrenate;
    [SerializeField] private int TotalForceThrowGrenate;
    [SerializeField] private float speed;
    [Header("Health")]
    [SerializeField] public float HealthMax;
    [SerializeField] public float HealthNow;
    [SerializeField] public float ShieldMax;
    [SerializeField] public float ShieldNow;
    [SerializeField] public float MaxTimeToRegShield;
    [SerializeField] public float NowTimeToRegShield;
    [SerializeField] public Coroutine CorRegShield;
    [Header("PlayerSettings")]
    [SerializeField] public int[] GrenadeInfo = { 1, 2, 0, 2}; // 0-index; 1-2-vallue grenades; 3-max grenades
    // 0-index; 1-rounds in the magazine; 2-max rounds in the magazine; 3-total ammunition; 4-max ammunition
    // THIS IS THE DEFAULT FOR AR + PISTOL
    [SerializeField] public int[] WeaponInfo1 = { 0, 30, 30, 90 , 120};
    [SerializeField] public int[] WeaponInfo2 = { 1, 6, 6, 18 , 24};

    private void Start()
    {
        if (!isLocalPlayer) return;

        #region FindButtons
        LJS = GameObject.Find("LJS").GetComponent<bl_Joystick>();
        RJS = GameObject.Find("RJS").GetComponent<bl_Joystick>();
        AmmoBar = GameObject.Find("AmmoBarText").GetComponent<Text>();
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
            _Weapons.Fire();
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
        for (int i = 0; i < _Weapons.WeaponsGO.Count; )
        {
            if (MinDistance > Vector2.Distance(gameObject.transform.position, _Weapons.WeaponsGO[i].transform.position))
            {
                MinDistance = Vector2.Distance(gameObject.transform.position, _Weapons.WeaponsGO[i].transform.position);
                nearestWeapon = i;
            }
            i++;
        }

        //set values
        int[] InfoCache = WeaponInfo1;
        UniversalBridge _UniversalBridge = _Weapons.WeaponsGO[nearestWeapon].GetComponent<UniversalBridge>();
        WeaponInfo1 = _UniversalBridge._weaponInfo.ItemInfo;
        _UniversalBridge._weaponInfo.PreDestroy();
        //create new Item
        GameObject NewWeaponItem = Instantiate(WeaponItemPrefab, transform.position, Quaternion.identity);
        WeaponItemInfo _weaponInfoCache = NewWeaponItem.GetComponent<WeaponItemInfo>();
        _weaponInfoCache.CustomStart(InfoCache);
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
                yield return new WaitForSeconds(_Weapons.ARReloadSpeed);
                break;
            case 1:
                yield return new WaitForSeconds(_Weapons.PistolReloadSpeed);
                break;
            case 2:
                yield return new WaitForSeconds(_Weapons.SGReloadSpeed);
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

    public void NetWorkCreator(GameObject _ThisBulletGO)
    {
        NetworkServer.Spawn(_ThisBulletGO);
    }
}