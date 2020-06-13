﻿using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("게임메니저 시작\n");

        Button_Next.enabled = false;
        Button_Next.GetComponent<Image>().color = Color.gray;
        Button_Reset.enabled = false;
        Button_Reset.GetComponent<Image>().color = Color.gray;

        if (!PhotonNetwork.IsMasterClient)
        {
            Button_Start.enabled = false;
            Button_Start.GetComponent<Image>().color = Color.gray;
        }
    }

    //=========================================================================
    // start 버튼을 누르면 시작하는 함수
    // 설명
    // 1. start버튼을 누르면 실행됩니다.
    // 2. 게임이 시작됩니다.
    // 3. 게임 순서를 랜덤으로 정합니다.
    //=========================================================================
    public void GameStart()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.UserId + "\n");

        Initialize();                                                     // DECK, PLAYERS Clear()
        Create_DECK();                                                    // DECK 인스턴스 생성
        Create_PLAYERS();                                                 // PLAYERS 인스턴스 생성
        photonView.RPC("Set_playerNum", RpcTarget.All);                   // 플레이어 번호 지정
        photonView.RPC("Create_TABLE", RpcTarget.All);                    // TABLE 인스턴스 생성
        Divide_Card();                                                    // 마스터(서버)가 카드를 나누어줌
        Sync_Card();                                                      // 마스터(서버)가 배분한 카드를 동기화 시킵니다.
        photonView.RPC("View_Card", RpcTarget.All);                       // 자신이 받은 카드를 확인합니다.
        photonView.RPC("View_TABLE", RpcTarget.All);                      // TABLE 게임판 확인하기

        // 게임의 턴을 설정합니다.
        Random random = new Random();
        Turn = (int)(random.NextDouble() * 1000 % PLAYERS.Count);
        photonView.RPC("Sync_Turn", RpcTarget.All, Turn);


        //버튼, 테이블을 셋팅합니다.
        photonView.RPC("Backup", RpcTarget.All);

        // 게임 시작
        photonView.RPC("Set_RunningGame", RpcTarget.All, 1);

        /* 에러 방지용 주석
        photonView.RPC("PrintPlayer0CardText", RpcTarget.All);  //Player0 카드 갯수 출력
        photonView.RPC("PrintPlayer1CardText", RpcTarget.All);  //Player1 카드 갯수 출력
        photonView.RPC("PrintPlayer2CardText", RpcTarget.All);  //Player2 카드 갯수 출력
        photonView.RPC("PrintPlayer3CardText", RpcTarget.All);  //Player3 카드 갯수 출력
        */
    }
    //=========================================================================
    // 게임 시작 플래그
    // 설명
    // 1. 게임이 시작되었음을 알리는 플래그입니다.
    //=========================================================================
    [PunRPC]
    void Set_RunningGame(int flag)
    {
        RunningGame = flag;
    }
    //=========================================================================
    // 플레이어 번호 지정
    // 설명
    // 1. 플레이어가 네트워크로 부터 받은 번호를 저장합니다.
    //=========================================================================
    [PunRPC]
    void Set_playerNum()
    {
        Debug.Log("플레이어 번호 지정 시작");

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                PlayerNum = i;
                PLAYER_COUNT = PhotonNetwork.PlayerList.Length;
                Debug.Log("     플레이어번호 : " + PlayerNum);
                Debug.Log("     플레이어 수 : " + PLAYER_COUNT);
                Debug.Log("플레이어 번호 지정 끝");
                return;
            }
        }
        
    }
    //=========================================================================
    // 덱, 플레이어 초기화
    // 설명
    // 1. 카드와 플레이어 인스턴스를 초기화합니다.
    //=========================================================================
    void Initialize()
    {
        DECK.Clear();            // 카드 초기화
        PLAYERS.Clear();         // 플레이어 초기화
    }
    //=========================================================================
    // 카드 생성 - 마스터만 실행
    // 설명
    // 1. 덱을 새롭게 생성합니다.
    // 2. 마스터만 자신의 덱을 섞습니다.
    //=========================================================================
    void Create_DECK()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드를 생성하고 있습니다.");
            return;
        }

        Debug.Log("카드 생성 시작\n");
        for (int i = 0; i < 104; i++)
        {
            Card c = new Card();
            c.number = ((i % 13) + 1).ToString();
            switch ((i / 26))
            {
                case 0: c.color = "red"; break;
                case 1: c.color = "blue"; break;
                case 2: c.color = "yellow"; break;
                case 3: c.color = "black"; break;
            }
            DECK.Add(c);
        }

        DECK.Add(new Card() { number = "J", color = "red" });
        DECK.Add(new Card() { number = "J", color = "black" });

        // 마스터 클라이언트만 카드를 섞습니다.
        if (PhotonNetwork.IsMasterClient)
            shuffle();

        Debug.Log("카드 생성 완료");
    }

    // 카드 섞기 - opencard()에서 사용
    void shuffle()
    {
        Debug.Log("마스터 : 카드 섞기 시작\n");
        int i, j;
        Random random = new Random();

        for (i = 0; i < DECK.Count; i++)
        {
            j = ((int)(random.NextDouble() * 1000)) % 106;
            swap(i, j);

        }

        Debug.Log("마스터 : 카드 섞기 완료\n");
    }

    // 카드 바꾸기 - shuffle()에서 사용
    void swap(int sour, int dest)
    {
        Card temp = DECK[sour];
        DECK[sour] = DECK[dest];
        DECK[dest] = temp;
    }

    //=========================================================================
    // PLAYERS 인스턴스 생성
    // 설명
    // 1. PLAYER의 인스턴스를 게임에 참여한 수에 맞게 설정합니다.
    //=========================================================================
    void Create_PLAYERS()
    {
        Debug.Log("플레이어 인스턴스 생성 시작");
        Player p;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            p = new Player();
            p.NickName = PhotonNetwork.PlayerList[i].NickName;
            PLAYERS.Add(p);
        }
        Debug.Log("     입장한 플레이어 수: " + PhotonNetwork.PlayerList.Length + "\n");
        Debug.Log("     플레이어 인스턴스 수 : " + PLAYERS.Count);
        Debug.Log("플레이어 인스턴스 생성 끝");
    }

    //=========================================================================
    // TABLE 인스턴스 생성
    // 설명
    // 1. TABLE 배열의 인스턴스를 생성합니다.
    //=========================================================================
    [PunRPC]
    void Create_TABLE()
    {
        Debug.Log("TABLE 인스턴스 생성 시작");
        for (int raw = 0; raw < RAW_TABLE; raw++)
        {
            for (int col = 0; col < COL_TABLE; col++)
            {
                TABLE[raw, col] = new Card() { number = "", color = "" };
            }
        }
        Debug.Log("TABLE 인스턴스 생성 끝");
    }

    //=========================================================================
    //카드 분배하기 - 마스터만 실행
    // 설명
    // 1. 마스터가 섞은 카드를 PLAYERS.Cards 배열에 분배합니다.
    //=========================================================================
    void Divide_Card()
    {
        Debug.Log("카드 분배 시작\n");

        //카드 분배, 멀티 플레이용
        // ## PLAYERS.count 사용 시 다른 사람이 접속이 끊겨도 계속 실행됩니다.
        try
        {
            for (int j = 0; j < PLAYERS.Count; j++)
            {
                for (int i = 0; i < 22; i++)
                {
                    PLAYERS[j].cards[i] = DECK[DECK.Count - 1];
                    DECK.RemoveAt(DECK.Count - 1);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("     Error : 오류가 발생했습니다. \n");
            return;
        }

        Debug.Log("카드 분배 완료\n");
    }

    //=========================================================================
    //카드의 싱크를 맞추는 함수 - 마스터만 실행
    // 설명
    // 1. 마스터가 PLAYERS.Cards 배열에 분배한 데이터를 모든 클라이언트에게 동기화 시킵니다.
    //=========================================================================
    void Sync_Card()
    {
        Debug.Log("카드 동기화 시작\n");

        Debug.Log("     입장한 플레이어 수: " + PhotonNetwork.PlayerList.Length + "\n");
        Debug.Log("     플레이어 인스턴스 수 : " + PLAYERS.Count);


        string[] num_col = { "", "" };

        // 플레이어 카드 동기화
        for (int playerIndex = 0; playerIndex < PhotonNetwork.PlayerList.Length; playerIndex++)
        {
            Debug.Log("     " + playerIndex + "플레이어 카드 동기화 시작");

            for (int cardIndex = 0; cardIndex < PLAYERS[playerIndex].cards.Length; cardIndex++)
            {
                // 마스터가 분배한 카드의 정보를 담슴니다.
                num_col[0] = PLAYERS[playerIndex].cards[cardIndex].number;
                num_col[1] = PLAYERS[playerIndex].cards[cardIndex].color;

                // 마스터가 카드를 나누어줍니다.
                Debug.Log("     RPC 송신" + cardIndex + ": " + num_col[0] + ", " + num_col[1]);
                photonView.RPC("Sync_Client_Card", RpcTarget.All, playerIndex, cardIndex, num_col);
                for (int i = 0; i < 1000; i++)
                    ;
            }

            Debug.Log("     " + playerIndex + "플레이어 카드 : " + PLAYERS[playerIndex].cards.Length);
            Debug.Log("     " + playerIndex + "플레이어 카드 동기화 끝");
        }
        Debug.Log("카드 동기화 완료\n");
    }

    //=========================================================================
    // RPC에서 사용하는 함수 - 클라이언트만 실행
    // num_col[]에 number와 color를 받아와 PLAYERS에 추가하는 함수
    //=========================================================================
    [PunRPC]
    void Sync_Client_Card(int playerIndex, int cardIndex, string[] num_col)
    {
        if (playerIndex == this.PlayerNum)
        {
            Debug.Log("     RPC 수신" + playerIndex + " : 카드" + cardIndex + " = " + num_col[0] + ", " + num_col[1]);
            clientCard[cardIndex] = new Card() { number = num_col[0], color = num_col[1] };
        }
    }

    //=========================================================================
    //카드 띄우기
    // 설명
    // 1. 클라이언트가 마스터로부터 동기화 받은 카드를 확인합니다.
    //=========================================================================
    [PunRPC]
    void View_Card()
    {
        Debug.Log("카드 띄우기 실행\n");

        Debug.Log("     플레이어번호 : " + PlayerNum);

        for (int i = 0; i < 22; i++)
        {
            Color color = Color.green;
            switch (clientCard[i].color)
            {
                case "red": color = Color.red; break;
                case "blue": color = Color.blue; break;
                case "yellow": color = Color.yellow; break;
                case "black": color = Color.black; break;
            }
           
            if (i < 11)
            {
                Debug.Log("Top : num/col = " + clientCard[i].number + "/" + clientCard[i].color);
                CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().text = clientCard[i].number;
                CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().color = color;
                Debug.Log("Real Top : num/col = " + CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().text +
                    "/" + CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().color);
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().number = PLAYERS[playerNum].card[i].number;
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().realcolor = color;
            }
            else
            {
                Debug.Log("Bottom : num/col = " + clientCard[i].number + "/" + clientCard[i].color);
                CardHandBot.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().text = clientCard[i].number;
                CardHandBot.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().color = color;
                Debug.Log("Real Bottom : num/col = " + CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().text +
                    "/" + CardHandTop.GetChild(i % 11).GetChild(0).GetChild(0).GetComponent<Text>().color);
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().number = PLAYERS[playerNum].card[i].number;
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().realcolor = color;
            }

        }
        Debug.Log("카드 띄우기 완료\n");
    }
}
