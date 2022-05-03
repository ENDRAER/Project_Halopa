using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public float damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<UPlayer>() != null)
        {
            UPlayer _UPlayer = other.GetComponent<UPlayer>();
            _UPlayer.Health -= damage;
            _UPlayer.HPBar.text = "HP: " + _UPlayer.Health + "/100";
            Destroy(gameObject);
        }
    }
}
