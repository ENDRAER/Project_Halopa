using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;
using System.Diagnostics;

public class Bullets : MonoBehaviour
{
    [SerializeField] public bool BisServer;
    [SerializeField] public int TeamId = 0;
    [SerializeField] private float DecayRate;
    [SerializeField] private GameObject Trace;
    [SerializeField] public bullets_trace CreatedTraceCS;
    [SerializeField] public float BulletDamage;
    [SerializeField] public float BulletSpeed;
    [SerializeField] public float Impulse;
    [SerializeField] private float timeToDestroy;
    [SerializeField] public enum _TypeOfBullet { rifles, plasma, explosions };
    [SerializeField] public _TypeOfBullet TypeOfBullet;
    [SerializeField] private byte DamageModHealth;
    [SerializeField] private byte DamageModShield;


    private void Start()
    {
        StartCoroutine(CorTimeToDestroy());
        CreatedTraceCS = Instantiate(Trace, transform.position, transform.rotation).GetComponent<bullets_trace>();
        CreatedTraceCS.DecayRate = DecayRate;
        CreatedTraceCS.CurrectPosition = transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerTexture>() != null)
        {
            PlayerTexture _PlayerTexture = other.gameObject.GetComponent<PlayerTexture>();
            UPlayer _UPlayer = _PlayerTexture._UPlayer;
            if (_UPlayer.TeamID != TeamId && TeamId != 0 && _UPlayer.IsDead == false && BisServer)
            {
                _UPlayer.Damage(DamageModHealth, DamageModShield, BulletDamage, (byte)TypeOfBullet, gameObject, Impulse); print("TheShit");
            }
        }
        else if (other.gameObject.GetComponent<UPlayer>() == null)
            Destroy(gameObject);
    }


    public IEnumerator CorTimeToDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }
}
