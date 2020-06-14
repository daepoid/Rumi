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
    
    public void OnClickSortbyColor()
    {
        List<Transform> cards = new List<Transform>();
        for (int i = 0; i < cardHandTop.childCount; i++)
        {
            cards.Add(cardHandTop.GetChild(i));            
        }

        for (int i = 0; i < cardHandBot.childCount; i++)
        {
            cards.Add(cardHandTop.GetChild(i));
        }
        
        Debug.Log("\nStart Sort!\ncards.Count : " + cards.Count + "\ncards[0].CardNumber : " + cards[0].GetChild(0).GetComponent<Text>().text);
        List<Transform> sortedCards = cards.OrderBy(x => x.GetChild(0).GetComponent<Text>().color.ToString()).ThenBy(x => x.GetChild(0).GetComponent<Text>().text).ToList();
        
        MoveDataToCardHands(sortedCards);
        Debug.Log("SortbyColor 789");
    }
    
    public void OnClickSortbyNumber()
    {
        List<Transform> cards = new List<Transform>();
        for (int i = 0; i < cardHandTop.childCount; i++)
        {
            cards.Add(cardHandTop.GetChild(i));            
        }

        for (int i = 0; i < cardHandBot.childCount; i++)
        {
            cards.Add(cardHandTop.GetChild(i));
        }
        
        Debug.Log("Start Sort!");
        List<Transform> sortedCards = cards.OrderBy(x => x.GetChild(0).GetComponent<Text>().text).ThenBy(x => x.GetChild(0).GetComponent<Text>().color.ToString()).ToList();
        
        MoveDataToCardHands(sortedCards);
        Debug.Log("SortByColor 777");
    }

    private void MoveDataToCardHands(List<Transform> cards)
    {
        int halfSize = MyGameManager.MaxHandSize / 2;
        for (int i = 0; i < MyGameManager.MaxHandSize; i++)
        {
            if (i < halfSize)
            {
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
            }
            else
            {
                cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().text = cards[i].GetChild(0).GetComponent<Text>().text;
                cardHandBot.GetChild(i - halfSize).GetChild(0).GetComponent<Text>().color = cards[i].GetChild(0).GetComponent<Text>().color;
            }
        }
    }
}
