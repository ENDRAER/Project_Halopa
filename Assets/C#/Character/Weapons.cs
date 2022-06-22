using System.Collections;
using UnityEngine;

public class Weapons : MonoBehaviour
{
    [SerializeField] public UPlayer _UPlayer;
    [SerializeField] private LayerMask Layer;
    private RaycastHit2D rayHit;

    // Balance Weapon
    [Header("Assault Rifle")]
    [SerializeField] private float ARDamage;
    [SerializeField] private bool ARReady = true;
    [SerializeField] private float ARRateOfFire;
    [SerializeField] public float ARReloadSpeed;

    [Header("Pistol")]
    [SerializeField] private float PistolDamage;
    [SerializeField] private bool PistolReady = true;
    [SerializeField] private float PistolRateOfFire;
    [SerializeField] public float PistolReloadSpeed;

    [Header("SG")]
    [SerializeField] private float SGDamage;
    [SerializeField] private bool SGReady = true;
    [SerializeField] private float SGRateOfFire;
    [SerializeField] public float SGReloadSpeed;
    [SerializeField] public float SGBulletsRange;

    //[Header("Fragmentation Grenade")]
    //[SerializeField] private float FGRateOfFire;
    //[SerializeField] private bool FGReady = true;


    public void Fire(float playerScaleX, float WeaponScaleX)
    {
        #region AR
        if (_UPlayer.WeaponInfo1[0] == 0 && ARReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            ARReady = false;
            _UPlayer.WeaponInfo1[1]--;
            {
                if (playerScaleX > 0)
                {
                    rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right), 100, Layer);
                    Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right) * 100);
                }

                else
                {
                    rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right), 100, Layer);
                    Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right) * 100);
                }
            } //Rotate RayCast

            if (rayHit.collider != null && rayHit.collider.gameObject.GetComponent<PlayerTexture>() != null)
            {
                //  DAMAGE
            } // hit
            StartCoroutine(ARWait(ARRateOfFire));
        }
        #endregion

        #region Pistol
        if (_UPlayer.WeaponInfo1[0] == 1 && PistolReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            PistolReady = false;
            _UPlayer.WeaponInfo1[1]--;
            {
                if (playerScaleX > 0)
                {
                    rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right), 100, Layer);
                    Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right) * 100);
                }

                else
                {
                    rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right), 100, Layer);
                    Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right) * 100);
                }
            } //Rotate RayCast

            if (rayHit && rayHit.collider.gameObject.GetComponent<PlayerTexture>() != null)
            {
                //  DAMAGE
            } // hit

            StartCoroutine(PistolWait(PistolRateOfFire));
        }
        #endregion

        #region ShotGun
        if (_UPlayer.WeaponInfo1[0] == 2 && SGReady == true && _UPlayer.WeaponInfo1[1] > 0 && _UPlayer.IsReloaded == false)
        {
            SGReady = false;
            _UPlayer.WeaponInfo1[1]--;
            for (int i = 0; i < SGBulletsRange; i++)
            {
                gameObject.transform.localRotation= Quaternion.Euler(0,0,Random.Range(-20f,20f));
                {
                    if (playerScaleX > 0)
                    {
                        rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right), 100, Layer);
                        Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? transform.right : -transform.right) * 100);
                    }

                    else
                    {
                        rayHit = Physics2D.Raycast(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right), 100, Layer);
                        Debug.DrawRay(transform.position, (WeaponScaleX > 0 ? -transform.right : transform.right) * 100);
                    }
                } //Rotate RayCast
                
                if (rayHit && rayHit.collider.gameObject.GetComponent<PlayerTexture>() != null)
                {
                    //  DAMAGE
                } // hit
            }
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
            StartCoroutine(SGWait(SGRateOfFire));
        }
        #endregion
    }

    private IEnumerator ARWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ARReady = true;
    }

    private IEnumerator PistolWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        PistolReady = true;
    }

    private IEnumerator SGWait(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SGReady = true;
    }
}
