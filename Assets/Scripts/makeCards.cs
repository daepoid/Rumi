using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeCards : MonoBehaviour
{
    public void CreateNewCards()
    {
        GameObject newCardPrefab = Resources.Load("prefab/Card") as GameObject;

        for (int i = 0; i < 32; i++)
        {
            GameObject newCard = Instantiate(newCardPrefab);
            newCard.name = "Card("  + i.ToString() + ")";
            newCard.transform.SetParent(transform, false);
            // newCard.transform.parent = this.transform;
            newCard.transform.localScale = new Vector3(1, 1, 1);
            newCard.transform.localPosition = new Vector3(30 * i - 465, 0, 0);
        }
    }

    void Start()
    {
        CreateNewCards();
    }

    void Update()
    {
        
    }
}
