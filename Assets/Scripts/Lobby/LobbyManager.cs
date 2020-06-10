using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private readonly string gameVersion = "1";

    public Text connectionInfoText;
    public Button joinButton;

    // Start is called before the first frame update
    void Start()
    {

        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        joinButton.interactable = false;
        connectionInfoText.text = "Connection To Master Server...";

    }
    
    
    public override void OnConnectedToMaster()
    {
        joinButton.interactable = true;
        connectionInfoText.text = "Online : Connection To Master Server";
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        joinButton.interactable = false;
        connectionInfoText.text = $"Offlin : Connection Disabled{cause.ToString()}";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Connect()
    {
        joinButton.interactable = false;
        if (PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting to Random Rom...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            connectionInfoText.text = $"Offlin : Connection Disabled-Try";
            PhotonNetwork.ConnectUsingSettings();

        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "방이 없습니다. 새방을 생성합니다.";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });

    }
    public override void OnJoinedRoom()
    {
        connectionInfoText.text = "방에 연결중입니다";
        PhotonNetwork.LoadLevel("Game");//게임 신 이름 넣기
    }
   

}
