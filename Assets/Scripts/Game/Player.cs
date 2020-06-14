using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string NickName { get; set; }
    static readonly int MAX_CARD = 22;
    public Card[] cards = new Card[MAX_CARD];
    public int cardNum = 22;
    public Player()
    {
        for(int i = 0; i < MAX_CARD; i++)
        {
            cards[i] = new Card();
        }
    }
}