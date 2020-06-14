using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager2 : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("게임메니저 시작\n");

        _playerNum = PhotonNetwork.PlayerList.Length - 1;

        if (!PhotonNetwork.IsMasterClient)
            playButton.enabled = false;
    }
    
    //=========================================================================
    // start 버튼을 누르면 시작하는 함수
    // 설명
    // 1. start버튼을 누르면 실행됩니다.
    // 2. 게임이 시작됩니다.
    // 3. 게임 순서를 랜덤으로 정합니다.
    //=========================================================================
    public void Reset()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.UserId + "\n");
        // Todo: 이름 확인하기
        photonView.RPC("SetPlayerNum", RpcTarget.All);  // 플레이어 번호 지정
        photonView.RPC("InitGameData", RpcTarget.All);  // 카드, 플레이어 초기화
        photonView.RPC("CreateDeck", RpcTarget.All);    // 카드 생성 및 섞기(마스터)
        photonView.RPC("CreatePlayers", RpcTarget.All); // 플레이어 인스턴스 생성
        photonView.RPC("CreateTable", RpcTarget.All);   // Table 인스턴스 생성
        DistributeCards();                                           // 마스터(서버)가 카드를 나누어줌
        SyncCard();                                                  // 마스터(서버)가 배분한 카드를 동기화 시킵니다.                
        photonView.RPC("ViewCards", RpcTarget.All);     // 자신이 받은 카드를 확인합니다.
        
        Random random = new Random();
        turn = (int)(random.NextDouble()* 1000 % PLAYERS.Count);     // 게임의 턴을 랜덤으로 설정합니다.

        // 게임 시작
        gameStart = 1;
        
        /* 오류 방지용 주석
        photonView.RPC("PrintPlayer0CardText", RpcTarget.All);  //Player0 카드 갯수 출력
        photonView.RPC("PrintPlayer1CardText", RpcTarget.All);  //Player1 카드 갯수 출력
        photonView.RPC("PrintPlayer2CardText", RpcTarget.All);  //Player2 카드 갯수 출력
        photonView.RPC("PrintPlayer3CardText", RpcTarget.All);  //Player3 카드 갯수 출력
        */
    }
    
    //=========================================================================
    // 플레이어 번호 지정
    // 설명
    // 1. 플레이어가 네트워크로 부터 받은 번호를 저장합니다.
    //=========================================================================
    [PunRPC]
    void SetPlayerNum()
    {
        Debug.Log("플레이어 번호 지정 시작");
        
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                _playerNum = i;
                Debug.Log("     플레이어번호 : " + _playerNum);
                return;
            }
        }

        Debug.Log("플레이어 번호 지정 끝");
    }
    
    //=========================================================================
    // 덱, 플레이어 초기화
    // 설명
    // 1. 카드와 플레이어 인스턴스를 초기화합니다.
    //=========================================================================
    [PunRPC]
    void InitGameData()
    {
        deck.Clear();            // 카드 초기화
        PLAYERS.Clear();         // 플레이어 초기화
    }
    
    //=========================================================================
    // 카드 생성 - 마스터만 실행
    // 설명
    // 1. 덱을 새롭게 생성합니다.
    // 2. 마스터만 자신의 덱을 섞습니다.
    //=========================================================================
    [PunRPC]
    void CreateDeck()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드를 생성하고 있습니다.");
            return;
        }
            
        Debug.Log("카드 생성 시작\n");
        for (int i = 0; i < 104; i++)
        {
            String cardNumber = ((i % 13) + 1).ToString();
            String cardColor;
            switch((i / 26))
            {
                case 0: cardColor = "black"; break;
                case 1: cardColor = "red"; break;
                case 2: cardColor = "green"; break;
                case 3: cardColor = "blue"; break;
                default: cardColor = "yellow"; break;
            }
            deck.Add(new Card(cardNumber, cardColor));
        }

        deck.Add(new Card("J", "black"));
        deck.Add(new Card("J", "red"));

        // 마스터 클라이언트만 카드를 섞습니다.
        if(PhotonNetwork.IsMasterClient)
            ShuffleCards();

        Debug.Log("카드 생성 완료");
    }
    
    // 카드 섞기 - CreateDeck()에서 사용
    void ShuffleCards()
    {
        Debug.Log("마스터 : 카드 섞기 시작\n");
        int i, j;
        Random random = new Random();

        for (i = 0; i < deck.Count; i++)
        {
            j = ((int)(random.NextDouble() * 1000)) % 106;
            SwapCardsInDecks(i, j);

        }
        Debug.Log("마스터 : 카드 섞기 완료\n");
    }

    // 카드 바꾸기 - ShuffleCards()에서 사용
    void SwapCardsInDecks(int sour, int dest)
    {
        Card temp = deck[sour];
        deck[sour] = deck[dest];
        deck[dest] = temp;
    }
    
    //=========================================================================
    // PLAYERS 인스턴스 생성
    // 설명
    // 1. PLAYER의 인스턴스를 게임에 참여한 수에 맞게 설정합니다.
    //=========================================================================
    [PunRPC]
    void CreatePlayers()
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
    // Table 인스턴스 생성
    // 설명
    // 1. Table 리스트의 인스턴스를 생성합니다.
    //=========================================================================
    [PunRPC]
    void CreateTable()
    {
        Debug.Log("Table 인스턴스 생성 시작");
        for (int row = 0; row < Row; row++)
        {
            List<Card> newCardSlot = new List<Card>();
            for (int col = 0; col < Col; col++)
            {
                Card card = new Card();
                newCardSlot.Add(card);
            }
            Table.Add(newCardSlot);
        }
        Debug.Log("Table 인스턴스 생성 끝");
    }

    //=========================================================================
    //카드 분배하기 - 마스터만 실행
    // 설명
    // 1. 마스터가 섞은 카드를 PLAYERS.Cards 배열에 분배합니다.
    //=========================================================================
    void DistributeCards()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드 분배하고 있습니다.\n");
            return;
        }
        Debug.Log("카드 분배 시작\n");
        try
        {
            for (int j = 0; j < PLAYERS.Count; j++)
            {
                for (int i = 0; i < MaxHandSize; i++)
                {
                    PLAYERS[j].cards[i] = deck[deck.Count - 1];
                    deck.RemoveAt(deck.Count - 1);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
        Debug.Log("카드 분배 완료\n");
    }

    //=========================================================================
    //카드의 싱크를 맞추는 함수 - 마스터만 실행
    // 설명
    // 1. 마스터가 PLAYERS.Cards 배열에 분배한 데이터를 모든 클라이언트에게 동기화 시킵니다.
    //=========================================================================
    void SyncCard()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드를 동기화하고 있습니다.\n");
            return;
        }

        Debug.Log("카드 동기화 시작\n");

        Debug.Log("     입장한 플레이어 수: " + PhotonNetwork.PlayerList.Length + "\n");
        Debug.Log("     플레이어 인스턴스 수 : " + PLAYERS.Count);

        string[] numCol = { "", "" };

        // 전체 카드 동기화
        for(int cardIndex = 0; cardIndex < deck.Count; cardIndex++)
        {
            numCol[0] = deck[cardIndex].CardNumber;
            numCol[1] = deck[cardIndex].CardColor;
            // Todo: mainCardsetting 이름
            photonView.RPC("mainCardSetting", RpcTarget.Others, numCol);
        }


        // 플레이어 카드 동기화
        for (int playerIndex = 0; playerIndex < PhotonNetwork.PlayerList.Length; playerIndex++)
        {
            Debug.Log("     " + playerIndex + "플레이어 카드 동기화 시작");

            for(int cardIndex = 0; cardIndex < PLAYERS[playerIndex].cards.Length; cardIndex++)
            {
                numCol[0] = PLAYERS[playerIndex].cards[cardIndex].CardNumber;
                numCol[1] = PLAYERS[playerIndex].cards[cardIndex].CardColor;

                Debug.Log("     RPC 송신" + cardIndex + ": " + numCol[0] + ", " + numCol[1]);
                photonView.RPC("SyncCardElements", RpcTarget.Others, playerIndex, cardIndex, numCol);
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
    // numCol[]에 number와 color를 받아와 PLAYERS에 추가하는 함수
    //=========================================================================
    [PunRPC]
    void SyncCards(string[] numCol)
    {
        deck.Add(new Card(numCol[0], numCol[1]));
    }

    [PunRPC]
    void SyncCardElements(int playerIndex, int cardIndex, string[] numCol)
    {
        Debug.Log("     RPC 수신" + playerIndex+" : 카드" + cardIndex+ " = " + numCol[0] + ", " + numCol[1]);
        PLAYERS[playerIndex].cards[cardIndex] = new Card(numCol[0], numCol[1]);
    }

    //=========================================================================
    //카드 띄우기
    // 설명
    // 1. 클라이언트가 마스터로부터 동기화 받은 카드를 확인합니다.
    //=========================================================================
    [PunRPC]
    void ViewCards()
    {
        Debug.Log("카드 띄우기 실행\n");
        Debug.Log("     플레이어번호 : " + _playerNum);
        Debug.Log("     호스트 플레이어 카드 갯수" + PLAYERS[0].cards.Length);
        Debug.Log("     플레이어 카드 갯수" + PLAYERS[_playerNum].cards.Length);
        
        for (int i = 0; i < PLAYERS.Count; i++)
        {
            Debug.Log("   @player" + i + "의 카드 ==========");
            for (int j = 0; j < PLAYERS[i].cards.Length; j++)
            {
                Debug.Log("     card[" + j + "]:" + PLAYERS[i].cards[j].CardColor + PLAYERS[i].cards[j].CardNumber);
            }
        }
        
        for (int i = 0; i < MaxHandSize; i++)
        {
            Debug.Log(PLAYERS[_playerNum].cards.Length);
            if (i < MaxHandSize / 2)
            {
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = PLAYERS[_playerNum].cards[i].CardNumber;
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = PLAYERS[_playerNum].cards[i].RealColor;
            }
            else
            {
                cardHandBot.GetChild(i % MaxHandSize / 2).GetChild(0).GetComponent<Text>().text = PLAYERS[_playerNum].cards[i].CardNumber;
                cardHandBot.GetChild(i % MaxHandSize / 2).GetChild(0).GetComponent<Text>().color = PLAYERS[_playerNum].cards[i].RealColor;
            }
        }
        gameStart = 1;

        Debug.Log("카드 띄우기 완료\n");
    }
}
