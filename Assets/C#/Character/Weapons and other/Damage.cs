using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Damage : MonoBehaviour
{
    //public float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<UPlayer>() != null)
        {
            UPlayer _UPlayer = other.GetComponent<UPlayer>();
            //_UPlayer.HealthNow -= damage;
            _UPlayer.HPBar.text = "HP: " + _UPlayer.HealthNow + "/100";
            Destroy(gameObject);
        }
    }
}
