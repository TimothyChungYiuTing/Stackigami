using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class CardInfo {
    public int id;
    public string name;
    public int type;
    public int sellPrice;
    public int sellEffect;
    public GameObject cardObj;
    public int currentHealth;
    public int attack = 0;
    public int attackCD = 1;
    public int attr = 0;

    public List<CardInfo> stackedCards;

    public CardInfo(int id, CardDataManager.CardData[] cardDataArray)
    {
        this.id = id;
        name = cardDataArray[id].name;
        type = cardDataArray[id].type;
        sellPrice = cardDataArray[id].sellPrice;
        sellEffect = cardDataArray[id].sellEffect;
        currentHealth = cardDataArray[id].health;
        attack = cardDataArray[id].attack;
        attackCD = cardDataArray[id].attackCD;
        attr = cardDataArray[id].attr;
    }
}

public class CardDataManager : MonoBehaviour
{
    public TextAsset cardDataJSON;
    public TextAsset recipeDataJSON;

    //Card Data

    [System.Serializable]
    public class CardData {
        public int id = 0;
        public string name;
        public int type = 0; // 0 is Charm Paper, 1 is character, 2 is enemy, 3 is spell, 4 is aura, 5 is riftgate, 6 is portal
        public int sellPrice = 0;
        public int sellEffect = 0; // -1 is not-sellable, 0 is no effect, 1 is spawn random enemy effect
        public int attack = 0;
        public int attackCD = 0;
        public int health = 0;
        public int attr = 0; // Chi, Sui, Ka, Fu, Ku
        public List<int> drops = new(); // Drops after death
    }

    [System.Serializable]
    public class CardDatas
    {
        public CardData[] cardDataArray;
    }
    public CardDatas cardDatas = new();

    //Recipe Data

    [System.Serializable]
    public class RecipeData {
        public List<int> ingredients = new(); // List of ingredients' id
        public int time = 1; // Crafting time in seconds
        public List<int> drops = new(); // List of possible outcomes
        public List<int> chance = new(); // List of chances (x0.8, because 20% is new recipe discovery)
        public int recipeStage = 0; // Which level can this recipe be found at?
        public List<int> protect = new(); // Which cards would remain after recipe finishes?

        public int GetRandomResultID()
        {
            int randID = UnityEngine.Random.Range(0, chance.Sum());
            int sumOfChances = 0;
            int randIDLargerThanDropID = -1;
            for (int i=0; i < chance.Count(); i++) {
                sumOfChances += chance[i];
                if (sumOfChances < randID) {
                    randIDLargerThanDropID = i;
                }
            }
            randIDLargerThanDropID++;
            return drops[randIDLargerThanDropID];
        }
    }

    [System.Serializable]
    public class RecipeDatas
    {
        public RecipeData[] recipeDataArray;
    }
    public RecipeDatas recipeDatas = new();
    void Awake()
    {
        cardDatas = JsonUtility.FromJson<CardDatas>(cardDataJSON.text);
        recipeDatas = JsonUtility.FromJson<RecipeDatas>(recipeDataJSON.text);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
