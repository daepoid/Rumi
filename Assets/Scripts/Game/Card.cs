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
        CardColor = ConvertToCardColor(color);
        RealColor = color;
    }

    public static int ToSymbolNumberFromCardNumber(String number)
    {
        if (number == "J")
        {
            return 14;
        }
        if (number == "" || number == "-1")
        {
            return 15;
        }

        return Int32.Parse(number);
    }
    
    public static String ConvertToCardColor(Color color)
    {
        string stringColor;
        switch (color.ToString())
        {
            case "RGBA(0.000, 0.000, 0.000, 1.000)": stringColor = "black"; break;
            case "RGBA(1.000, 0.000, 0.000, 1.000)": stringColor = "red"; break;
            case "RGBA(0.000, 1.000, 0.000, 1.000)": stringColor = "green"; break;
            case "RGBA(0.000, 0.000, 1.000, 1.000)": stringColor = "blue"; break;
            default : stringColor = "yellow"; break;
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