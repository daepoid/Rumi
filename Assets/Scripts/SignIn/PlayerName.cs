using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    private Text nameText;
    void Start()
    {
        nameText = GetComponent<Text>();
        if (AuthManager.User != null)
        {
            nameText.text = $"{AuthManager.User.Email}";
        }
        else
        {
            nameText.text = "ERROR";
        }
    }
}
