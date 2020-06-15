using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string NickName { get; set; }
    public List<Card> cards = new List<Card>();
    public int cardNum;
}