using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class RoomName : MonoBehaviour
{
    private Text RoomNameText;

    // Start is called before the first frame update
    void Start()
    {
        RoomNameText = GetComponent<Text>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
