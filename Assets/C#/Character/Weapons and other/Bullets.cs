using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Bullets : NetworkBehaviour
{
    [SyncVar] public int DontFrendlyFire;
    public float BulletDamage;
    public float BulletSpeed;
    public float timeToDestroy;
    public enum _TypeOfBullet { rifles, plasma, explosions};
    public _TypeOfBullet TypeOfBullet;

    private void Start()
    {
        gameObject.GetComponent<Rigidbody2D>().AddForce(transform.right * BulletSpeed, ForceMode2D.Impulse);
        StartCoroutine(CorTimeToDestroy());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<PlayerTexture>() != null)
        {
            PlayerTexture _PlayerTexture = other.gameObject.GetComponent<PlayerTexture>();
            UPlayer _UPlayer = _PlayerTexture._UPlayer;

            if (_UPlayer.TeamID != DontFrendlyFire && _UPlayer.IsDead == false)
            {
                switch (TypeOfBullet)
                {
                    #region rifle
                    case _TypeOfBullet.rifles:
                        {
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
                                _UPlayer.DeadPanel.SetActive(true);
                                _UPlayer.ShieldNow = 0;
                                _UPlayer.HealthNow = 0;
                            }
                            Destroy(gameObject);
                            break;
                        }
                    #endregion

                    #region plasma
                    case _TypeOfBullet.plasma:
                        {
                            if (_UPlayer.ShieldNow >= BulletDamage * 2)
                            {
                                _UPlayer.ShieldNow -= BulletDamage * 2;
                            }
                            else if (_UPlayer.ShieldNow < BulletDamage * 2 && _UPlayer.HealthNow + _UPlayer.ShieldNow/2 > BulletDamage)
                            {
                                _UPlayer.HealthNow -= BulletDamage - _UPlayer.ShieldNow/2;
                                _UPlayer.ShieldNow = 0;
                            }
                            else
                            {
                                _UPlayer.IsDead = true;
                                _UPlayer.ShieldNow = 0;
                                _UPlayer.HealthNow = 0;
                            }
                            Destroy(gameObject);
                            break;
                        }
                    #endregion
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator CorTimeToDestroy()
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }
}
