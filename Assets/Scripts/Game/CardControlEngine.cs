using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardControlEngine : MonoBehaviour
{
    public Transform invisibleCard;
    private List<CardSlot> _cardSlots;
    CardSlot _workingCardHand;        // BeginDrag를 수행 할 때 선택 된 Card의 CardHand
    int _beforeCardIndex;             // BeginDrag를 수행 할 때 선택 된 Card의 CardHand에서의 Index
    public CardSlot whichCardSlot;

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
    
    void Update()
    {
        
    }

    public static void SwapCards(Transform sour, Transform dest)
    {
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
        Debug.Log("Swap Cards By Info");
        Card cardTemp = new Card();
        // Transform temp = sour;
        // sour = dest;
        // dest = temp;
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
        Debug.Log("BeginDrag");
        _workingCardHand = _cardSlots.Find(t => ContainPos(t.transform as RectTransform, card.position));
        _beforeCardIndex = card.GetSiblingIndex();
        // SwapCardsInHierarchy(invisibleCard, card);
        SwapCardsByInvisible(invisibleCard, card);
    }
    
    void Drag(Transform card)
    {
        // CardSlot 찾기
        whichCardSlot = _cardSlots.Find(t=> ContainPos(t.transform as RectTransform, card.position));
        if (whichCardSlot == null)
        {
            // CardSlot이 없는 곳에 놓을 경우 원래 자리로 돌아가야한다.
            Debug.Log("Drag whichCardSlot == null");
            invisibleCard.SetParent(transform);
            if(invisibleCard.parent.parent != transform)
            {
                _cardSlots.ForEach(t=>t.UpdateCardSlot());
            }
        }
        else // CardSlot이 있는 곳을 지나는 경우
        {
            Debug.Log("whichCardSlot.originCardCount : " + whichCardSlot.originCardCount.ToString());
            Debug.Log("Drag whichCardSlot != null\n" + invisibleCard.parent.name);
            if (invisibleCard.parent == transform)
            {
                // invisibleCard가 Canvas의 바로 아래 hierarchy에 위치하는 경우
                int index = whichCardSlot.GetIndexByPosition(card);
                Debug.Log("whichCardSlot : " + index.ToString());
                invisibleCard.SetParent(whichCardSlot.transform);
                whichCardSlot.InsertCard(invisibleCard, index);
            }
            else
            {
                // invisibleCard가 Canvas의 바로 아래 hierarchy에 위치하지 않는 경우
                _invisibleCardIndex = invisibleCard.GetSiblingIndex();
                _targetIndex = whichCardSlot.GetIndexByPosition(card, _invisibleCardIndex);
                Debug.Log("TargetIndex : " + _targetIndex.ToString() + "\nwhichCardhandName : " + whichCardSlot.name);
                
                if (_invisibleCardIndex != _targetIndex)
                {
                    SwapCardsByInfo(
                        whichCardSlot.transform.GetChild(_invisibleCardIndex),
                        whichCardSlot.transform.GetChild(_targetIndex)
                        );
                    whichCardSlot.UpdateCardSlot();
                    // whichCardSlot.SwapCard2(_invisibleCardIndex, targetIndex);
                    // whichCardSlot.SwapCard(_invisibleCardIndex, targetIndex);
                }
            }
        }
    }
    
    void EndDrag(Transform card)
    {
        Debug.Log("EndDrag : " + invisibleCard.parent.name);
        if (invisibleCard.parent == transform)
        {
            Debug.Log("invisibleCard.parent==transform");
            card.SetParent(_workingCardHand.transform);
            _workingCardHand.InsertCard(card, _beforeCardIndex);
            _workingCardHand = null;
            _beforeCardIndex = -1;
        }
        else if (whichCardSlot.CountRealCard() > whichCardSlot.originCardCount - 1)
        {
            Debug.Log("카드 제한을 넘는 경우");
            Debug.Log(whichCardSlot.transform.GetChild(_targetIndex).name);
            // invisibleCard 제자리로 보내기
            invisibleCard.SetParent(transform);
            card.SetParent(_workingCardHand.transform);
            _workingCardHand.InsertCard(card, _beforeCardIndex);
            _workingCardHand = null;
            _beforeCardIndex = -1;
            SwapCardsInHierarchy(whichCardSlot.transform.GetChild(_targetIndex), card);
        }
        else
        {
            Debug.Log("invisibleCard.parent!=transform");
            Debug.Log("whichCardSlot Count : " + whichCardSlot.CountRealCard().ToString());
            SwapCardsByInvisible(invisibleCard, card);
        }
        // Debug.Log("EndDrag : " + invisibleCard.parent.name);
        // // whichCardSlot = _cardSlots.Find(t => ContainPos(t.transform as RectTransform, card.position));
        // if (invisibleCard.parent == transform)
        // {
        //     Debug.Log("invisibleCard.parent==transform");
        //     card.SetParent(_workingCardHand.transform);
        //     _workingCardHand.InsertCard(card, _beforeCardIndex);
        //     _workingCardHand = null;
        //     _beforeCardIndex = -1;
        // }
        // else if (whichCardSlot.CountRealCard() > whichCardSlot.originCardCount - 1)
        // {
        //     Debug.Log("카드 제한을 넘는 경우");
        //     Debug.Log(whichCardSlot.transform.GetChild(targetIndex).name);
        //     invisibleCard.SetParent(transform);
        //     card.SetParent(_workingCardHand.transform);
        //     _workingCardHand.InsertCard(card, _beforeCardIndex);
        //     _workingCardHand = null;
        //     _beforeCardIndex = -1;
        //     SwapCardsInHierarchy(whichCardSlot.transform.GetChild(targetIndex), card);
        // }
        // else
        // {
        //     Debug.Log("invisibleCard.parent!=transform");
        //     Debug.Log("whichCardSlot Count : " + whichCardSlot.CountRealCard().ToString());
        //     SwapCardsInHierarchy(invisibleCard, card);
        // }
    }
}
