using System;
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
        Debug.Log("BeginDrag");
        _workingCardHand = _cardSlots.Find(t => ContainPos(t.transform as RectTransform, card.position));
        _beforeCardIndex = card.GetSiblingIndex();
        // SwapCardsInHierarchy(invisibleCard, card);
        SwapCardsByInvisible(invisibleCard, card);
    }
    
    void Drag(Transform card)
    {
        // CardSlot 찾기
        _whichCardSlot = _cardSlots.Find(t=> ContainPos(t.transform as RectTransform, card.position));
        Debug.Log("Drag : " + invisibleCard.parent.name);
        Debug.Log(_whichCardSlot);
        if (_whichCardSlot == null)
        {
            // CardSlot이 없는 곳에 놓을 경우 원래 자리로 돌아가야한다.
            Debug.Log("Drag _whichCardSlot == null");
            // invisibleCard.SetParent(transform);
            if(invisibleCard.parent.parent != transform)
            {
                _cardSlots.ForEach(t=>t.UpdateCardSlot());
            }
        }
        else // CardSlot이 있는 곳을 지나는 경우
        {
            // 기존의 방식은 카드를 교환하는 방식
            // 카드의 정보만 교환하는 방식으로 교체
            _invisibleCardIndex = invisibleCard.GetSiblingIndex();
            _targetIndex = _whichCardSlot.GetIndexByPosition(card, _invisibleCardIndex);
            Debug.Log("TargetIndex : " + _targetIndex.ToString() + "\nwhichCardhandName : " + _whichCardSlot.name);
        }
    }
    
    void EndDrag(Transform card)
    {
        Debug.Log("EndDrag\ninvisibleCard position : " + invisibleCard.parent.name);
        // if (invisibleCard.parent == transform)
        // if(_whichCardSlot == null || !MyGameManager.ControlFlag)
        if(_whichCardSlot == null)
        {
            Debug.Log("invisibleCard.parent==transform");
            card.SetParent(_workingCardHand.transform);
            _workingCardHand.InsertCard(card, _beforeCardIndex);
            _workingCardHand = null;
            _beforeCardIndex = -1;
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
            Debug.Log(_whichCardSlot.transform.GetChild(_targetIndex).GetChild(0).GetComponent<Text>().text);
            // invisibleCard 제자리로 보내기
            invisibleCard.SetParent(transform);
            SwapCardsByInfo(card, _whichCardSlot.transform.GetChild(_targetIndex));
            card.SetParent(_workingCardHand.transform);
            _workingCardHand.InsertCard(card, _beforeCardIndex);
            _workingCardHand = null;
            _beforeCardIndex = -1;
        }
        else
        {
            // 모두 차 있지 않아 자리를 바꾸지 않고 그냥 추가한다.
            Debug.Log("invisibleCard.parent!=transform");
            Debug.Log("_whichCardSlot Count : " + _whichCardSlot.CountRealCard().ToString());
            SwapCardsByInvisible(invisibleCard, card);
            // 자리가 비어있으므로 해당 cardslot에 집어 넣는다.
            _whichCardSlot.InsertCard(card, _targetIndex);
            // SwapCardsByInfo(card, _whichCardSlot.transform.GetChild(_targetIndex));
        }
        _cardSlots.ForEach(t=>t.UpdateCardSlot());
        // _whichCardSlot.RefreshCardSlot();
        // _workingCardHand.RefreshCardSlot();
    }
}
