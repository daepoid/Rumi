using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;

public class PlayerTextManager : MonoBehaviourPunCallbacks
{

    public Text[] PlayerText;                                       //텍스트 오브젝트들의 텍스트 컴포넌트

    private Photon.Realtime.Player[] currentPlayersInRoom;          //현재 룸 플레이어 정보
    private Photon.Realtime.Player[] playersInRoomOthersLeft;       //바로 전 상태 룸 플레이어 정보
    private int localPlayerIndex;
    
    void Start()
    {
        currentPlayersInRoom = PhotonNetwork.PlayerList;
        playersInRoomOthersLeft = PhotonNetwork.PlayerListOthers;

        printNameText();
    }

    //게임 시작버튼 눌렀을 때
    public void ClickStartButton()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            getCardNumberList();
            photonView.RPC("StartCardNumberText", RpcTarget.All, getCardNumberList());

        }
    }

    //카드 갯수 리스트 얻어오기
    int[] getCardNumberList()
    {
        int[] cardlist = { 0, 0, 0, 0 };

        for(int i = 0; i < currentPlayersInRoom.Length; i++)
        {
            cardlist[i] = MyGameManager.PLAYERS[i].card_num;
        }

        return cardlist;
    }


    [PunRPC]
    void StartCardNumberText(int[] playersCardNumber)
    {
        int passLocal = 0;
        //player0 카드 갯수 출력
        PlayerText[1].text = playersCardNumber[localPlayerIndex].ToString();

        //다른 플레이어 카드 갯수 출력
        for(int i = 0; i < currentPlayersInRoom.Length; i++)
        {
            if (i == localPlayerIndex)
            {
                PlayerText[1].text = playersCardNumber[localPlayerIndex].ToString();
                continue;
            }
            PlayerText[2 * (i - passLocal) + 1].text = playersCardNumber[i].ToString();
        }
    }
    
    /*
    //게임 중 확인버튼 늘렀을 때
    public void ClickOkButton()
    {
        photonView.RPC("PrintCardNumberText", RpcTarget.All, currentPlayersInRoom[localPlayerIndex]);
    }

    [PunRPC]
    void updateCardNumberText(Player currentPlayer)
    {
        //원격 플레이어 중 있는지 확인
        //있으면 카드 갯수 수정
        try
        {
            int playerIndex = Array.FindIndex(playersInRoomOthersLeft, x => x.NickName == currentPlayer.NickName);
            Player player = playersInfo.Find(x => x.NickName == currentPlayersInRoom[playerIndex].NickName);
            PlayerText[2 * playerIndex + 1].text = player.cards.Length.ToString();
        }
        //Array.FindIndex에서 못찾을 경우
        //로컬 플레이어 카드 갯수 수정
        catch(ArgumentNullException exception)
        {
            Player player = playersInfo.Find(x => x.NickName == currentPlayersInRoom[localPlayerIndex].NickName);
            PlayerText[1].text = player.cards.Length.ToString();
        }
    }
    */
    
    //텍스트 출력
    void printNameText()
    {
        //로컬 플레이어 인덱스 찾기
        localPlayerIndex = Array.FindIndex(currentPlayersInRoom, player => player.IsLocal == true);

        //플레이어 순서는 원격 플레이어들 정보가 들어있는
        //PhotonNetwork.PlayerListOthers배열의 원소 순서대로 지정
        
        //Player0 출력
        PlayerText[0].text = currentPlayersInRoom[localPlayerIndex].NickName.ToString();
        PlayerText[1].text = "준비중";

        //Player1 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 1)
        {
            PlayerText[2].text = PhotonNetwork.PlayerListOthers[0].NickName.ToString();
            PlayerText[3].text = "준비중";
        }

        //Player2 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 2)
        {
            Debug.Log(PhotonNetwork.PlayerListOthers[1]);
            PlayerText[4].text = PhotonNetwork.PlayerListOthers[1].NickName.ToString();
            PlayerText[5].text = "준비중";
        }

        //Player3 존재시 출력
        if (PhotonNetwork.PlayerListOthers.Length >= 3)
        {
            PlayerText[6].text = PhotonNetwork.PlayerListOthers[2].NickName.ToString();
            PlayerText[7].text = "준비중";
        }
    }

    //새로운 플레이어가 방에 들어왔을 때 호출
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        //새 플레이어가 포함된 플레이어 정보 얻기
        //쓰이진 않는데 일단 넣어둠
        currentPlayersInRoom = PhotonNetwork.PlayerList;

        //새 플레이어가 몇번째 원격 플레이어인지 찾기
        int newPlayerindex = Array.FindIndex(PhotonNetwork.PlayerListOthers, i => i.NickName == newPlayer.NickName);

        //텍스트 출력
        PlayerText[newPlayerindex * 2 + 2].text = newPlayer.NickName;
        PlayerText[newPlayerindex * 2 + 3].text = "준비중";

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
        PlayerText[OtherPlayerIndex * 2 + 2].text = "";
        PlayerText[OtherPlayerIndex * 2 + 3].text = "";

        //현 상태 플레이어 정보 남겨두기
        playersInRoomOthersLeft = PhotonNetwork.PlayerListOthers;
    }
}