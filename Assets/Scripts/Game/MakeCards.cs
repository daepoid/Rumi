using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MakeCards : MonoBehaviour
{
    public void CreateNewCards()
    {
        GameObject newCardPrefab = Resources.Load("prefab/Card") as GameObject;

        for (int i = 0; i < MyGameManager.TableCol; i++)
        {
            GameObject newCard = Instantiate(newCardPrefab);
            newCard.name = "Card("  + i.ToString() + ")";
            newCard.transform.SetParent(transform, false);
            newCard.transform.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            newCard.transform.localScale = new Vector3(1, 1, 1);
            newCard.transform.localPosition = new Vector3(30 * i - 465, 0, 0);
            newCard.transform.GetComponent<CardOwnership>().cardOwner = -1;
        }
    }

    void Start()
    {
        CreateNewCards();
    }
}