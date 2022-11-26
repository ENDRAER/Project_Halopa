using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;
using System.Runtime.CompilerServices;
using static Bullets;
using UnityEngine.UIElements;

public class Bullets : NetworkBehaviour
{
    [SerializeField][SyncVar] public int TeamId = 0;
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerTexture>() != null)
        {
            PlayerTexture _PlayerTexture = other.gameObject.GetComponent<PlayerTexture>();
            UPlayer _UPlayer = _PlayerTexture._UPlayer;
            if (_UPlayer.TeamID != TeamId && TeamId != 0 && _UPlayer.IsDead == false)
            {
                float hitAngle = Mathf.Atan2(_PlayerTexture.transform.position.y - transform.position.y, _PlayerTexture.transform.position.x - transform.position.x) * Mathf.Rad2Deg;
                _UPlayer.Damage(DamageModHealth, DamageModShield, BulletDamage, (byte)TypeOfBullet, hitAngle, Impulse);
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
