using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class RoomPlayer : MonoBehaviourPunCallbacks
{
    public Text[] PlayerText;                                       //텍스트 오브젝트들의 텍스트 컴포넌트

    private Photon.Realtime.Player[] currentPlayersInRoom;          //현재 룸 플레이어 정보
    private Photon.Realtime.Player[] playersInRoomOthersLeft;       //바로 전 상태 룸 플레이어 정보
    private int localPlayerIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentPlayersInRoom = PhotonNetwork.PlayerList;
        playersInRoomOthersLeft = PhotonNetwork.PlayerListOthers;

        printNameText();
    }


    void printNameText()
    {
        //로컬 플레이어 인덱스 찾기
        localPlayerIndex = Array.FindIndex(currentPlayersInRoom, player => player.IsLocal == true);

        //플레이어 순서는 원격 플레이어들 정보가 들어있는
        //PhotonNetwork.PlayerListOthers배열의 원소 순서대로 지정

        //Player0 출력
        PlayerText[0].text = currentPlayersInRoom[localPlayerIndex].NickName.ToString();

        //Player1 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 1)
        {
            PlayerText[1].text = PhotonNetwork.PlayerListOthers[0].NickName.ToString();
        }

        //Player2 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 2)
        {
            Debug.Log(PhotonNetwork.PlayerListOthers[1]);
            PlayerText[2].text = PhotonNetwork.PlayerListOthers[1].NickName.ToString();
        }

        //Player3 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 3)
        {
            PlayerText[3].text = PhotonNetwork.PlayerListOthers[2].NickName.ToString();
        }
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        //새 플레이어가 포함된 플레이어 정보 얻기
        //쓰이진 않는데 일단 넣어둠
        currentPlayersInRoom = PhotonNetwork.PlayerList;

        //새 플레이어가 몇번째 원격 플레이어인지 찾기
        int newPlayerindex = Array.FindIndex(PhotonNetwork.PlayerListOthers, i => i.NickName == newPlayer.NickName);

        //텍스트 출력
        for(int i = 0; i < 4; i++)
        {
            if (PlayerText[i].text == "")
            {
                PlayerText[i].text = newPlayer.NickName;
                break;
            }
        }
    

        //현 상태 플레이어 정보 남겨두기
        playersInRoomOthersLeft = PhotonNetwork.PlayerListOthers;
    }

    //기존 플레이어가 방에서 나갔을때 호출
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        //새 플레이어가 포함된 플레이어 정보 얻기
        //쓰이진 않는데 일단 넣어둠
        currentPlayersInRoom = PhotonNetwork.PlayerList;

        //나간 플레이어가 몇번째 원격 플레이어인지 찾기
        int OtherPlayerIndex = Array.FindIndex(playersInRoomOthersLeft, leftPlayer => leftPlayer.NickName == otherPlayer.NickName);

        //텍스트 출력
        for (int i = 0; i < 4; i++)
        {
            if (PlayerText[i].text == otherPlayer.NickName)
            {
                PlayerText[i].text ="";
                break;
            }
        }

        //현 상태 플레이어 정보 남겨두기
        playersInRoomOthersLeft = PhotonNetwork.PlayerListOthers;
    }
}
