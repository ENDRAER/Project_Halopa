using UnityEngine.SceneManagement;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class Lobby : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text HostName;
    [SerializeField] private Text NumberOfPlayers;
    [SerializeField] private int NumberOfPlayersInt = 1;

    private void Start()
    {
        //HostName.text = PhotonNetwork.NickName;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        NumberOfPlayersInt++;
        NumberOfPlayers.text = NumberOfPlayersInt + "/8";
    }

    private void OnPlayerDisconnected(Player player)
    {
        NumberOfPlayersInt--;
        NumberOfPlayers.text = NumberOfPlayersInt + "/8";
    }

    public void LeaveLobby()
    {
        PhotonNetwork.LeaveLobby();
        SceneManager.LoadScene(0);
    }

    public void StartMatch(int Scene)
    {
        SceneManager.LoadScene(Scene);
    }
}
