using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Recipe : MonoBehaviour
{
    public TextMeshProUGUI Text_Result;
    public TextMeshProUGUI Text_Context;
    public Image ResultBox;
    public Image ContextBox;
    public GameObject NewBox;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("ToYellow", 15f);
        Invoke("Seen", 60f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(CardDataManager.RecipeData recipeData, bool made)
    {
        CardDataManager cardDataManager = FindObjectOfType<CardDataManager>();
        Text_Result.text = cardDataManager.cardDatas.cardDataArray[recipeData.drops[0]].name;

        List<int> requiredAuras = new();

        //Ingredients
        foreach(int ingredientID in recipeData.ingredients) {
            if (ingredientID != 7) {
                if (cardDataManager.cardDatas.cardDataArray[ingredientID].type == 4) {
                    requiredAuras.Add(ingredientID);
                }
                else {
                    if (Text_Context.text != "")
                        Text_Context.text += " + ";
                    Text_Context.text += cardDataManager.cardDatas.cardDataArray[ingredientID].name;
                }
            }
        }

        //Required Auras
        if (requiredAuras.Count > 0) {

            for (int i=0; i<requiredAuras.Count; i++) {
                if (i == 0) {
                    Text_Context.text = " >\n" + Text_Context.text;
                }

                Text_Context.text = cardDataManager.cardDatas.cardDataArray[requiredAuras[i]].name + Text_Context.text;

                if (i > 0) {
                    Text_Context.text = ", " + Text_Context.text;
                }
            }

            Text_Context.text = "< " + Text_Context.text;
        }

        switch(cardDataManager.cardDatas.cardDataArray[recipeData.drops[0]].type) {
            case 0:
                Text_Result.color = new Color(1f, 1f, 0.5f, 1f);
                ResultBox.color = new Color(0.33f, 0.33f, 0.3f, 1f);
                ContextBox.color = new Color(0.33f, 0.33f, 0.3f, 1f);
                break;
            case 1:
                Text_Result.color = new Color(0.8f, 0.8f, 1f, 1f);
                ResultBox.color = new Color(0.3f, 0.3f, 0.35f, 1f);
                ContextBox.color = new Color(0.3f, 0.3f, 0.35f, 1f);
                break;
            case 2:
                Text_Result.color = new Color(1f, 0.8f, 0.8f, 1f);
                ResultBox.color = new Color(0.35f, 0.3f, 0.3f, 1f);
                ContextBox.color = new Color(0.35f, 0.3f, 0.3f, 1f);
                break;
            case 3:
                Text_Result.color = new Color(1f, 0.6f, 1f, 1f);
                ResultBox.color = new Color(0.33f, 0.3f, 0.33f, 1f);
                ContextBox.color = new Color(0.33f, 0.3f, 0.33f, 1f);
                break;
            case 4:
                Text_Result.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                ResultBox.color = new Color(0f, 0f, 0f, 1f);
                ContextBox.color = new Color(0f, 0f, 0f, 1f);
                break;
            case 5:
                Text_Result.color = new Color(0.8f, 1f, 0.85f, 1f);
                ResultBox.color = new Color(0.28f, 0.4f, 0.32f, 1f);
                ContextBox.color = new Color(0.28f, 0.4f, 0.32f, 1f);
                break;
            case 6:
                Text_Result.color = new Color(0.8f, 1f, 0.85f, 1f);
                ResultBox.color = new Color(0.28f, 0.4f, 0.32f, 1f);
                ContextBox.color = new Color(0.28f, 0.4f, 0.32f, 1f);
                break;
        }
    }

    private void ToYellow()
    {
        NewBox.GetComponent<Image>().color = Color.yellow;
        NewBox.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
    }

    public void Seen()
    {
        NewBox.SetActive(false);
    }
}
