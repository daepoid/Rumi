using System.Collections.Generic;
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

        playerNum = PhotonNetwork.PlayerList.Length - 1;

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

        photonView.RPC("setPlayerNum", RpcTarget.All);  // 플레이어 번호 지정
        photonView.RPC("initialize", RpcTarget.All);    // 카드, 플레이어 초기화
        photonView.RPC("opencard", RpcTarget.All);      // 카드 생성 및 섞기(마스터)
        photonView.RPC("createPlayer", RpcTarget.All);  // 플레이어 인스턴스 생성
        photonView.RPC("createTable", RpcTarget.All);   // TABLE 인스턴스 생성
        cardCall();                                     // 마스터(서버)가 카드를 나누어줌
        syncCard();                                     // 마스터(서버)가 배분한 카드를 동기화 시킵니다.                
        photonView.RPC("viewCard", RpcTarget.All);      // 자신이 받은 카드를 확인합니다.

        TABLE_backup = TABLE;

        Random random = new Random();
        turn = (int)(random.NextDouble()* 1000 % PLAYERS.Count); // 게임의 턴을 랜덤으로 설정합니다.

        // 게임 시작
        gameStart = 1;
        /* 에러 방지용 주석
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
    void setPlayerNum()
    {
        Debug.Log("플레이어 번호 지정 시작");

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (PhotonNetwork.PlayerList[i].IsLocal)
            {
                playerNum = i;
                Debug.Log("     플레이어번호 : " + playerNum);
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
    void initialize()
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
    [PunRPC]
    void opencard()
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
    [PunRPC]
    void createPlayer()
    {
        Debug.Log("플레이어 인스턴스 생성 시작");
        Player p;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            p = new Player();
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
    void createTable()
    {
        Debug.Log("TABLE 인스턴스 생성 시작");
        for (int raw = 0; raw < 12; raw++)
        {
            for (int col = 0; col < 32; col++)
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
    void cardCall()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드 분배하고 있습니다.\n");
            return;
        }

        Debug.Log("카드 분배 시작\n");

        /*Debug.Log("     전체 카드 확인 \n");
        //###########################카드 출력
        for (int i = 0;  i < DECK.Count; i++)
        {
            Debug.Log("     card[" + i + "] :" + DECK[i].color + DECK[i].number);
        }*/

        //카드 분배, 멀티 플레이용
        // ## PLAYERS.count 사용 시 다른 사람이 접속이 끊겨도 계속 실행됩니다.
        try
        {
            for (int j = 0; j < PLAYERS.Count; j++)
            {
                for (int i = 0; i < 22; i++)
                {
                    //Debug.Log("     마지막 카드" + i + DECK[DECK.Count - 1].color + DECK[DECK.Count - 1].number);
                    PLAYERS[j].cards[i] = DECK[DECK.Count - 1];
                    //Debug.Log("     입력한 카드" + i + PLAYERS[j].cards[PLAYERS[j].cards.Length - 1].color
                    //     + PLAYERS[j].cards[PLAYERS[j].cards.Length - 1].number);

                    DECK.RemoveAt(DECK.Count - 1);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("     Error : 오류가 발생했습니다. \n");
            return;
        }

        //###########################카드 출력
        /*  
        //Debug.Log("   @남은 카드 확인");
        for (int i = 0; i < DECK.Count; i++)
        {
            Debug.Log("     card[" + i + "] :" + DECK[i].color + DECK[i].number);
        }
        

        for(int i =0; i< PLAYERS.Count; i++)
        {
            Debug.Log("   @player"+i+"의 카드 ==========");
            for (int j = 0; j< PLAYERS[i].cards.Length; j++)
            {
                Debug.Log("     card["+j+"]:"+PLAYERS[i].cards[j].color+ PLAYERS[i].cards[j].number);
            }
        }
        */

        Debug.Log("카드 분배 완료\n");
    }

    //=========================================================================
    //카드의 싱크를 맞추는 함수 - 마스터만 실행
    // 설명
    // 1. 마스터가 PLAYERS.Cards 배열에 분배한 데이터를 모든 클라이언트에게 동기화 시킵니다.
    //=========================================================================
    void syncCard()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("마스터가 카드를 동기화하고 있습니다.\n");
            return;
        }

        Debug.Log("카드 동기화 시작\n");

        Debug.Log("     입장한 플레이어 수: " + PhotonNetwork.PlayerList.Length + "\n");
        Debug.Log("     플레이어 인스턴스 수 : " + PLAYERS.Count);

        string[] num_col = { "", "" };

        // 전체 카드 동기화
        for (int cardIndex = 0; cardIndex < DECK.Count; cardIndex++)
        {
            num_col[0] = DECK[cardIndex].number;
            num_col[1] = DECK[cardIndex].color;

            photonView.RPC("mainCardSetting", RpcTarget.Others, num_col);
        }


        // 플레이어 카드 동기화
        for (int playerIndex = 0; playerIndex < PhotonNetwork.PlayerList.Length; playerIndex++)
        {
            Debug.Log("     " + playerIndex + "플레이어 카드 동기화 시작");

            for (int cardIndex = 0; cardIndex < PLAYERS[playerIndex].cards.Length; cardIndex++)
            {
                num_col[0] = PLAYERS[playerIndex].cards[cardIndex].number;
                num_col[1] = PLAYERS[playerIndex].cards[cardIndex].color;

                Debug.Log("     RPC 송신" + cardIndex + ": " + num_col[0] + ", " + num_col[1]);
                photonView.RPC("playerCardSetting", RpcTarget.Others, playerIndex, cardIndex, num_col);
                for (int i = 0; i < 1000; i++)
                    ;
            }

            Debug.Log("     " + playerIndex + "플레이어 카드 : " + PLAYERS[playerIndex].cards.Length);
            Debug.Log("     " + playerIndex + "플레이어 카드 동기화 끝");
        }
        /*
        Debug.Log("     분배 후 전체카드 수 : " + DECK.Count);
        Debug.Log("     플레이어 번호:" + playerNum);
        Debug.Log("     카드수:" + PLAYERS[playerNum].cards.Length);
        Debug.Log("     플레이어 수:" + PhotonNetwork.PlayerList.Length);
        */
        Debug.Log("카드 동기화 완료\n");
    }

    //=========================================================================
    // RPC에서 사용하는 함수 - 클라이언트만 실행
    // num_col[]에 number와 color를 받아와 PLAYERS에 추가하는 함수
    //=========================================================================
    [PunRPC]
    void mainCardSetting(string[] num_col)
    {
        DECK.Add(new Card() { number = num_col[0], color = num_col[1] });
    }

    [PunRPC]
    void playerCardSetting(int playerIndex, int cardIndex, string[] num_col)
    {
        Debug.Log("     RPC 수신" + playerIndex + " : 카드" + cardIndex + " = " + num_col[0] + ", " + num_col[1]);
        PLAYERS[playerIndex].cards[cardIndex] = new Card() { number = num_col[0], color = num_col[1] };
    }

    //=========================================================================
    //카드 띄우기
    // 설명
    // 1. 클라이언트가 마스터로부터 동기화 받은 카드를 확인합니다.
    //=========================================================================
    [PunRPC]
    void viewCard()
    {
        Debug.Log("카드 띄우기 실행\n");

        Debug.Log("     플레이어번호 : " + playerNum);
        Debug.Log("     호스트 플레이어 카드 갯수" + PLAYERS[0].cards.Length);
        Debug.Log("     플레이어 카드 갯수" + PLAYERS[playerNum].cards.Length);

        for (int i = 0; i < PLAYERS.Count; i++)
        {
            Debug.Log("   @player" + i + "의 카드 ==========");
            for (int j = 0; j < PLAYERS[i].cards.Length; j++)
            {
                Debug.Log("     card[" + j + "]:" + PLAYERS[i].cards[j].color + PLAYERS[i].cards[j].number);
            }
        }

        for (int i = 0; i < 22; i++)
        {
            Color color = Color.green;
            switch (PLAYERS[playerNum].cards[i].color)
            {
                case "red": color = Color.red; break;
                case "blue": color = Color.blue; break;
                case "yellow": color = Color.yellow; break;
                case "black": color = Color.black; break;
            }

            if (i < 11)
            {
                Debug.Log("Top : num/col = " + PLAYERS[playerNum].cards[i].number + "/" + PLAYERS[playerNum].cards[i].color);
                CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().text = PLAYERS[playerNum].cards[i].number;
                CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().color = color;
                Debug.Log("Real Top : num/col = " + CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().text +
                    "/" + CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().color);
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().number = PLAYERS[playerNum].card[i].number;
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().realcolor = color;
            }
            else
            {
                Debug.Log("Bottom : num/col = " + PLAYERS[playerNum].cards[i].number + "/" + PLAYERS[playerNum].cards[i].color);
                CardHandBot.GetChild(i % 11).GetChild(0).GetComponent<Text>().text = PLAYERS[playerNum].cards[i].number;
                CardHandBot.GetChild(i % 11).GetChild(0).GetComponent<Text>().color = color;
                Debug.Log("Real Bottom : num/col = " + CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().text +
                    "/" + CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Text>().color);
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().number = PLAYERS[playerNum].card[i].number;
                // CardHandTop.GetChild(i % 11).GetChild(0).GetComponent<Card>().realcolor = color;
            }

            // if (i < 11)
            // {
            //     CardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = PLAYERS[playerNum].card[i].number;
            //     switch (PLAYERS[playerNum].card[i].color)
            //     {
            //         case "red": CardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.red; break;
            //         case "blue": CardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.blue; break;
            //         case "yellow": CardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.yellow; break;
            //         case "black": CardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.black; break;
            //     }
            // }
            // else
            // {
            //     CardHandBot.GetChild(i - 11).GetChild(0).GetComponent<Text>().text = PLAYERS[playerNum].card[i].number;
            //     switch (PLAYERS[playerNum].card[i].color)
            //     {
            //         case "red": CardHandBot.GetChild(i-11).GetChild(0).GetComponent<Text>().color = Color.red; break;
            //         case "blue": CardHandBot.GetChild(i-11).GetChild(0).GetComponent<Text>().color = Color.blue; break;
            //         case "yellow": CardHandBot.GetChild(i-11).GetChild(0).GetComponent<Text>().color = Color.yellow; break;
            //         case "black": CardHandBot.GetChild(i-11).GetChild(0).GetComponent<Text>().color = Color.black; break;
            //     }
            // }
        }
        gameStart = 1;

        Debug.Log("카드 띄우기 완료\n");
    }
}
