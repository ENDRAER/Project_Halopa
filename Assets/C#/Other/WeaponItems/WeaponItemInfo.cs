using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponItemInfo : NetworkBehaviour
{
    [SerializeField][SyncVar] public ushort ItemType; //  0 - Weapon, 1 - Grenade, 2 - Ability
    [SerializeField] public SyncList<ushort[]> ItemInfo = new SyncList<ushort[]> { new ushort[] { 2, 6, 6, 99, 99, 1 } }; // 1 - Items id, 2 - ammo value, 3 - max am val, 5 - magazine type(for weapons)
    [SerializeField] public bool MustBeDestroyed;
    [SerializeField] private float SecToDestroy;

    [Header("Weapons")]
    [SerializeField] private Sprite[] WeaponSprites;
    [SerializeField] private List<double[]> SizeForWeapons = new List<double[]>
    {
        new double[] { 1.48, 0.37}, // AR
        new double[] { 1.48, 0.52}, // DMR
        new double[] { 1.42, 0.37}, // ShotGun
    };

    [Header("Grenade")]
    [SerializeField] private Sprite[] GrenadeSprites;
    [SerializeField] private List<double[]> SizeForGrenades = new List<double[]>
    {
        new double[] { 0.21, 0.21}, // Fragmentation
        new double[] { 0.21, 0.21}, // Plasma
    };

    [Header("Ability")]
    [SerializeField] private Sprite[] AbilitySprites;
    [SerializeField] private List<double[]> SizeForAbility = new List<double[]>
    {
        new double[] { 1.48, 0.37}, // Null !!!!!!!!!!!!!
    };



    public void CustomStart(ushort[] Info, ushort itemType)
    {
        ItemType = itemType; ItemInfo[0] = Info;
        switch (ItemType) {
            case 0:
                gameObject.GetComponent<SpriteRenderer>().sprite = WeaponSprites[ItemInfo[0][0]];
                gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2((float)SizeForWeapons[ItemInfo[0][0]][0], (float)SizeForWeapons[ItemInfo[0][0]][1]);
                break;
            case 1:
                gameObject.GetComponent<SpriteRenderer>().sprite = GrenadeSprites[ItemInfo[0][0]];
                gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2((float)SizeForGrenades[ItemInfo[0][0]][0], (float)SizeForGrenades[ItemInfo[0][0]][1]);
                break;
            case 2:
                gameObject.GetComponent<SpriteRenderer>().sprite = AbilitySprites[ItemInfo[0][0]];
                gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2((float)SizeForAbility[ItemInfo[0][0]][0], (float)SizeForAbility[ItemInfo[0][0]][1]);
                break;
        }; // set textures

        if (MustBeDestroyed == true)
        {
            StartCoroutine(TimeToDestroy());
        }
        if (ItemInfo[0][3] == 0 && ItemInfo[0][1] == 0) 
        {
            PreDestroy();
        }
    }

    private IEnumerator TimeToDestroy()
    {
        yield return new WaitForSeconds(SecToDestroy);
        Destroy(gameObject);
    }
    public void PreDestroy()
    {
        Destroy(gameObject);
    }
}