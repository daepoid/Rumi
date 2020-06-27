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
    List<Card> cards = new List<Card>();
    
    public void OnClickSortByColor()
    {
        cards.Clear();
        MakeCardList(cards);
        cards.Sort((x, y) =>
        {
            int comp = String.Compare(x.CardColor, y.CardColor);
            if (comp == 0)
            {
                if (Card.ToSymbolNumberFromCardNumber(x.CardNumber) == Card.ToSymbolNumberFromCardNumber(y.CardNumber))
                {
                    return 0;
                }
                return Card.ToSymbolNumberFromCardNumber(x.CardNumber) <= Card.ToSymbolNumberFromCardNumber(y.CardNumber) ? -1 : 1;
            }
            return comp;
        });
        MoveDataToCardHands(cards);
        MyGameManager.SortButtonFlag = true;
        Debug.Log("SortByColor 789");
    }
    
    public void OnClickSortByNumber()
    {
        cards.Clear();
        MakeCardList(cards);
        cards.Sort((x, y) =>
        {
            if (Card.ToSymbolNumberFromCardNumber(x.CardNumber) == Card.ToSymbolNumberFromCardNumber(y.CardNumber))
            {
                return String.Compare(x.CardColor, y.CardColor);
            }
            return Card.ToSymbolNumberFromCardNumber(x.CardNumber) < Card.ToSymbolNumberFromCardNumber(y.CardNumber) ? -1 : 1;
        });
        MoveDataToCardHands(cards);
        MyGameManager.SortButtonFlag = true;
        Debug.Log("SortByNumber 777");
    }

    private void MakeCardList(List<Card> newCards)
    {
        for (int i = 0; i < MyGameManager.MaxHandSize / 2; i++)
        {
            Card newCard;
            if (cardHandTop.childCount <= i && cardHandBot.childCount <= i)
            {
                return;
            }
            if (cardHandTop.childCount > i)
            {
                if (cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text != "" &&
                    cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text != "-1")
                {
                    newCard = new Card(cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color);
                }
                else
                {
                    newCard = new Card();
                }
                newCards.Add(newCard);
            }
            if (cardHandBot.childCount > i)
            {
                if (cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text != "" &&
                    cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text != "-1")
                {
                    newCard = new Card(cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text, cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color);
                }
                else
                {
                    newCard = new Card();
                }
                newCards.Add(newCard);
            }
        }
    }
    
    private void MoveDataToCardHands(List<Card> newCards)
    {
        int halfSize = MyGameManager.MaxHandSize / 2;
        for (int i = 0; i < halfSize; i++)
        {
            if (newCards.Count > i)
            {
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = newCards[i].CardNumber;
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = newCards[i].RealColor;
                if (newCards[i].CardNumber != "" && newCards[i].CardNumber != "-1")
                {
                    cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
                }
                else
                {
                    cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0.0F, 0.0F, 0.0F, 0F);
                }
            }
            else
            {
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                cardHandTop.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.yellow;
                cardHandTop.GetChild(i).GetComponent<Image>().color = new Color(0.0F, 0.0F, 0.0F, 0F);
            }
            
            if (newCards.Count > i + halfSize)
            {
                cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = newCards[i + halfSize].CardNumber;
                cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color = newCards[i + halfSize].RealColor;
                if (newCards[i + halfSize].CardNumber != "" && newCards[i + halfSize].CardNumber != "-1")
                {
                    cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0.8F, 0.6F, 0.1F, 1F);
                }
                else
                {
                    cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0.0F, 0.0F, 0.0F, 0F);
                }
            }
            else
            {
                cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().text = "";
                cardHandBot.GetChild(i).GetChild(0).GetComponent<Text>().color = Color.yellow;
                cardHandBot.GetChild(i).GetComponent<Image>().color = new Color(0.0F, 0.0F, 0.0F, 0F);
            }
        }
    }
}
