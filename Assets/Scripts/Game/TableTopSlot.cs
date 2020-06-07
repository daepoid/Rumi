using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.CardControl
{
    public class TableTopSlot : MonoBehaviour
    {
        public int slot_position { get; set; }

        public List<Card> cards { get; set; }

        public TableTopSlot()
        {
            
        }
        public TableTopSlot(int position)
        {
            cards = new List<Card>();
            slot_position = position;
            reset();
        }

        public void reset()
        {
            cards.Clear();
        }
        
        public void add_card(Card card)
        {
            cards.Add(card);
        }

        public void remove_card(Card card)
        {
            cards.Remove(card);
        }

        public bool is_empty()
        {
            return cards.Count <= 0;
        }
        
    }
}