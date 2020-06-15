using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SortButtonHandler : MonoBehaviour
{
    public Transform cardHandTop;
    public Transform cardHandBot;
    
    
    // 어차피 정렬이나 대충 들어가도 될듯
    public void OnClickSortByColor()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < MyGameManager.MaxHandSize / 2; i++)
        {
            Card newCard1 = new Card(cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color);
            Card newCard2 = new Card(cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color);
            cards.Add(newCard1);
            cards.Add(newCard2);
        }
        cards.Sort((x, y) =>
        {
            int comp = String.Compare(x.CardColor, y.CardColor);
            if (comp == 0)
            {
                if (x.CardNumber == "J" && y.CardNumber == "J")
                {
                    return 0;
                }   
                else if (x.CardNumber == "J")
                {
                    return -1;
                }
                else if (y.CardNumber == "J")
                {
                    return 1;
                }
                else
                {
                    return Int32.Parse(x.CardNumber).CompareTo(Int32.Parse(y.CardNumber));
                }
            }
            else
            {
                return comp;
            }
        });
        MoveDataToCardHands(cards);
        
        // List<Card> sortedCards = cards.OrderBy(x => x.CardColor).ThenBy(x => x.CardNumber).ToList();
        // List<Card> sortedCards = cards.OrderBy(x => x.CardColor).ThenBy(x => x.CardNumber).ToList();
        // MoveDataToCardHands(sortedCards);
        Debug.Log("SortByColor 789");
    }
    
    public void OnClickSortByNumber()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < MyGameManager.MaxHandSize / 2; i++)
        {
            Card newCard1 = new Card(cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color);
            Card newCard2 = new Card(cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color);
            cards.Add(newCard1);
            cards.Add(newCard2);
        }
        cards.Sort((x, y) =>
        {
            if (x.CardNumber == "J" && y.CardNumber == "J")
            {
                return String.Compare(x.CardColor, y.CardColor);
            }   
            else if (x.CardNumber == "J")
            {
                return -1;
            }
            else if (y.CardNumber == "J")
            {
                return 1;
            }
            else
            {
                int comp = Int32.Parse(x.CardNumber).CompareTo(Int32.Parse(y.CardNumber));
                if (comp == 0)
                {
                    return String.Compare(x.CardColor, y.CardColor);
                }
                else
                {
                    return comp;
                }
            }
        });
        Debug.Log("SortByNumber 777");
    }

    private void MoveDataToCardHands(List<Card> cards)
    {
        int halfSize = MyGameManager.MaxHandSize / 2;
        // for (int i = 0; i < MyGameManager.MaxHandSize; i++)
        for (int i = 0; i < halfSize; i++)
        {
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = cards[i].CardNumber;
            cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = cards[i].RealColor;
            
            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = cards[i + halfSize].CardNumber;
            cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color = cards[i + halfSize].RealColor;
            // if (i < halfSize)
            // {
            //     cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
            //     cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
            //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
            //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
            // }
            // else
            // {
            //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
            //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
            // }
        }
    }
    
    
    // private void MoveDataToCardHands(List<Transform> cards)
    // {
    //     int halfSize = MyGameManager.MaxHandSize / 2;
    //     // for (int i = 0; i < MyGameManager.MaxHandSize; i++)
    //     for (int i = 0; i < halfSize; i++)
    //     {
    //         cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text =
    //             cards[i].GetChild(0).GetComponent<Text>().text;
    //         cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color =
    //             cards[i].GetChild(0).GetComponent<Text>().color;
    //         
    //         cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text =
    //             cards[i + halfSize].GetChild(0).GetComponent<Text>().text;
    //         cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color =
    //             cards[i + halfSize].GetChild(0).GetComponent<Text>().color;
    //         // if (i < halfSize)
    //         // {
    //         //     cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
    //         //     cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
    //         //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
    //         //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
    //         // }
    //         // else
    //         // {
    //         //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
    //         //     cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
    //         // }
    //     }
    // }
}
