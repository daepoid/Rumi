using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card
{
    public string CardNumber { get; set; } // J:Joker, 1~13:CardNumber
    public string CardColor { get; set; } // black, blue, red, yellow
    public Color RealColor { get; set; }
    public Card()
    {
        CardNumber = "-1";
        CardColor = "green";
    }

    public Card(string number, string color)
    {
        this.CardNumber = number;
        this.CardColor = color;
        this.RealColor = ConvertToRealColor(color);
    }

    public Card(string number, Color color)
    {
        CardNumber = number;
        CardColor = ConvertToCardColor(ColorUtility.ToHtmlStringRGB(color));
        RealColor = color;
    }

    public static String ConvertToCardColor(string color)
    {
        string stringColor;
        switch (color)
        {
            case "000000": stringColor = "black"; break;
            case "FF0000": stringColor = "red"; break;
            case "00FF00": stringColor = "green"; break;
            case "0000FF": stringColor = "blue"; break;
            default: stringColor = "yellow"; break;
        }

        return stringColor;
    }
    
    public static Color ConvertToRealColor(string stringColor)
    {
        Color color = Color.green;
        stringColor = stringColor.ToLower(); 
        switch (stringColor)
        {
            case "black": color = Color.black; break;
            case "red": color = Color.red; break;
            case "green": color = Color.green; break;
            case "blue": color = Color.blue; break;
            default: color = Color.yellow; break;
        }
        return color;
    }
}