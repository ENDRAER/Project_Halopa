using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Mirror;

public class MultiPlayerManager : NetworkBehaviour
{
    #region Links
    public GameObject _UPlayerGO;
    public UPlayer _UPlayerCS;
    #endregion

    #region Values
    public SyncList<ushort[]> StartWeaponInfo = new SyncList<ushort[]> { new ushort[] { 0, 30, 30, 999, 999, 0 }, new ushort[] { 2, 6, 6, 30, 30, 1 } };
    public SyncList<byte[]> StartGrenadeInfo = new SyncList<byte[]> { new byte[] { 0, 2, 2 }, new byte[] { 1, 2, 2 } };
    public int HealthMax;
    public int ShieldMax;
    #endregion


    [Command]public void ReviveButton()
    {
        _UPlayerGO.SetActive(true);
        _UPlayerGO.transform.position = _UPlayerCS.Spawns[1].transform.position;

        _UPlayerCS.WeaponInfo = StartWeaponInfo;
        _UPlayerCS.WeaponUseIndex = 0;

        _UPlayerCS.GrenadeInfo = StartGrenadeInfo;
        _UPlayerCS.GrenadesSlotUsing = 0;

        _UPlayerCS.HealthNow = HealthMax;
        _UPlayerCS.ShieldNow = ShieldMax;

        _UPlayerCS.IsDead = false;
        _UPlayerCS.DeadPanel.SetActive(false);
    }
}
