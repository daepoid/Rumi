using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SortButtonHandler : MonoBehaviour
{
    public Transform CardHandTop;
    public Transform CardHandBot;
    
    public void OnClickSortbyColor()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < CardHandTop.childCount; i++)
        {
            cards.Add(CardHandTop.GetChild(i).GetComponent<Card>());
        }

        for (int i = 0; i < CardHandBot.childCount; i++)
        {
            cards.Add(CardHandBot.GetChild(i).GetComponent<Card>());
        }
        cards.Sort(delegate(Card cardA, Card cardB)
        {
            if (cardA.color == cardB.color)
            {
                return String.Compare(cardA.number, cardB.number);
            }
            else
            {
                return String.Compare(cardA.color, cardB.color);
            }
        });
        UpdateHand(cards);
        CardHandTop.GetComponent<CardSlot>().UpdateCardSlot();
        CardHandBot.GetComponent<CardSlot>().UpdateCardSlot();
        Debug.Log("SortbyColor 789");
    }
    
    public void OnClickSortbyNumber()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < CardHandTop.childCount; i++)
        {
            cards.Add(CardHandTop.GetChild(i).GetComponent<Card>());
        }

        for (int i = 0; i < CardHandBot.childCount; i++)
        {
            cards.Add(CardHandBot.GetChild(i).GetComponent<Card>());
        }
        
        cards.Sort(delegate(Card cardA, Card cardB)
        {
            if (cardA.number == cardB.number)
            {
                return String.Compare(cardA.color, cardB.color);
            }
            else
            {
                return String.Compare(cardA.number, cardB.number);
            }
        });
        UpdateHand(cards);
        CardHandTop.GetComponent<CardSlot>().UpdateCardSlot();
        CardHandBot.GetComponent<CardSlot>().UpdateCardSlot();
        Debug.Log("SortByColor 777");
    }

    public void UpdateHand(List<Card> cards)
    {
        for (int i = 0; i < 22; i++)
        {
            if (i < 11)
            {
                CardHandTop.GetChild(i).GetComponent<Card>().color = cards[i].color;
                CardHandTop.GetChild(i).GetComponent<Card>().number = cards[i].number;
            }
            else
            {
                CardHandBot.GetChild(i % 11).GetComponent<Card>().color = cards[i].color;
                CardHandBot.GetChild(i % 11).GetComponent<Card>().color = cards[i].color;
            }
        }
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
