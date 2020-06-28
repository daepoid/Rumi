using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System;

public partial class MyGameManager : MonoBehaviourPunCallbacks 
{
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
        bool joker = false;

        // 규칙 검사 시작
        for (int row = 0; row < TableRow; row++)
        {
            for (int col = 0; col < TableCol; col++)
            {
                cur_num = Table[row, col].CardNumber;
                cur_col = Table[row, col].CardColor;

                Debug.Log("현재카드:" + cur_col + cur_num);

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
                    if (type == 0 && cur_num == "J")
                    {
                        joker = true;
                        jok_num = (int.Parse(pre_num) + 1).ToString();
                        jok_col = pre_col;
                        Count_Continue++;
                        continue;
                    }

                    // 카드가 2장 이상 확정일 때
                    if (type == 0)                                          // 어떤 규칙인지 설정이 안되었을 때 실행
                        type = (pre_col != cur_col) ? 1 : 2;                // 색상이 다르면 같은 숫자가 연속됩니다.(1), 색상이 같으면 숫자가 오름차순입니다.(2) 


                    if (type == 1)
                    {
                        // 같은 숫자가 연속됩니다. 
                        // EX ) 1(red), 1(blue), 1(yellow), 1(black)
                        // 1. 숫자가 다르면 안된다. 
                        // 2. 색상이 같으면 안된다.

                        if (cur_num == "J")
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

                        if (joker)
                        {
                            Debug.Log("     현재 조커가 2번째 있었음.");
                            pre_num = jok_num;
                            pre_col = jok_col;
                            joker = false;
                        }

                        if (cur_num == "J")
                        {
                            joker = true;
                            jok_num = (int.Parse(pre_num) + 1).ToString();
                            jok_col = pre_col;
                            Count_Continue++;
                            continue;
                        }

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
                }
            } // for - col
        } // for - row
        return true;    // 규칙 문제가 없을 시 true 반환
    }
}