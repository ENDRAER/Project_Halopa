using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Mirror;

public class Grenade : NetworkBehaviour
{
    [SerializeField] public bool CanBeSticky;
    [NonSerialized] public bool sticky = false;
    [SerializeField] private float Damage;
    [SerializeField] public float Impulse;
    [SerializeField] private float Distance;
    [SerializeField] public float Time = 6;
    [SerializeField] public float TimeToBoomAfterThrow;
    [SerializeField] private LayerMask ExplosionLayer;
    [SerializeField] private LayerMask RayLayer;
    [SerializeField] public GameObject Sender;
    [NonSerialized] public Coroutine coroutine;
    [SerializeField] public Transform FollowFor;
    private RaycastHit2D rayHit;
    
    private void Start()
    {
        coroutine = StartCoroutine(TimeToBoom());
    }

    public void RestCor()
    {
        StopCoroutine(coroutine);
        StartCoroutine(TimeToBoom());
    }

    public IEnumerator TimeToBoom()
    {
        //Timer until explosion
        yield return new WaitForSeconds(Time);
        GameObject Player = gameObject; // gameObject is here for delete the warning
        float MinDistance = 2286669;

        //Get all players in damage zone and set Damage
        Collider2D[] ExplosionZone = Physics2D.OverlapCircleAll(transform.position, Distance, ExplosionLayer);
        foreach (Collider2D Col2D in ExplosionZone)
        {
            if (Col2D.gameObject.GetComponents<PlayerTexture>() != null)
            {
                Col2D.gameObject.layer = 0;

                for (float t = 0; t != 180; t++)
                {
                    rayHit = Physics2D.Raycast(transform.position, transform.right, Distance, RayLayer);
                    Debug.DrawRay(transform.position, transform.right * (rayHit ? Vector3.Distance(transform.position, rayHit.point) : Distance));
                    transform.eulerAngles = new Vector3(0, 0, t * 2);

                    if (rayHit.collider != null && rayHit.collider.tag == "Player" && MinDistance > Vector3.Distance(transform.position, rayHit.point))
                    {
                        Player = rayHit.collider.gameObject;
                        MinDistance = Vector3.Distance(transform.position, rayHit.point);
                    }
                }
                if (MinDistance != 2286669 && Player != gameObject)
                {
                    float DamageNow = Damage * (1 / Distance * (Distance - MinDistance));
                    Player.GetComponent<PlayerTexture>()._UPlayer.Damage(1, 1, DamageNow, 3, gameObject, Impulse * (1 / Distance * (Distance - MinDistance)));
                }

                Col2D.gameObject.layer = 2;
            }
        }

        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D Other)
    {
        if (sticky && Other.gameObject.tag == "Player" && Other.gameObject != Sender && Sender != null)
        {

        }
    }

    private void Update()
    {
        if (FollowFor != null)
        {
            transform.rotation = FollowFor.rotation;
            transform.position = FollowFor.position;
        }
    }
}