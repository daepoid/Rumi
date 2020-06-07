using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public string number; // J:Joker, 1~13:Number
    public string color; // black, blue, red, yellow

    public Card()
    {
        number = "0";
        color = "black";
    }

    public Card(string number, string color)
    {
        this.number = number;
        this.color = color;
    }
    
}
