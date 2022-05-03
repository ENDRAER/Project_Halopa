using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class WeaponItemInfo : MonoBehaviour
{
    [SerializeField] public bool NeedToBeDestroy;
    [SerializeField] private float FloatTimeToDestroy;
    [SerializeField] private GameObject[] WeaponObjects;
    [SerializeField] public int[] ItemInfo = { 0, 30, 30, 90, 120 };

    public void CustomStart(int[] Info)
    {
        ItemInfo = Info;
        WeaponObjects[ItemInfo[0]].SetActive(true);
        if (NeedToBeDestroy == true)
        {
            StartCoroutine(TimeToDestroy());
        }
        if (ItemInfo[3]==0 && ItemInfo[1]==0)
        {
            PreDestroy();
        }
    }

    private IEnumerator TimeToDestroy()
    {
        yield return new WaitForSeconds(FloatTimeToDestroy);
        Destroy(gameObject);
    }

    public void PreDestroy()
    {
        Destroy(gameObject);
    }
}