using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager : MonoBehaviourPunCallbacks
{
    public static readonly int TableRow = 6;                  // 게임판의 RAW 크기
    public static readonly int TableCol = 16;                 // 게임판의 COLUM 크기
    public static readonly int MaxHandSize = 22;
    public static readonly int MaxTime = 10;

    public static bool ControlFlag = false;
    public List<Card> deck = new List<Card>();                  // 전체 카드 덱 : 분배하 남은 카드가 있습니다.
    public static List<Player> Players = new List<Player>();    // 플레이어 : 각 플레이어들의 카드를 저장하고 있습니다.
    public static Card[,] Table = new Card[TableRow, TableCol];      // Rumikub의 게임판을 저장한 배열입니다.
    public static Card[,] TableBackup = new Card[TableRow, TableCol]; // 게임판을 백업합니다. 백업은 마스터만 관리합니다.
    public static List<Card> ClientCard = new List<Card>();         // 클라이언트의 카드를 나타냅니다.
    public static List<Card> ClientCardBackup = new List<Card>();    //클라이언트의 카드를 백업합니다.
    public static bool SortButtonFlag = false;
    // public static bool accessTableTopChek = false;                   // 테이블탑에 있는 값들과 교환을 할 수 있는지 설정한다.
    public static bool DragableCheck = false;                        // 드래그 할 수 있는 상태인지 확인.
                                                                     // 다음 턴으로 넘어가면서 드래그를 강제로 종료하고 다음사람 턴이 시작되면서 드래그가 다시 가능해진다.
    
    public int ClientCardNum = 0;
    public int[] ClientCardNum_Board = new int[4] { 0, 0, 0, 0 };

    private int _playerCount = 0;                                       // 게임중인 플레이어 수를 알려줍니다.
    private int _playerNum = -1;                                      // 자신의 플레이어 번호를 알려줍니다. 0~4
    private int _runningGame = 0;                                   // 게임전:0, 게임중:1
    private int _turn = -1;                                       // 턴을 나타내는 변수
    private float _time = 0;                                    // 60초 타이머

    public Button buttonStart;                                 // start button : 마스터만 실행
    public Button buttonReset;                                  // 등록한 카드에 대한 요청 : 이 후 마스터가 판단
    public Button buttonNext;                                  // turn을 넘겨주는 버튼
    public Transform cardHandTop;                               // 로컬 플레이어 카드패 
    public Transform cardHandBot;                               // 로컬 플레이어 카드패
    public Transform tableTop;                                  // 게임판
    public Text textTimer;                                    // 60초 타이머
    public Text showWhosTurn;
    public Transform PLAYERS;                                 // Game Scene의 Players와 연결되어 있습니다.

    private bool _turnStartFlag = true;                        // 자신의 턴이 시작될때 한 번만 수행

    // Update is called once per frame
    void Update()
    {
        if (_runningGame == 1)
        {
            if (SortButtonFlag)
            {
                Get_ClientCard();
                SortButtonFlag = false;
            }
            if (_playerNum == _turn)
            {
                // 게임판의 사용을 허가하는 코드를 추가해야 합니다.
                if (_turnStartFlag)
                {
                    DragableCheck = true;
                    Backup();
                    showWhosTurn.enabled = false;
                    ControlFlag = true;
                    _turnStartFlag = false;
                    Get_ClientCard();    // 자신의 카드의 개수를 셉니다.
                    Count_ClientCard();
                    SwitchTableAccess(ControlFlag);
                }
                buttonNext.enabled = true;
                buttonNext.GetComponent<Image>().color = Color.white;
                buttonReset.enabled = true;
                buttonReset.GetComponent<Image>().color = Color.white;
            }
            else
            {
                buttonNext.enabled = false;
                buttonNext.GetComponent<Image>().color = Color.gray;
            }

            // 제한시간(MaxTime)이 지나면 모든 '사용자'들의 Turn을 1 증가시킵니다.
            // Next를 누르지 못하면 자동으로 규칙을 검사합니다.
            if (PhotonNetwork.IsMasterClient)
            {
                _time += Time.deltaTime;

                if (_time > MaxTime)
                {
                    _time = 0;
                    _turnStartFlag = true;
                    ControlFlag = false;
                    showWhosTurn.enabled = false;
                    SwitchTableAccess(ControlFlag);
                    photonView.RPC("Next", RpcTarget.All);
                }
                photonView.RPC("SyncTime", RpcTarget.All, _time);

                photonView.RPC("Sync_ClientCardNum", RpcTarget.All,ClientCardNum_Board);
            }
            // 자신의 카드의 개수가 0개면 게임을 종료합니다.
            /* if()
              {
              }
             */
        }
    }

    private void SwitchTableAccess(bool flag)
    {
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                tableTop.GetChild(row).GetChild(col).GetComponent<Draggable>().enabled = flag;
            }
        }

        for (int i = 0; i < MaxHandSize / 2; i++)
        {
            cardHandTop.GetChild(i).GetComponent<Draggable>().enabled = flag;
            cardHandBot.GetChild(i).GetComponent<Draggable>().enabled = flag;
        }
    }

    //=========================================================================
    // Reset 버튼 - BETA
    // 설명
    // 1. Reset 버튼을 누르면 자신이 작업하기 전 게임판으로 만듭니다.
    //=========================================================================
    public void Reset()
    {
        // 테이블 복원
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                Table[row, col] = new Card(TableBackup[row, col].CardNumber, TableBackup[row, col].CardColor);
            }
        }
        
        //클라이언트 카드 복원
        ClientCard.Clear();
        for (int i = 0; i < ClientCardBackup.Count; i++)
        {
            ClientCard.Add(new Card(ClientCardBackup[i].CardNumber, ClientCardBackup[i].CardColor));
        }
        View_TABLE();
        View_ClientCard();
    }

    //=========================================================================
    // Next 버튼 - BETA
    // 설명
    // 1. 게임판을 읽어와 Rule을 확인합니다.
    //   1) Rule을 만족시키지 못하면 수정 하기 전 게임판으로 다시 만듭니다.
    //   2) Rule을 만족하면 현재의 테이블을 다른 테이블에게 동기화합니다.
    // 2. 모든 '사용자'가 게임판을 백업합니다.
    // 3. 모든 '사용자'의 Turn을 증가시킵니다.
    //=========================================================================
    [PunRPC]
    public void Next()
    {
        if (_turn != _playerNum)
            return;

        Backup();
        Get_ClientCard();
        Get_TABLE();

        // 클라이언트 카드
        string str = "";
        foreach (Card c in ClientCard)
            str = str + c.CardColor + c.CardNumber + " ";
        Debug.Log("ClientCard : " + str);

        // 클라이언트 카드 백업
        str = "";
        foreach (Card c in ClientCardBackup)
            str = str + c.CardColor + c.CardNumber + " ";
        Debug.Log("ClientCardBackup : " + str);

        //테이블 출력
        for (int i = 0; i < TableRow; i++)
        {
            str = "";
            for (int j = 0; j < TableCol; j++)
            {
                str = str + " " + Table[i, j].CardColor + Table[i, j].CardNumber;
            }
            Debug.Log("table" + i + " : " + str);
        }

        // 테이블 백업
        for (int i = 0; i < TableRow; i++)
        {
            str = "";
            for (int j = 0; j < TableCol; j++)
            {
                str = str + " " + TableBackup[i, j].CardColor + TableBackup[i, j].CardNumber;
            }
            Debug.Log("TableBackup" + i + " : " + str);
        }

        if (ClientCardNum == Count_ClientCard() && ClientCardNum < MaxHandSize)
        {
            Debug.Log("Request() : 카드를 한 장 요청합니다.");
            photonView.RPC("Serve_Card", RpcTarget.MasterClient, _playerNum); 
        }
        else
        {
            Debug.Log("Rule() : 시작");

            // TABLE Rule 검사 시작
            Get_TABLE();
            if (!Rule())
            {
                // Rule을 만족시키지 못함
                Debug.Log("     Rule : Fail");
                Reset();
            }
            else
            {
                // Rule을 만족시킴
                Debug.Log("     Rule : True");
                Get_ClientCard();
                Sync_TABLE();
            }
        }

        photonView.RPC("Backup", RpcTarget.All);
        _time = 0;
        _turn = (_turn + 1) % _playerCount;
        photonView.RPC("SyncTime", RpcTarget.All, _time);
        photonView.RPC("Sync_Turn", RpcTarget.All, _turn);
        photonView.RPC("View_TABLE", RpcTarget.All);
        photonView.RPC("Report_ClientCardNum", RpcTarget.MasterClient,_playerNum,ClientCardNum);
        photonView.RPC("Print_ClientCardNum", RpcTarget.All);

        Debug.Log("Next() : 다음 플레이어에게 순서가 넘어갑니다.");
    }

    //=========================================================================
    // Rule() - BETA
    // 설명
    // 1. TABLE의 Rule을 확인합니다.
    //   1) Rule을 만족시키지 못하면 False
    //   2) Rule을 만족하면 True
    //=========================================================================
    public bool Rule()
    {
        // 카드의 규칙을 연산하기 위한 변수
        int type = 0;           // 연속 방식 - 1 : 같은 숫자, 2 : 같은 색상         
        int Count_Continue = 0; // 연속된 카드 카운트
        string pre_num = "-1";
        string pre_col = "yellow";
        string cur_num;
        string cur_col;
        string jok_num = "-1";
        string jok_col = "yellow";
        List<string> Card_Color = new List<string>(); //색상이 겹치는지 확인할 때 사용하는 리스트

        // 규칙 검사 시작
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                cur_num = Table[row, col].CardNumber;
                cur_col = Table[row, col].CardColor;

                // 해당 위치에 카드가 없을 때
                if (cur_num == "-1")
                {
                    if (Count_Continue == 1 || Count_Continue == 2)
                    {
                        Debug.Log("     Fail:카드 개수 3개 이하");
                        return false;
                    }
                    else
                    {
                        pre_num = "-1";
                        pre_col = "yellow";
                        Count_Continue = 0;
                        type = 0;
                        Card_Color.Clear();
                        continue;
                    }
                }

                // 해당 위치에 카드가 있을 때
                else
                {
                    if (type == 1 && Count_Continue == 13)
                    {
                        Count_Continue = 0;
                        type = 0;
                        pre_num = "-1";
                        pre_col = "yellow";
                        continue;
                    }
                    if (type == 2 && Count_Continue == 4)
                    {
                        Count_Continue = 0;
                        type = 0;
                        pre_num = "-1";
                        pre_col = "yellow";
                        continue;
                    }

                    // 카드가 한장만 확정일 때
                    if (Count_Continue == 0)
                    {
                        Debug.Log("     카드가 한장만 확정");
                        if (cur_num == "J")
                        {
                            // 맨 앞에 조커가 나왔을 경우
                            Debug.Log("     가장 앞 조커");
                            pre_num = cur_num;
                            Count_Continue++;
                            type = 0;
                            continue;
                        }
                        
                        Card_Color.Add(cur_col);
                        pre_num = cur_num;
                        pre_col = cur_col;
                        Count_Continue++;
                        type = 0;
                        continue;
                    }

                    // 조커가 카드덱의 가장 앞에 있을 경우
                    if (Count_Continue == 1 && pre_num =="J")
                    {
                        Card_Color.Add(cur_col);
                        pre_num = cur_num;
                        pre_col = cur_col;
                        Count_Continue++;
                        continue;
                        // 아직 타입을 결정 할 수 없다.
                    }

                    // 타입이 결정되지 않을 채 조커가 2번째 카드로 나왔을 경우
                    if (Count_Continue == 1 && cur_num == "J")
                    {
                        Count_Continue++;
                        continue;
                    }

                    // 카드가 2장 이상 확정일 때
                    if (type == 0)                                          // 어떤 규칙인지 설정이 안되었을 때 실행
                        type = (pre_col != cur_col) ? 1 : 2;                // 색상이 다르면 같은 숫자가 연속됩니다.(1), 색상이 같으면 숫자가 오름차순입니다.(2) 

                    Debug.Log("pre:" +pre_col + pre_num);

                    if (type == 1)
                    {
                        // 같은 숫자가 연속됩니다. 
                        // EX ) 1(red), 1(blue), 1(yellow), 1(black)
                        // 1. 숫자가 다르면 안된다. 
                        // 2. 색상이 같으면 안된다.

                        if(cur_num == "J")
                        {
                            Count_Continue++;
                            continue;
                        }

                        // 카드의 숫자가 다름 -> 1번 위반
                        if (pre_num != cur_num)
                        {
                            Debug.Log("     Fail:(1)-1 같은 숫자가 아님");
                            Debug.Log("     pre:" + pre_col + pre_num + "/ cur:" + cur_col + cur_num);                 
                            return false;
                        }

                        // 카드의 색상이 겹침 -> 2번 위반
                        if (Card_Color.Contains(cur_col))
                        {
                            Debug.Log("     Fail:(1)-2 색상이 겹침");
                            return false;
                        }

                        // 다음 카드 확인
                        pre_num = cur_num;
                        pre_col = cur_col;
                        Card_Color.Add(cur_col);
                        Count_Continue++;
                    }
                    else if (type == 2)
                    {
                        // 같은 색상 연속
                        // EX ) 1(red), 2(red), 3(red), 4(red) ...
                        // 1. 숫자가 연속되지 않으면 안된다.
                        // 2. 색상이 다르면 안된다.

                        if(cur_num == "J")
                        {
                            jok_num = (int.Parse(pre_num) + 1).ToString();
                            jok_col = pre_col;
                            pre_num = "J";
                            Count_Continue++;
                            continue;
                        }

                        if (pre_num != "J")
                        {
                            // 카드 사이가 1 차이가 아님 -> 1번 위반
                            if (int.Parse(pre_num) + 1 != int.Parse(cur_num))
                            {
                                Debug.Log("     Fail:(2)-1 오름차순이 아님");
                                Debug.Log("     pre:" + pre_col + pre_num + "/ cur:" + cur_col + cur_num);

                                return false;
                            }
                            // 카드의 색상이 다름 -> 2번 위반
                            if (pre_col != cur_col)
                            {
                                Debug.Log("     Fail:(2)-2 같은 색상이 아님");
                                Debug.Log("     pre:" + pre_col + pre_num + "/ cur:" + cur_col + cur_num);

                                return false;
                            }

                            // 다음 카드 확인
                            pre_num = cur_num;
                            pre_col = cur_col;
                            Count_Continue++;
                        }
                        else
                        {
                            // 카드 사이에 조커가 껴 있을 경우
                            // 카드 사이가 2 차이가 아님 -> 1번 위반
                            if (int.Parse(jok_num) + 1 != int.Parse(cur_num))
                            {
                                Debug.Log("     Fail:(2)-1 오름차순이 아님(조커 Rule)");
                                Debug.Log("     pre:" + jok_col + jok_num + "/ cur:" + cur_col + cur_num);

                                return false;
                            }
                            // 카드의 색상이 다름 -> 2번 위반
                            if (jok_col != cur_col)
                            {
                                Debug.Log("     Fail:(2)-2 같은 색상이 아님(조커 Rule)");
                                Debug.Log("     pre:" + jok_col + jok_num + "/ cur:" + cur_col + cur_num);

                                return false;
                            }

                            // 다음 카드 확인
                            pre_num = cur_num;
                            pre_col = cur_col;
                            Count_Continue++;
                        }
                    }
                }
            } // for - col
        } // for - row
        return true;    // 규칙 문제가 없을 시 true 반환
    }

    //=========================================================================
    // Backup() : TABLE[] 배열의 내용을 TABLE_backup[] 배열로 복사합니다.
    // Sync_Turn() : 게임의 Turn을 동기화 합니다.
    // Count_ClientCard() : 클라이언트들이 자신의 카드를 셉니다. 그 수만큼을 반환합니다.
    //=========================================================================
    [PunRPC]
    void Backup()
    {
        Debug.Log("백업 시작");
        // 테이블 백업
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                TableBackup[row, col] = new Card(Table[row, col].CardNumber, Table[row, col].CardColor);
            }
        }
        // 클라이언트 카드 백업
        ClientCardBackup.Clear();
        for (int i = 0; i < ClientCard.Count; i++)
        {
            ClientCardBackup.Add(new Card(ClientCard[i].CardNumber, ClientCard[i].CardColor));
        }
        Debug.Log("백업 끝");
    }

    [PunRPC]
    void Sync_Turn(int turn)
    {
        _turn = turn;
    }

    [PunRPC]
    int Count_ClientCard()
    {
        ClientCardNum = 0;
        foreach (Card item in ClientCard)
        {
            if (item.CardNumber != "-1" && item.CardNumber != "")
            {
                ClientCardNum++;
            }
        }
        return ClientCardNum;
    }

    //=========================================================================
    // Table 동기화
    // 설명
    // 1. 자신의 TABLE을 모든 클라이언트에게 동기화 시킵니다.
    // 2. raw와 col로 Table 배열의 인덱스로 접근하며, num_col에 해당 Table 요소의 숫자와 색상을 저장합니다.
    //=========================================================================
    [PunRPC]
    void Sync_TABLE()
    {
        Debug.Log("게임판 동기화 시작");
        string[] numCol = { "", "" };
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                numCol[0] = Table[row, col].CardNumber;
                numCol[1] = Table[row, col].CardColor;
                photonView.RPC("Sync_TABLE_Element", RpcTarget.Others, row, col, numCol);
            }
        }
        Debug.Log("게임판 동기화 끝");
    }
    
    //=========================================================================
    // Table 요소 동기화 - 위 함수에 의해 호출됩니다.
    // 설명
    // 1. TABLE들의 요소를 동기화 합니다.
    // 2. row, col 으로 TABLE의 인덱스를 나타내며, numCol 배열에 숫자와 색상이 담겨 있습니다.
    //=========================================================================
    [PunRPC]
    void Sync_TABLE_Element(int row, int col, string[] numCol)
    {
        Table[row, col].CardNumber = numCol[0];
        Table[row, col].CardColor = numCol[1];
    }

    //=========================================================================
    // Table 가져오기
    // 설명
    // 1. 게임판에 있는 데이터를 Table 배열로 가져옵니다.
    //=========================================================================
    void Get_TABLE()
    {
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                try
                {
                    Table[row, col] = new Card(tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text,
                        tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color);
                    //Table[row, col].CardNumber = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text;
                    if (Table[row, col].CardNumber == "")
                    {
                        Table[row, col].CardNumber = "-1";
                    }
                    //Table[row, col].CardColor = Card.ConvertToCardColor(tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color);
                }
                catch (Exception e)
                {
                    Debug.Log("getTable 에러발생 : " + e.Message);
                    return;
                }
            }
        }
    }

    //=========================================================================
    // Table 띄우기
    // 설명
    // 1. Table 배열에 있는 데이터를 게임판으로 보냅니다.
    //=========================================================================
    [PunRPC]
    void View_TABLE()
    {
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {

                tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text = Table[row, col].CardNumber;
                tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color = Table[row, col].RealColor;

                if (Table[row, col].CardNumber != "" && Table[row, col].CardNumber != "-1")
                {
                    tableTop.GetChild(row).GetChild(col).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
                }
                else
                {
                    tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text = "";
                    tableTop.GetChild(row).GetChild(col).GetComponent<Image>().color = new Color(0, 0, 0, 0);
                }
            }
        }
    }

    //=========================================================================
    // Card 가져오기
    // 설명
    // 1. 사용자가 들고있는 카드를 clientCard 배열 가져옵니다.
    //=========================================================================
    void Get_ClientCard()
    {
        ClientCard.Clear();

        for (int i = 0; i < cardHandTop.childCount; i++)
        {
            Card newCard = new Card(cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color);
            if (newCard.CardNumber == "")
                newCard.CardNumber = "-1";
            ClientCard.Add(newCard);
        }

        for (int i = 0; i < cardHandBot.childCount; i++)
        {
            Card newCard = new Card(cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color);
            if (newCard.CardNumber == "")
                newCard.CardNumber = "-1";
            ClientCard.Add(newCard);
        }
    }

    //=========================================================================
    // Card 띄우기
    // 설명
    // 1. 클라이언트가 마스터로부터 동기화 받은 카드를 확인합니다.
    //=========================================================================
    [PunRPC]
    void View_ClientCard()
    {
        int halfSize = MaxHandSize / 2;
        for (int i = 0; i < halfSize; i++)
        {
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = ClientCard[i].CardNumber;
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = ClientCard[i].RealColor;
            if (ClientCard[i].CardNumber != "-1" && ClientCard[i].CardNumber != "")
            {

                cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
            }
            else
            {
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }

            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = ClientCard[i + halfSize].CardNumber;
            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color = ClientCard[i + halfSize].RealColor;
            if (ClientCard[i + halfSize].CardNumber != "-1" && ClientCard[i+halfSize].CardNumber != "")
            {
                cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
            }
            else
            {
                cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0, 0, 0, 0);
            }
        }
    }

    //=========================================================================
    // 타이머 동기화
    // 설명
    // 1. 마스터의 타이머를 동기화 하는 프로그램 입니다.
    //=========================================================================
    [PunRPC]
    void SyncTime(float time)
    // void SyncTime(String time)
    {
        textTimer.text = time.ToString("N1");
    }

    //=========================================================================
    // Serve_Card()
    // 1. 마스터가 해당 클라이언트에게 카드를 제공합니다.
    // --------------------------------------------------------
    // Receive_Card()
    // 1. 마스터가 제공한 카드를 받습니다.
    // 2. 제공 받은 카드는 빈 공간에 채워집니다.
    //=========================================================================
    [PunRPC]
    void Serve_Card(int playerNum)
    {
        Card card = deck[deck.Count - 1];
        deck.RemoveAt(deck.Count - 1);

        Debug.Log("Serve_Card : " + playerNum + "에게" + card.CardColor+card.CardNumber+"전달.");
        photonView.RPC("Receive_Card", RpcTarget.All, playerNum, new string[] { card.CardNumber, card.CardColor });
    }

    [PunRPC]
    void Receive_Card(int playerNum, string[] numCol)
    {
        if (playerNum != _playerNum)
            return;
        int index = 0;
        for (index = 0; index < MaxHandSize; index++)
        {
            if (ClientCard[index].CardNumber == "-1")
            {
                Debug.Log("   index =" + index);
                break;
            }
        }

        ClientCard[index] = new Card(numCol[0], numCol[1]);
        Count_ClientCard();
        View_TABLE();
        View_ClientCard();
    }

    //=========================================================================
    // ClientCard List Clear
    // 설명
    // 1. ClientCard List를 초기화 합니다.
    // 2. 게임 재실행 시 사용됩니다.
    //=========================================================================
    [PunRPC]
    void Clear_ClientCard()
    {
        ClientCard.Clear();
    }
    
    //=========================================================================
    // 카드 개수 동기화 - 마스터만 실행
    // 설명
    // 1. 클라이언트가 가지고 있는 카드 갯수를 동기화합니다.
    // --------------------------------------------------------
    // 카드 개수 보고
    //설명
    // 1. 자신이 가지고 있는 카드의 수를 마스터에게 보고합니다.
    //---------------------------------------------------------
    // 카트 개수 프린트
    // 설명
    // 1. 플레이어별 카드의 개수를 프린트 합니다.
    //=========================================================================
    [PunRPC]
    void Sync_ClientCardNum(int[] num)
    {
        for (int index = 0; index < _playerCount; index++)
            ClientCardNum_Board[index] = num[index];
    }

    [PunRPC]
    void Report_ClientCardNum(int playerNum,int cardCount)
    {
        ClientCardNum_Board[playerNum] = cardCount;
    }

    [PunRPC]
    void Print_ClientCardNum()
    {
        for (int index = 0; index < _playerCount; index++)
        {
            PLAYERS.GetChild(index).GetChild(1).GetComponent<Text>().text = ClientCardNum_Board[index].ToString();
        }
    }
}
