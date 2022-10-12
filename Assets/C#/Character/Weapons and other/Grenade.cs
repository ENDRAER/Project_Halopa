using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class Grenade : MonoBehaviour
{
    [SerializeField] private float Damage;
    [SerializeField] private float Distance;
    [SerializeField] private float Time; 
    [SerializeField] private GameObject DeadZone;
    [SerializeField] private LayerMask RayLayer;
    [SerializeField] public GameObject Sender;
    private RaycastHit2D rayHit;
    
    private void Start()
    {
        StartCoroutine(TimeToBoom());
    }

    private IEnumerator TimeToBoom()
    {
        GameObject me = gameObject;
        yield return new WaitForSeconds(Time);
        float MinDistance = 2286669;
        for (float t = 0; t != 180; t++)
        {
            rayHit = Physics2D.Raycast(transform.position, DeadZone.transform.forward, Distance, RayLayer);
            Debug.DrawRay(transform.position, DeadZone.transform.forward * (rayHit ? Vector3.Distance(transform.position, rayHit.point) : Distance));
            DeadZone.transform.rotation *= Quaternion.AngleAxis(2, new Vector3(1, 0, 0));

            if (rayHit.collider != null && rayHit.collider.tag == "Player" && MinDistance > Vector3.Distance(transform.position, rayHit.point))
            {
                me = rayHit.collider.gameObject;
                MinDistance = Vector3.Distance(transform.position, rayHit.point);
            }
        }
        if (MinDistance != 2286669)
        {
            //  DAMAGE
        }
        Destroy(gameObject);
    }
    /*
    private void OnTriggerEnter2D(Collider2D Other)
    {
        if (Other.gameObject.layer != 0 && Other.gameObject != Sender)
        {
            gameObject.transform.SetParent(Other.gameObject.transform);
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }*/
}