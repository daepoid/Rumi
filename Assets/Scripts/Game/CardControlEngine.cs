﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardControlEngine : MonoBehaviour
{
    public Transform invisibleCard;
    public Transform tempCard;
    
    private List<CardSlot> _cardSlots;
    private CardSlot _workingCardHand;        // BeginDrag를 수행 할 때 선택 된 Card의 CardHand
    private int _beforeCardIndex;             // BeginDrag를 수행 할 때 선택 된 Card의 CardHand에서의 Index
    private CardSlot _whichCardSlot;
    public static Card WoringCardBackup = new Card();
    public static bool IsDragging = false;
    
    private int _targetIndex;
    private int _invisibleCardIndex;
    
    void Start()
    {
        _cardSlots = new List<CardSlot>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var arrs = transform.GetChild(i).GetComponentsInChildren<CardSlot>();
            for (int j = 0; j < arrs.Length; j++)
            {
                _cardSlots.Add(arrs[j]);
            }
        }
        Debug.Log("_cardSlots : " + _cardSlots.Count.ToString());
    }
    
    public static void SwapCards(Transform sour, Transform dest)
    {
        Debug.Log("Swap Cards Position");
        Transform sourParent = sour.parent;
        Transform destParent = dest.parent;

        int sourIndex = sour.GetSiblingIndex();
        int destIndex = dest.GetSiblingIndex();
        
        sour.SetParent(destParent);
        sour.SetSiblingIndex(destIndex);
        
        dest.SetParent(sourParent);
        dest.SetSiblingIndex(sourIndex);
    }

    public static void SwapCardsByInfo(Transform sour, Transform dest)
    {   
        Debug.Log("Swap Cards Infomation");
        Color textColor = sour.GetChild(0).GetComponent<Text>().color;
        String number = sour.GetChild(0).GetComponent<Text>().text;
        Color imageColor = sour.GetComponent<Image>().color;
        
        sour.GetChild(0).GetComponent<Text>().color = dest.GetChild(0).GetComponent<Text>().color;
        sour.GetChild(0).GetComponent<Text>().text = dest.GetChild(0).GetComponent<Text>().text;
        sour.GetComponent<Image>().color = dest.GetComponent<Image>().color;

        dest.GetChild(0).GetComponent<Text>().color = textColor;
        dest.GetChild(0).GetComponent<Text>().text = number;
        dest.GetComponent<Image>().color = imageColor;
    }

    void SwapCardsInHierarchy(Transform sour, Transform dest)
    {
        // SwapCards(sour, dest);
        SwapCardsByInfo(sour, dest);
        _cardSlots.ForEach(t => t.UpdateCardSlot());
    }
    
    void SwapCardsByInvisible(Transform sour, Transform dest)
    {
        SwapCards(sour, dest);
        _cardSlots.ForEach(t => t.UpdateCardSlot());
    }

    bool ContainPos(RectTransform rt, Vector2 pos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rt, pos);
    }

    void BeginDrag(Transform card)
    {
        Debug.Log("BeginDrag : "  + card.GetChild(0).GetComponent<Text>().text);
        _workingCardHand = _cardSlots.Find(t => ContainPos(t.transform as RectTransform, card.position));
        _beforeCardIndex = card.GetSiblingIndex();
        IsDragging = true;
        WoringCardBackup = new Card(card.GetChild(0).GetComponent<Text>().text, card.GetChild(0).GetComponent<Text>().color);
        // SwapCardsInHierarchy(invisibleCard, card);
        SwapCardsByInvisible(invisibleCard, card);
    }
    
    void Drag(Transform card)
    {
        // if (MyGameManager.Turn != MyGameManager.PlayerNum)
        // {
        //     EndDrag(card);
        //     card.GetComponent<Draggable>().enabled = false;
        // }
        // CardSlot 찾기
        _whichCardSlot = _cardSlots.Find(t=> ContainPos(t.transform as RectTransform, card.position));
        if (_whichCardSlot == null)
        {
            // CardSlot이 없는 곳에 놓을 경우 원래 자리로 돌아가야한다.
            if(invisibleCard.parent.parent != transform)
            {
                _cardSlots.ForEach(t=>t.UpdateCardSlot());
            }
        }
        else // CardSlot이 있는 곳을 지나는 경우
        {
            _invisibleCardIndex = invisibleCard.GetSiblingIndex();
            _targetIndex = _whichCardSlot.GetIndexByPosition(card, _invisibleCardIndex);
        }
    }

    void GoBackBeforeHand(Transform card)
    {
        invisibleCard.SetParent(transform);
        card.SetParent(_workingCardHand.transform);
        _workingCardHand.InsertCard(card, _beforeCardIndex);
        _workingCardHand = null;
        _beforeCardIndex = -1;
    }
    
    void EndDrag(Transform card)
    {
        IsDragging = false;
        if (!card.GetComponent<Draggable>().enabled || MyGameManager.Turn != MyGameManager.PlayerNum)
        {
            GoBackBeforeHand(card);
        }
        // TableTop에서 CardHand 방향으로 가져오지 못하게 만듬
        else if ((!_workingCardHand.cardHandFlag && _whichCardSlot.cardHandFlag))
        {
            GoBackBeforeHand(card);
            // Todo: return의 필요성 확인
            return;
        }
        else if(_whichCardSlot == null)
        {
            Debug.Log("CardSlot 바깥에 카드를 드롭한 경우");
            GoBackBeforeHand(card);
        }
        // else if (MyGameManager.ControlFlag)
        // {
        //     card.SetParent(_workingCardHand.transform);
        //     _workingCardHand.InsertCard(card, _beforeCardIndex);
        //     _workingCardHand = null;
        //     _beforeCardIndex = -1;
        // }
        // else if(!_workingCardHand.cardHandFlag && _whichCardSlot.cardHandFlag)
        // {
        //     card.SetParent(_workingCardHand.transform);
        //     _workingCardHand.InsertCard(card, _beforeCardIndex);
        //     _workingCardHand = null;
        //     _beforeCardIndex = -1;
        // }
        else if (_whichCardSlot.CountRealCard() > _whichCardSlot.OriginCardCount - 1)
        {
            Debug.Log("카드 제한을 넘는 경우");
            // invisibleCard 제자리로 보내기
            GoBackBeforeHand(card);
            string temp = _whichCardSlot.transform.GetChild(_targetIndex).GetChild(0).GetComponent<Text>().text;
            // cardHand에서는 TableTop의 빈 칸에만 놓을 수 있다.
            if (temp == "" || temp == "-1")
            {
                SwapCardsByInfo(card, _whichCardSlot.transform.GetChild(_targetIndex));
            }
            // invisibleCard.SetParent(transform);
            // SwapCardsByInfo(card, _whichCardSlot.transform.GetChild(_targetIndex));
            // card.SetParent(_workingCardHand.transform);
            // _workingCardHand.InsertCard(card, _beforeCardIndex);
            // _workingCardHand = null;
            // _beforeCardIndex = -1;
        }
        else
        {
            // TODO: 지우고 테스트 해보기
            Debug.Log("카드 제한을 넘지 않는 경우");
            SwapCardsByInvisible(invisibleCard, card);               // 모두 차 있지 않아 자리를 바꾸지 않고 그냥 추가한다.
            _whichCardSlot.InsertCard(card, _targetIndex);                        // 자리가 비어있으므로 해당 cardslot에 집어 넣는다.
        }
        _cardSlots.ForEach(t=>t.UpdateCardSlot());
    }
}
