using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;

public class Grenade : MonoBehaviour
{
    [SerializeField] private bool sticky;
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

    [Client]
    private IEnumerator TimeToBoom()
    {
        yield return new WaitForSeconds(Time);
        GameObject Player = gameObject;
        float MinDistance = 2286669;
        for (float t = 0; t != 180; t++)
        {
            rayHit = Physics2D.Raycast(transform.position, DeadZone.transform.forward, Distance, RayLayer);
            Debug.DrawRay(transform.position, DeadZone.transform.forward * (rayHit ? Vector3.Distance(transform.position, rayHit.point) : Distance));
            DeadZone.transform.rotation *= Quaternion.AngleAxis(2, new Vector3(1, 0, 0));

            if (rayHit.collider != null && rayHit.collider.tag == "Player" && MinDistance > Vector3.Distance(transform.position, rayHit.point))
            {
                Player = rayHit.collider.gameObject;
                MinDistance = Vector3.Distance(transform.position, rayHit.point);
            }
        }
        if (MinDistance != 2286669)
        {
            float DamageNow = Damage * (1 / Distance * (Distance - MinDistance));
            UPlayer _UPlayer = Player.GetComponent<PlayerTexture>()._UPlayer;
            if (_UPlayer.ShieldNow >= DamageNow)
            {
                _UPlayer.ShieldNow -= DamageNow;
            }
            else if (_UPlayer.ShieldNow < DamageNow && _UPlayer.HealthNow + _UPlayer.ShieldNow > DamageNow)
            {
                _UPlayer.HealthNow -= DamageNow - _UPlayer.ShieldNow;
                _UPlayer.ShieldNow = 0;
            }
            else
            {
                _UPlayer.IsDead = true;
                _UPlayer.ShieldNow = 0;
                _UPlayer.HealthNow = 0;
            }
        }
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D Other)
    {
        if (sticky && Other.gameObject.tag == "Player" && Other.gameObject != Sender && Sender != null)
        {
            gameObject.transform.SetParent(Other.gameObject.transform);
            Destroy(gameObject.GetComponent<Rigidbody2D>());
        }
    }
}