using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponItemInfo : NetworkBehaviour
{
    [SerializeField] public bool MustBeDestroyed;
    [SerializeField] private float SecToDestroy;
    [SerializeField] private Sprite[] WeaponSprites;
    [SerializeField] private List<double[]> SizeForWeapons = new List<double[]>
    {
        new double[] { 1.48, 0.37}, //AR
        new double[] { 1.48, 0.52}, //DMR
        new double[] { 1.42, 0.37}, //ShotGun
    };
    [SerializeField] public SyncList<int[]> ItemInfo = new SyncList<int[]> { new int[] { 2, 6, 6, 99, 99, 1 }};
    public int[] a;

    public void CustomStart(int[] Info)
    {
        ItemInfo[0] = Info;
        gameObject.GetComponent<SpriteRenderer>().sprite = WeaponSprites[ItemInfo[0][0]];
        gameObject.GetComponent<CapsuleCollider2D>().size = new Vector2((float)SizeForWeapons[ItemInfo[0][0]][0], (float)SizeForWeapons[ItemInfo[0][0]][1]);
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

    private void Update()
    {
        a = ItemInfo[0];
    }
}