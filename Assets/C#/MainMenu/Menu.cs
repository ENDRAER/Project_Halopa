using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;

public class Menu : MonoBehaviourPunCallbacks
{
    [Header("MainMenu")]
    [SerializeField] private Text NameText;
    [SerializeField] private Text NamePlaceHolder;
    [Header("ServerCreate")]
    [SerializeField] private Text JoinServerName;
    [SerializeField] private Text CreateServerName;


    void Start()
    {
        // Name
        NamePlaceHolder.text = PlayerPrefs.GetString("UrName");
        PhotonNetwork.NickName = NamePlaceHolder.text.ToString();
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("ServerConnected");
    }

    // MainMenu
    public void ChangeName()
    {
        PhotonNetwork.NickName = NameText.ToString();
        PlayerPrefs.SetString("UrName", NameText.text.ToString());
    }

    // Servers
    public void createServer()
    {
        PhotonNetwork.CreateRoom(CreateServerName.text.ToString(), new Photon.Realtime.RoomOptions { MaxPlayers = 8 });
        SceneManager.LoadScene(1);
    }

    public void ConnectToServer()
    {
        PhotonNetwork.JoinRoom(JoinServerName.text.ToString());
    }

    //other
    public void CloseMenu(GameObject Close)
    {
        Close.SetActive(false);
    }
    public void OpenMenu(GameObject Open)
    {
        Open.SetActive(true);
    }
}
