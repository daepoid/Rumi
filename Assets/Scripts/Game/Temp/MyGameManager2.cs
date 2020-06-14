using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager2 : MonoBehaviourPunCallbacks
{
    public static readonly int Col = 16;
    public static readonly int Row = 6;
    public static readonly int MaxHandSize = 22;
    private static readonly int MaxTime = 5;

    public List<Card> deck = new List<Card>(); // 전체 카드 덱 : 분배하고 남은 카드가 있습니다.
    
    // Readonly로 ?
    public static List<Player> PLAYERS = new List<Player>(); // 플레이어 : 각 플레이어들의 카드를 저장하고 있습니다.
    public static List<List<Card>> Table = new List<List<Card>>();
    public static List<List<Card>> TableBackup = new List<List<Card>>();
    public static List<Card> CardHandBackup = new List<Card>();
    public static bool ControlFlag = false;

    public int gameStart = 0; // 게임전:0, 게임중:1
    public int turn = 0; // 턴을 나타내는 변수
    private int _playerNum; // 자신의 플레이어 번호를 알려줍니다. 0~4
    private float _time = 0;
    
    public Button playButton;
    public Button resetButton;
    public Button request;
    public Button nextButton;
    public Transform cardHandTop;
    public Transform cardHandBot;
    public Transform tableTop;
    public Text timer;
    public Text turnText;

    void Update()
    {
        if (gameStart == 1)
        {
            if (_playerNum == turn && !ControlFlag)
            {
                // 게임판의 사용을 허가하는 코드를 추가해야 합니다.
                ControlFlag = true;
                turnText.enabled = false;
                SaveCardHand();
            }

            _time += Time.deltaTime;
            if (PhotonNetwork.IsMasterClient)
            {
                if (_time > MaxTime)
                {
                    _time = 0;
                    GetTable();
                    LoadCardHand();
                    Next();
                    ControlFlag = false;
                    turnText.enabled = true;
                }
                // TODO: Sync_time 이름 정리
                photonView.RPC("Sync_time", RpcTarget.All, _time);
            }
            
            // 자신의 카드의 개수가 0개면 게임을 종료합니다.
            /* if()
              {
              }
             */
        }
    }

    public bool ValidateTableTop()
    {
        return false;
    }

    public void SaveCardHand()
    {
        for (int i = 0; i < cardHandTop.childCount; i++)
        {
            Card newCard = new Card(cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color.ToString());
            CardHandBackup.Add(newCard);
        }

        for (int i = 0; i < cardHandBot.childCount; i++)
        {
            Card newCard = new Card(cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color.ToString());
            CardHandBackup.Add(newCard);
        }
    }

    public void LoadCardHand()
    {
        for (int i = 0; i < MaxHandSize / 2; i++)
        {
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = CardHandBackup[i].CardNumber;
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = CardHandBackup[i].RealColor;
            if (CardHandBackup[i].CardNumber != "")
            {
                cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
            }
            
            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = CardHandBackup[i + (MaxHandSize / 2)].CardNumber;
            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color = CardHandBackup[i + (MaxHandSize / 2)].RealColor;
            if (CardHandBackup[i + (MaxHandSize / 2)].CardNumber != "")
            {
                cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
            }
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
        SetTable();

        bool flag = false;
        //Rule 구현
        // ....
        // ....

        // 만약 자신의 테이블이 참이라면
        if (flag)
            SyncTable();
        else
            Table = TableBackup;
    }

    //=========================================================================
    // next 버튼 - BETA
    // 설명
    // 1. next 버튼을 누르게 되면 int turn이 증가하여 다음 플레이어에게 턴을 전달합니다.
    //=========================================================================
    public void Next()            // Next에 _controlFlag를 true로 만들어주는 코드 작성
    {
        Debug.Log("Next 버튼 클릭");
        if (!ValidateTableTop())
        {
            
        }
        photonView.RPC("Backup", RpcTarget.All);
        turn++;
        if (turn > PLAYERS.Count)
        {
            turn = turn % PLAYERS.Count;
        }
        // turn = (turn + 1) % PLAYERS.Count;
        Debug.Log(turn);
    }
    
    //=========================================================================
    // 테이블을 백업합니다.
    // 위 함수에서 호출합니다.
    //=========================================================================
    [PunRPC]
    void Backup()
    {
        TableBackup = Table;
    }
    
    //=========================================================================
    // Table 동기화
    // 설명
    // 1. 자신의 TABLE을 모든 클라이언트에게 동기화 시킵니다.
    // 2. raw와 col로 Table 배열의 인덱스로 접근하며, num_col에 해당 Table 요소의 숫자와 색상을 저장합니다.
    //=========================================================================
    void SyncTable()
    {
        Debug.Log("백업 시작");
        SetTable();
        TableBackup = Table;
        for (int row = 0; row < Row; row++)
        {
            for (int col = 0; col < Col; col++)
                Debug.Log("(" + row + "," + col + ") = " + TableBackup[row][col].CardColor + "/" + TableBackup[row][col].CardNumber);
        }
    }
    
    //=========================================================================
    // Table 요소 동기화 - 위 함수에 의해 호출됩니다.
    // 설명
    // 1. TABLE들의 요소를 동기화 합니다.
    // 2. raw, col 으로 TABLE의 인덱스를 나타내며, numCol 배열에 숫자와 색상이 담겨 있습니다.
    //=========================================================================
    [PunRPC]
    void SyncTableElements(int row, int col, string[] numCol)
    {
        Table[row][col].CardNumber = numCol[0];
        Table[row][col].CardColor = numCol[1];
        Table[row][col].RealColor = Card.ConvertToRealColor(numCol[1]);
    }

    //=========================================================================
    // Table 가져오기
    // 설명
    // 1. 게임판에 있는 데이터를 Table 배열로 가져옵니다.
    //=========================================================================
    // Todo: 확인
    void SetTable()
    {
        for (int row = 0; row < Row; row++)
        {
            for (int col = 0; col < Col; col++)
            {
                Table[row][col].CardNumber = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text;
                Table[row][col].CardColor = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color.ToString();
                Table[row][col].RealColor = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color;
            }
        }
    }
    
    //=========================================================================
    // Table 띄우기
    //=========================================================================
    void GetTable()
    {
        for(int row = 0; row < Row; row++)
        {
            for(int col = 0; col < Col; col++)
            {
                tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text = Table[row][col].CardNumber;
                tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color = Table[row][col].RealColor;
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
        this.timer.text = $"{_time:N1}";
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
