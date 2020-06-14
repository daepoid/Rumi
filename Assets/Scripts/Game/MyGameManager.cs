﻿using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager : MonoBehaviourPunCallbacks
{
    static public readonly int TableRow = 6;                  // 게임판의 RAW 크기
    static public readonly int TableCol = 16;                 // 게임판의 COLUM 크기
    public static readonly int MaxHandSize = 22;
    
    public static bool ControlFlag = false;
    public List<Card> deck = new List<Card>();                  // 전체 카드 덱 : 분배하 남은 카드가 있습니다.
    public static List<Player> Players = new List<Player>();    // 플레이어 : 각 플레이어들의 카드를 저장하고 있습니다.
    public static Card[,] Table = new Card[TableRow, TableCol];      // Rumikub의 게임판을 저장한 배열입니다.
    public static Card[,] TableBackup;                                // 게임판을 백업합니다. 백업은 마스터만 관리합니다.
    public static List<Card> CardHandBackup = new List<Card>();
    public Card[] clientCard = new Card[22];                    // 클라이언트의 카드를 나타냅니다.

    private int _playerCount = 0;
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
    // public Text textTurn;

    private bool turnStartFlag = true;
    // Update is called once per frame
    void Update()
    {
        if (_runningGame == 1)
        {
            if (turnStartFlag)
            {
                ControlFlag = true;
                SaveCardHand();
            }
            if (_playerNum == _turn)
            {
                // 게임판의 사용을 허가하는 코드를 추가해야 합니다.
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

            // 제한시간 60초가 지나면 모든 '사용자'들의 Turn을 1 증가시킵니다.
            // Next를 누르지 못하면 자동으로 규칙을 검사합니다.
            if (PhotonNetwork.IsMasterClient)
            {
                _time += Time.deltaTime;

                if (_time > 60)
                {
                    _time = 0;
                    photonView.RPC("Next", RpcTarget.All);
                }

                photonView.RPC("Sync_Time", RpcTarget.All, _time);
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
    
    //=========================================================================
    // Reset 버튼 - BETA
    // 설명
    // 1. Reset 버튼을 누르면 자신이 작업하기 전 게임판으로 만듭니다.
    //=========================================================================
    public void Reset()
    {
        Table = TableBackup;
        View_TABLE();
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

        Debug.Log("Next 버튼 클릭");

        Get_TABLE();

        if (!Rule())
        {
            // Rule을 만족시키지 못함
            Debug.Log("     Rule : Fail");
            Table = TableBackup;
            View_TABLE();
        }
        else
        {
            // Rule을 만족시킴
            Debug.Log("     Rule : True");
            photonView.RPC("Sync_TABLE", RpcTarget.All);
        }

        photonView.RPC("Backup", RpcTarget.All);
        _time = 0;
        _turn = (_turn + 1) % _playerCount;
        photonView.RPC("Sync_Time", RpcTarget.All, _time);
        photonView.RPC("Sync_Turn", RpcTarget.All, _turn);
        photonView.RPC("View_TABLE", RpcTarget.All);

        Debug.Log("Next 버튼 끝");
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
        Card cur = null;        // 현재 확인하는 카드
        Card pre = null;        // 이 전에 확인한 카드

        // 카드의 규칙을 연산하기 위한 변수
        int type = 0;           // 연속 방식 - 1 : 같은 숫자, 2 : 같은 색상         
        int Count_Continue = 0; // 연속된 카드 카운트
        List<string> Card_Color = new List<string>(); //색상이 겹치는지 확인할 때 사용하는 리스트

        // 규칙 연산 시작
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                cur = Table[row, col];
                Debug.Log("Table[" + row + "," + col + "]:" + Table[row,col].CardColor + Table[row, col].CardNumber);
                Debug.Log("cur:" +cur.CardColor+cur.CardNumber);

                // 해당 위치에 카드가 없을 때
                if (cur.CardNumber == "")
                {
                    if (Count_Continue == 1 || Count_Continue == 2)
                    {
                        Debug.Log("     Fail:카드 개수 3개 이하");
                        return false;
                    }
                    else
                    {
                        pre = null;
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
                        pre = null;
                    }
                    if (type == 2 && Count_Continue == 4)
                    {
                        Count_Continue = 0;
                        type = 0;
                        pre = null;
                    }


                    // 카드가 한장만 확정일 때
                    if (pre == null)
                    {
                        Debug.Log("     카드가 한장만 확정");
                        Card_Color.Add(cur.CardColor);
                        pre = cur;
                        Count_Continue++;
                        type = 0;
                        continue;
                    }


                    // 카드가 2장 이상 확정일 때
                    if (type == 0)                                      // 어떤 규칙인지 설정이 안되었을 때 실행
                        type = (pre.CardColor != cur.CardColor) ? 1 : 2;        // 색상이 다르면 같은 숫자가 연속됩니다.(1), 색상이 같으면 숫자가 오름차순입니다.(2) 

                    Debug.Log("pre:" + pre.CardColor + pre.CardNumber);

                    if (type == 1)
                    {
                        // 같은 숫자가 연속됩니다. 
                        // EX ) 1(red), 1(blue), 1(yellow), 1(black)
                        // 1. 숫자가 다르면 안된다. 
                        // 2. 색상이 같으면 안된다.

                        // 카드의 숫자가 다름 -> 1번 위반
                        if (pre.CardNumber != cur.CardNumber) {
                            Debug.Log("     Fail:(1)-1 같은 숫자가 아님");
                            return false;
                        }

                        // 카드의 색상이 겹침 -> 2번 위반
                        if (Card_Color.Contains(cur.CardColor))
                        {
                            Debug.Log("     Fail:(1)-2 색상이 겹침");
                            return false;
                        }

                        // 다음 카드 확인
                        pre = cur;
                        Card_Color.Add(cur.CardColor);
                        Count_Continue++;
                    }
                    else if (type == 2)
                    {
                        // 같은 색상 연속
                        // EX ) 1(red), 2(red), 3(red), 4(red) ...
                        // 1. 숫자가 연속되지 않으면 안된다.
                        // 2. 색상이 다르면 안된다.

                        // 카드 사이가 1 차이가 아님 -> 1번 위반
                        if (int.Parse(pre.CardNumber) + 1 != int.Parse(cur.CardNumber))
                        {
                            Debug.Log("     Fail:(2)-1 오름차순이 아님");
                            
                            return false;
                        }
                        // 카드의 색상이 다름 -> 2번 위반
                        if (pre.CardColor != cur.CardColor)
                        {
                            Debug.Log("     Fail:(2)-2 같은 색상이 아님");

                            return false;
                        }

                        // 다음 카드 확인
                        pre = cur;
                        Count_Continue++;
                    }
                }
            } // for - col
        } // for - row

        // 규칙 문제가 없을 시 true 반환
        return true;
    }

    //=========================================================================
    // 테이블을 백업합니다.
    // 위 함수에서 호출합니다.
    //=========================================================================
    [PunRPC]
    void Backup()
    {
        Debug.Log("백업 시작");
        TableBackup = Table;
        Debug.Log("백업 끝");
    }

    [PunRPC]
    void Sync_Turn(int turn)
    {
        _turn = turn;
        // textTurn.text = "_turn : "+ _turn.ToString();
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
                    string color = "red";
                    switch (tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color.ToString())
                    {
                        case "RGBA(1.000, 0.000, 0.000, 1.000)": color = "red"; break;
                        case "RGBA(0.000, 0.000, 1.000, 1.000)": color = "blue"; break;
                        case "RGBA(1.000, 0.920, 0.016, 1.000)": color = "yellow"; break;
                        case "RGBA(0.000, 0.000, 0.000, 1.000)": color = "black"; break;
                        default : color = "green"; break;
                    }

                    Table[row, col].CardNumber = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().text;
                    Table[row, col].CardColor = color;
                    // Table[row, col].RealColor = tableTop.GetChild(row).GetChild(col).GetChild(0).GetComponent<Text>().color;
                }
                catch (Exception e)
                {
                    Debug.Log("getTable 에러발생");
                    return;
                }
            }
        }
        // for (int row = 0; row < TableRow; row++)
        // {
        //     for (int col = 0; col < TableCol; col++)
        //     {
        //         Debug.Log("Table[" + row + "," + col + "]" + Table[row, col].CardColor + Table[row, col].CardNumber);
        //     }
        // }
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
            }
        }
    }

    //=========================================================================
    // 타이머 동기화
    // 설명
    // 1. 마스터의 타이머를 동기화 하는 프로그램 입니다.
    //=========================================================================
    [PunRPC]
    void Sync_Time(float time)
    {
        this.textTimer.text = $"{time:N1}";
        // textTimer.text = time.ToString();
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
        Player localPlayer = Players.Find(x => x.NickName == playersInRoom[localPlayerIndex].NickName);
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
        Player Player1 = Players.Find(x => x.NickName == playerInRoom.NickName);
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
        Player cardNumber = Players.Find(x => x.NickName == playerInRoom.NickName);
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
        Player cardNumber = Players.Find(x => x.NickName == playerInRoom.NickName);
        //텍스트 출력
        nameText.text = cardNumber.cards.Length.ToString();
    }
}
