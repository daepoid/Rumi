using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.CardControl
{
    public class TableTopCardManager : MonoBehaviour
    {
        List<TableTopSlot> slots;

        public TableTopCardManager()
        {
            slots = new List<TableTopSlot>();
            for (int i = 0; i < 10; i++)
            {
                slots.Add(new TableTopSlot(i));
            }
        }

        public void reset()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].reset();
            }
        }

        TableTopSlot FindEmptySlot()
        {
            return slots.Find(x => x.is_empty());
        }
        
        // 드래그 앤 드롭을 통해 놓으려는 자리를 확인함
        // 규칙과 관계없이 수행됨
        TableTopSlot FindSlot(string cardNumber, string color)
        {
            TableTopSlot temp = new TableTopSlot();
            return temp;
        }

        public void PutOnCard(Card card)
        {
            TableTopSlot slot = FindSlot(card.number, card.color);
            if (slot == null)
            {
                Debug.Log("PutOnCard Error");
                return;
            }
        }

        public void RemoveCard(Card card)
        {
            TableTopSlot slot = FindSlot(card.number, card.color);
            if (slot != null)
            {
                slot.remove_card(card);
                return;
            }
            Debug.Log("RemoveCard Error");
        }

        // 카드들이 규칙에 맞게 잘 놓여졌는지 확인
        public bool ValidateTableTopCards()
        {
            return true;
        }

        public bool is_empty()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].is_empty())
                {
                    return false;
                }
            }

            return true;
        }
    }
}