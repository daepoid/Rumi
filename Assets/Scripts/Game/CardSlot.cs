using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : MonoBehaviour
{ 
    private List<Transform> _cards;
    public int originCardCount = 0;
    void Start()
    {
        _cards = new List<Transform>();
        UpdateCardSlot();
        originCardCount = CountRealCard();
    }

    public void UpdateCardSlot()
    {
        _cards.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            _cards.Add(transform.GetChild(i));
        }
    }
    
    public void InsertCard(Transform card, int index)
    {
        _cards.Add(card);
        card.SetSiblingIndex(index);
        UpdateCardSlot();
    }

    public int GetIndexByPosition(Transform card, int skipIndex = -1)
    {
        int result = 0;
        for (int i = 0; i < _cards.Count; i++)
        {
            if (card.position.x < _cards[i].position.x)
            {
                break;
            }
            if (skipIndex != i)
            {
                result++;
            }
        }
        return result;
    }

    public void SwapCard(int index01, int index02)
    {
        CardControlEngine.SwapCards(_cards[index01], _cards[index02]);
        UpdateCardSlot();
    }
    public void SwapCard2(int index01, int index02)
    {
        CardControlEngine.SwapCardsByInfo(_cards[index01], _cards[index02]);
        UpdateCardSlot();
    }
    public int CountRealCard()
    {
        var realCards = transform.GetComponentsInChildren<Draggable>();
        return realCards.Length;
    }
    
    void Update()
    {
        
    }
}