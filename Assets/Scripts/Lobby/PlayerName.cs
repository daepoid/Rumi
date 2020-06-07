using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerName : MonoBehaviour
{
    // Start is called before the first frame update
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
