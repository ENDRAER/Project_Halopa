using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;
using System;

public class UPlayer : MonoBehaviour, IPunObservable
{
    [Header("Links")]
    [SerializeField] private Weapons _Weapons;
    [SerializeField] private PlayerTexture _playerTexture;
    [SerializeField] private Rigidbody2D PlayerRB;
    [SerializeField] private GameObject CharacterGO;
    [SerializeField] private GameObject LegsGO;
    [SerializeField] private GameObject WeaponsEmpty;
    [SerializeField] private PhotonView photonView;
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
    [SerializeField] private bl_Joystick RJS;
    [SerializeField] private GameObject _RUlookAt;
    [SerializeField] private GameObject _LUlookAt;
    [SerializeField] private GameObject _RLookAt;
    [SerializeField] private GameObject _LLookAt;
    [Header("values")]
    private float ForceThrowGrenate = 10;
    private int FactorForceThrowGrenate;
    [SerializeField] private int TotalForceThrowGrenate;
    [SerializeField] private float speed;
    [SerializeField] public float Health;
    [Header("PlayerSettings")]
    [SerializeField] public int[] GrenadeInfo = { 1, 2, 0, 2}; // 0-index; 1-2-vallue grenades; 3-max grenades
    [SerializeField] public int[] WeaponInfo1 = { 0, 30, 30, 90 , 120};
    [SerializeField] public int[] WeaponInfo2 = { 1, 6, 6, 18 , 24};
    // 0-index; 1-rounds in the magazine; 2-max rounds in the magazine; 3-total ammunition; 4-max ammunition
    // THIS IS THE DEFAULT FOR AR + PISTOL

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(WeaponInfo1[0]);
            stream.SendNext(Health);
        }
        else
        {
            WeaponInfo1[0] = (int)stream.ReceiveNext();
            Health = (float)stream.ReceiveNext();
        }
    }

    void Start()
    {
        //Find and sets
        if (photonView.IsMine)
        {
            WeaponsEmpty.layer = 9;
            HPBar = GameObject.Find("HPBar").GetComponent<Text>();
            AmmoBar = GameObject.Find("AmmoBar").GetComponent<Text>();
            GameObject.Find("Main Camera").transform.SetParent(CharacterGO.transform, false);
            GameObject.Find("SwapWeapon").GetComponent<Button>().onClick.AddListener(SwapWeaponClass);
            GameObject.Find("SwapGrenade").GetComponent<Button>().onClick.AddListener(SwapGrenadeClass);
            GameObject.Find("Reload").GetComponent<Button>().onClick.AddListener(ReloadStartClass);
            TakeWeaponButton = GameObject.Find("TakeWeapon");
            TakeWeaponButton.SetActive(false);
            TakeWeaponButton.GetComponent<Button>().onClick.AddListener(TakeWeaponClass);
            {
                LJS = GameObject.Find("JoystickL").GetComponent<bl_Joystick>();
                RJS = GameObject.Find("JoystickR").GetComponent<bl_Joystick>();

                // lookAt Stick
                _RUlookAt = GameObject.Find("RLookAtAss");
                _LUlookAt = GameObject.Find("LLookAtAss");
                _RLookAt = GameObject.Find("RStick");
                _LLookAt = GameObject.Find("LStick");
            }//Sticks
            {
                EventTrigger ThrowGrenateButton = GameObject.Find("ThrowGrenade").GetComponent<EventTrigger>();
                //Enter
                EventTrigger.Entry Enter = new EventTrigger.Entry();
                Enter.eventID = EventTriggerType.PointerEnter;
                Enter.callback.AddListener((data) => { ForceGrenateClass(); });
                ThrowGrenateButton.triggers.Add(Enter);
                //Exit
                EventTrigger.Entry Exit = new EventTrigger.Entry();
                Exit.eventID = EventTriggerType.PointerExit;
                Exit.callback.AddListener((data) => { ThrowGrenateClass(); });
                ThrowGrenateButton.triggers.Add(Exit);
            }//ThrowGrenateButton
        }
    }

    void Update()
    {
        {
            AmmoBar.text = (WeaponInfo1[1] + " | " + WeaponInfo1[3]);
        } // REBUILD THIS AND HARRY UP *****************************

        {
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
        } // change textures

        if (photonView.IsMine)
        {
            // reload first weapon
            if (WeaponInfo1[1] < 1 && IsReloaded == false && WeaponInfo1[3] != 0)
            {
                StartCoroutine(ReloadCor = RealoadIE());
            } 

            double KatetLJS = Math.Sqrt((LJS.Vertical * LJS.Vertical) + (LJS.Horizontal * LJS.Horizontal));
            double KatetRJS = Math.Sqrt((RJS.Vertical * RJS.Vertical) + (RJS.Horizontal * RJS.Horizontal));

            //weapon rot
            _RUlookAt.transform.LookAt(_RLookAt.transform);
            _LUlookAt.transform.LookAt(_LLookAt.transform);

            //player
            { 
                PlayerRB.AddForce(new Vector2(LJS.Horizontal * speed * Time.deltaTime, LJS.Vertical * speed * Time.deltaTime)); //move
                if (KatetLJS > 0.1) 
                {
                    LegsGO.transform.rotation = Quaternion.Euler(0, 0, LJS.Horizontal <= 0 ? _LUlookAt.transform.eulerAngles.x : -_LUlookAt.transform.eulerAngles.x);
                    WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, LJS.Horizontal <= 0 ? _LUlookAt.transform.eulerAngles.x : -_LUlookAt.transform.eulerAngles.x);
                } // rotate Player

                if (KatetRJS > 0.2)
                {
                    WeaponsEmpty.transform.rotation = Quaternion.Euler(0, 0, RJS.Horizontal <= 0 ? _RUlookAt.transform.eulerAngles.x : -_RUlookAt.transform.eulerAngles.x);
                } // rotate

                if (KatetRJS > 4.8)
                {
                    _Weapons.Fire(RJS.Horizontal, WeaponsEmpty.transform.localScale.x);
                } // fire Weapon
            }
        }
    }

    private void FixedUpdate()
    {
        ForceThrowGrenate += ForceThrowGrenate > 30 ? 0 : 0.2f * FactorForceThrowGrenate;
    }

    public void SwapWeaponClass()
    {
        if (ReloadCor != null) { StopCoroutine(ReloadCor); }
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
        GameObject NewWeaponItem = PhotonNetwork.Instantiate(WeaponItemPrefab.name, transform.position, Quaternion.identity);
        WeaponItemInfo _weaponInfoCache = NewWeaponItem.GetComponent<WeaponItemInfo>();
        _weaponInfoCache.CustomStart(InfoCache);
    }

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
        {
            if (WeaponInfo1[0] == 0)
            {
                yield return new WaitForSeconds(_Weapons.ARReloadSpeed);
            }

            if (WeaponInfo1[0] == 1)
            {
                yield return new WaitForSeconds(_Weapons.PistolReloadSpeed);
            }

            if (WeaponInfo1[0] == 1)
            {
                yield return new WaitForSeconds(_Weapons.SGReloadSpeed);
            }
        } // Speed Reloading
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

    public void ForceGrenateClass()
    {
        ForceThrowGrenate = 10;
        FactorForceThrowGrenate = 1;
    }
    public void ThrowGrenateClass()
    {
        if (GrenadeInfo[GrenadeInfo[0]]==0) { return; }
        GameObject createdGrenate = PhotonNetwork.Instantiate(GrenadeInfo[0] == 1? FGrenade.name : PGrenade.name, transform.position, Quaternion.identity);
        createdGrenate.GetComponent<Grenade>().Sender = WeaponsEmpty;
        Rigidbody2D createdGrenateRB = createdGrenate.GetComponent<Rigidbody2D>();
        createdGrenateRB.AddForce(new Vector2(RJS.Horizontal * ForceThrowGrenate * TotalForceThrowGrenate, RJS.Vertical * ForceThrowGrenate * TotalForceThrowGrenate));
        Debug.Log(new Vector2(RJS.Vertical, RJS.Vertical));
    }
}