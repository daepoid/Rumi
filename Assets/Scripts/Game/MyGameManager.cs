using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager : MonoBehaviourPunCallbacks
{
    public List<Card> DECK = new List<Card>();                  // 전체 카드 덱 : 분배하 남은 카드가 있습니다.
    public static List<Player> PLAYERS = new List<Player>();    // 플레이어 : 각 플레이어들의 카드를 저장하고 있습니다.
    public Card[,] TABLE = new Card[12, 32];                    // Rumikub의 게임판을 저장한 배열입니다.
    public Card[,] TABLE_backup;                                // 게임판을 백업합니다. 백업은 마스터만 관리합니다.
    private int playerNum;                                      // 자신의 플레이어 번호를 알려줍니다. 0~4
    public int gameStart = 0;                                   // 게임전:0, 게임중:1
    public int turn = -1;                                       // 턴을 나타내는 변수
    private float time = 0;

    public Button playButton;                                   // start button : 마스터만 실행
    public Button requet;                                       // 등록한 카드에 대한 요청 : 이 후 마스터가 판단
    public Button nextButton;                                   // turn을 넘겨주는 버튼
    public Transform CardHandTop;                               // 로컬 플레이어 카드패 
    public Transform CardHandBot;                               // 로컬 플레이어 카드패
    public Transform TableTop;                                  // 게임판
    public Text Timmer;                                          // 타이머

    // Update is called once per frame
    void Update()
    {
        if (gameStart == 1)
        {
            if (playerNum == turn)
            {
                // 게임판이 사용을 허가하는 코드를 추가해야 합니다.
            }

            if (PhotonNetwork.IsMasterClient)
            {
                time += Time.deltaTime;

                if (time > 60)
                {
                    time = 0;
                    Next();
                }

                photonView.RPC("Sync_time", RpcTarget.All, time);
            }


            // 자신의 카드의 개수가 0개면 게임을 종료합니다.
            /* if()
              {
              }
             */
        }
    }

    //=========================================================================
    // request 버튼 - BETA
    // 설명
    // 1. request 버튼을 누르게 되면 클라이언트의 TABLE을 읽어와 Rule을 검사합니다.
    // 2. Rule을 검사하고 Rule에 문제가 없으면 모든 클라이언트의 TABLE을 동기화합니다.
    // 3. Rule에 문제가 있으면 TABLE을 TABLE_backup 내용으로 바꿉니다.
    // 4. 자신의 카드 개수를 업데이트 합니다.
    //=========================================================================
    public void Request()
    {
        Get_TABLE();

        bool flag = false;
        //Rule 구현
        // ....
        // ....

        // 만약 자신의 테이블이 참이라면
        if (flag)
            Sync_TABLE();
        else
            TABLE = TABLE_backup;
    }

    //=========================================================================
    // next 버튼 - BETA
    // 설명
    // 1. next 버튼을 누르게 되면 int turn이 증가하여 다음 플레이어에게 턴을 전달합니다.
    //=========================================================================
    public void Next()
    {
        photonView.RPC("Backup", RpcTarget.All);
        turn++;
    }

    //=========================================================================
    // 테이블을 백업합니다.
    // 위 함수에서 호출합니다.
    //=========================================================================
    [PunRPC]
    void Backup()
    {
        TABLE_backup = TABLE;
    }

    //=========================================================================
    // TABLE 동기화
    // 설명
    // 1. 자신의 TABLE을 모든 클라이언트에게 동기화 시킵니다.
    // 2. raw와 col로 TABLE 배열의 인덱스로 접근하며, num_col에 해당 TABLE 요소의 숫자와 색상을 저장합니다.
    //=========================================================================
    void Sync_TABLE()
    {
        string[] num_col = { "", "" };
        for (int raw = 0; raw < 12; raw++)
        {
            for (int col = 0; col < 32; col++)
            {
                num_col[0] = TABLE[raw, col].number;
                num_col[1] = TABLE[raw, col].color;
                photonView.RPC("Sync_TABLE_Element", RpcTarget.Others, raw, col, num_col);
            }
        }

    }
    //=========================================================================
    // TABLE 요소 동기화 - 위 함수에 의해 호출됩니다.
    // 설명
    // 1. TABLE들의 요소를 동기화 합니다.
    // 2. raw, col 으로 TABLE의 인덱스를 나타내며, num_col 배열에 숫자와 색상이 담겨 있습니다.
    //=========================================================================
    [PunRPC]
    void Sync_TABLE_Element(int raw, int col, string[] num_col)
    {
        TABLE[raw, col].number = num_col[0];
        TABLE[raw, col].color = num_col[1];
    }

    //=========================================================================
    // TABLE 가져오기
    // 설명
    // 1. 게임판에 있는 데이터를 TABLE 배열로 가져옵니다.
    //=========================================================================
    void Get_TABLE()
    {
        for (int raw = 0; raw < 12; raw++)
        {
            for (int col = 0; col < 32; col++)
            {
                try
                {
                    TABLE[raw, col].number = TableTop.GetChild(raw).GetChild(col).GetChild(0).GetComponent<Text>().text;
                    TABLE[raw, col].color = TableTop.GetChild(raw).GetChild(col).GetChild(0).GetComponent<Text>().color.ToString();
                }
                catch (Exception e)
                { 
                    Debug.Log("getTable 에러발생");
                    return;
                }
            }
        }
    }

    //=========================================================================
    // TABLE 띄우기
    // 설명
    // 1. TABLE 배열에 있는 데이터를 게임판으로 보냅니다.
    //=========================================================================
    void View_TABLE()
    {
        for(int raw=0;raw<12;raw++)
        {
            for(int col=0;col<32;col++)
            {
                Color color = Color.green;
                switch (TABLE[raw,col].color)
                {
                    case "red": color = Color.red; break;
                    case "blue": color = Color.blue; break;
                    case "yellow": color = Color.yellow; break;
                    case "black": color = Color.black; break;
                }
                //test
                TABLE[raw, col].number = col.ToString();
                //test

                TableTop.GetChild(raw).GetChild(col).GetChild(0).GetComponent<Text>().text = TABLE[raw,col].number;
                TableTop.GetChild(raw).GetChild(col).GetChild(0).GetComponent<Text>().color = color;
            }
        }
    }

    //=========================================================================
    // 타이머 동기화
    // 설명
    // 1. 마스터의 타이머를 동기화 하는 프로그램 입니다.
    //=========================================================================
    [PunRPC]
    void Sync_time(float time)
    {
        this.Timmer.text = $"{time:N2}";
    }
    //=========================================================================
    //카드 갯수 띄우기
    //=========================================================================
    [PunRPC]
    void PrintPlayer0CardText()
    {
        //오브젝트와 컴포넌트 가져오기
        GameObject TextObject = GameObject.Find("Player0CardNumber");
        Text nameText = TextObject.GetComponent<Text>();

        //현재 플레이어들 정보 가져오기
        Photon.Realtime.Player[] playersInRoom = PhotonNetwork.PlayerList;
        //로컬 플레이어 인덱스 찾기
        int localPlayerIndex = Array.FindIndex(playersInRoom, player => player.IsLocal == true);


        //로컬 플레이어 찾기
        Player localPlayer = PLAYERS.Find(x => x.NickName == playersInRoom[localPlayerIndex].NickName);
        //텍스트 출력
        nameText.text = localPlayer.cards.Length.ToString();

    }

    [PunRPC]
    void PrintPlayer1CardText()
    {
        //오브젝트와 컴포넌트 가져오기
        GameObject TextObject = GameObject.Find("Player1CardNumber");
        Text nameText = TextObject.GetComponent<Text>();

        //Player1 정보 가져오기
        Photon.Realtime.Player playerInRoom = PhotonNetwork.PlayerListOthers[0];

        //players 리스트에서 Player1 찾기
        Player Player1 = PLAYERS.Find(x => x.NickName == playerInRoom.NickName);
        //텍스트 출력
        nameText.text = Player1.cards.Length.ToString();

    }

    [PunRPC]
    void PrintPlayer2CardText()
    {
        //오브젝트와 컴포넌트 가져오기
        GameObject TextObject = GameObject.Find("Player2CardNumber");
        Text nameText = TextObject.GetComponent<Text>();

        //Player2 정보 가져오기
        Photon.Realtime.Player playerInRoom = PhotonNetwork.PlayerListOthers[1];


        //players 리스트에서 Player2 찾기
        Player cardNumber = PLAYERS.Find(x => x.NickName == playerInRoom.NickName);
        //텍스트 출력
        nameText.text = cardNumber.cards.Length.ToString();

    }

    [PunRPC]
    void PrintPlayer3CardText()
    {
        //오브젝트와 컴포넌트 가져오기
        GameObject TextObject = GameObject.Find("Player3CardNumber");
        Text nameText = TextObject.GetComponent<Text>();

        //Player3 정보 가져오기
        Photon.Realtime.Player playerInRoom = PhotonNetwork.PlayerListOthers[2];


        //players 리스트에서 Player3 찾기
        Player cardNumber = PLAYERS.Find(x => x.NickName == playerInRoom.NickName);
        //텍스트 출력
        nameText.text = cardNumber.cards.Length.ToString();
    }
}
