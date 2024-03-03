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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Setup(CardDataManager.RecipeData recipeData)
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
    }
}
