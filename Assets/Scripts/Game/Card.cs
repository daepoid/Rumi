using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card
{
    public string number; // J:Joker, 1~13:Number
    public string color; // black, blue, red, yellow

    public Card()
    {
        number = "0";
        color = "green";
    }
    
}
