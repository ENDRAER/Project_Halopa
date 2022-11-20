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
    [SerializeField] private float timeToDestroy;
    [SerializeField] public enum _TypeOfBullet { rifles, plasma, explosions };
    [SerializeField] public _TypeOfBullet TypeOfBullet;


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
                float hitAngle = Mathf.Atan2(_UPlayer.transform.position.x - transform.position.x, _UPlayer.transform.position.y - transform.position.y) * Mathf.Rad2Deg;
                switch (TypeOfBullet)
                {
                    case _TypeOfBullet.rifles:
                        if(hitAngle <= 180 && hitAngle >= -180)
                            _UPlayer.Damage(1, 1, BulletDamage, 0, hitAngle);
                        else
                            _UPlayer.Damage(1, 1, BulletDamage, 1, hitAngle);
                        break;
                    case _TypeOfBullet.plasma:
                        _UPlayer.Damage(1, 2, BulletDamage, 2, hitAngle);
                        break;
                    case _TypeOfBullet.explosions:
                        print("build me");
                        break;
                }
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
