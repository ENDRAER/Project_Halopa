using UnityEngine;
using Photon.Pun;

public class MatchSettings : MonoBehaviour
{
    [SerializeField] private GameObject Character;

    void Start()
    {
        PhotonNetwork.Instantiate(Character.name, new Vector3(0,0,0), Quaternion.identity);
    }
}
