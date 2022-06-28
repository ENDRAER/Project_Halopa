using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public int DontDrendlyFire;
    public float BulletDamage;
    public float BulletSpeed;
    public float timeToDestroy;
    public enum _TypeOfBullet { rifles, plasma, explosions};
    public _TypeOfBullet TypeOfBullet;


    private void Start()
    {
        StartCoroutine(CorTimeToDestroy());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Weapons>() != null)
        {
            if (other.gameObject.GetComponent<Weapons>().Team != DontDrendlyFire)
            {
                switch (TypeOfBullet)
                {
                    #region rifle
                    case _TypeOfBullet.rifles:
                        if (other.gameObject.GetComponent<UPlayer>() != null)
                        {
                            UPlayer uPlayer = other.gameObject.GetComponent<UPlayer>();
                            if (uPlayer.ShieldNow >= BulletDamage)
                            {
                                uPlayer.ShieldNow -= BulletDamage;
                            }
                            else if (uPlayer.ShieldNow < BulletDamage && uPlayer.HealthNow + uPlayer.ShieldNow > BulletDamage)
                            {
                                uPlayer.HealthNow -= BulletDamage - uPlayer.ShieldNow;
                                uPlayer.ShieldNow = 0;
                            }
                            else
                            {
                                uPlayer.ShieldNow = 0;
                                uPlayer.HealthNow = 0;
                            }
                        }
                        Destroy(gameObject);
                        break;
                    #endregion

                    #region plasma
                    case _TypeOfBullet.plasma:
                        if (other.gameObject.GetComponent<UPlayer>() != null)
                        {
                            UPlayer uPlayer = other.gameObject.GetComponent<UPlayer>();
                            if (uPlayer.ShieldNow >= BulletDamage * 2)
                            {
                                uPlayer.ShieldNow -= BulletDamage * 2;
                            }
                            else if (uPlayer.ShieldNow < BulletDamage * 2 && uPlayer.HealthNow + uPlayer.ShieldNow/2 > BulletDamage)
                            {
                                uPlayer.HealthNow -= BulletDamage - uPlayer.ShieldNow/2;
                                uPlayer.ShieldNow = 0;
                            }
                            else
                            {
                                uPlayer.ShieldNow = 0;
                                uPlayer.HealthNow = 0;
                            }
                        }
                        Destroy(gameObject);
                        break;
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
