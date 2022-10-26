using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;
using System.Runtime.CompilerServices;

public class Bullets : NetworkBehaviour
{
    [SerializeField][SyncVar] public int TeamId = 0;
    [SerializeField] private float BulletDamage;
    [SerializeField] public float BulletSpeed;
    [SerializeField] private float timeToDestroy;
    [SerializeField] private enum _TypeOfBullet { rifles, plasma, explosions };
    [SerializeField] private _TypeOfBullet TypeOfBullet;


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
                Damage(_UPlayer);
            }
        }
        else if (other.gameObject.GetComponent<UPlayer>() == null)
            Destroy(gameObject);
    }

    [Client]
    public void Damage(UPlayer _UPlayer)
    {
        switch (TypeOfBullet)
        {
            #region rifle
            case _TypeOfBullet.rifles:
                if (_UPlayer.ShieldNow >= BulletDamage)
                {
                    _UPlayer.ShieldNow -= BulletDamage;
                }
                else if (_UPlayer.ShieldNow < BulletDamage && _UPlayer.HealthNow + _UPlayer.ShieldNow > BulletDamage)
                {
                    _UPlayer.HealthNow -= BulletDamage - _UPlayer.ShieldNow;
                    _UPlayer.ShieldNow = 0;
                }
                else
                {
                    _UPlayer.IsDead = true;
                    _UPlayer.ShieldNow = 0;
                    _UPlayer.HealthNow = 0;
                }
                break;
            #endregion

            #region plasma
            case _TypeOfBullet.plasma:
                if (_UPlayer.ShieldNow >= BulletDamage * 2)
                {
                    _UPlayer.ShieldNow -= BulletDamage * 2;
                }
                else if (_UPlayer.ShieldNow < BulletDamage * 2 && _UPlayer.HealthNow + _UPlayer.ShieldNow / 2 > BulletDamage)
                {
                    _UPlayer.HealthNow -= BulletDamage - _UPlayer.ShieldNow / 2;
                    _UPlayer.ShieldNow = 0;
                }
                else
                {
                    _UPlayer.IsDead = true;
                    _UPlayer.ShieldNow = 0;
                    _UPlayer.HealthNow = 0;
                }
                break;
                #endregion
        }
    }

    public IEnumerator CorTimeToDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }
}
