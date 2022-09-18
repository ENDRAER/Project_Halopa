using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class WeaponItemInfo : NetworkBehaviour
{
    [SerializeField] public bool MustBeDestroyed;
    [SerializeField] private float SecToDestroy;
    [SerializeField] private Sprite[] WeaponSprites;
    [SerializeField] public SyncList<int[]> ItemInfo = new SyncList<int[]> { new int[] { 2, 6, 6, 99, 99, 1 }};

    public void CustomStart(int[] Info)
    {
        ItemInfo[0] = Info;
        gameObject.GetComponent<SpriteRenderer>().sprite = WeaponSprites[ItemInfo[0][0]];
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