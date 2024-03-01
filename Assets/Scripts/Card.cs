using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Card : MonoBehaviour
{
    public int id = 0;
    public bool isHost = true;
    public CardInfo cardInfo;
    public CardDataManager cardDataManager;
    public Card prevCard = null;
    public List<Card> stackedCards;

    public CardDataManager.RecipeData combiningRecipe = null;
    
    private bool isDragging = false;
    private Collider2D coll;
    private Rigidbody2D rb;


    // Start is called before the first frame update    
    void Start()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHost && combiningRecipe == null && RecipeExists(id, stackedCards, cardDataManager.recipeDatas.recipeDataArray)) {

        }
    }

    private void OnMouseDrag()
    {
        if (!isHost) {
            isHost = true;
            //TODO: Recursively remove previous cards' partial stackedCards
            prevCard = null;
        }

        int i = 0;
        foreach(Card stackedCard in stackedCards) {
            i++;
            stackedCard.transform.position = transform.position + Vector3.right * 0.4f * i - Vector3.up * 0.7f * i + Vector3.back * 0.001f * i;
        }

        transform.position = GetMousePos();
        isDragging = true;
        coll.isTrigger = true;
    }

    private void OnMouseUp()
    {
        if (isDragging) {
            isDragging = false;

            //TODO: Stack Check, and Stack
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

            int i = 0;
            foreach(Card stackedCard in stackedCards) {
                i++;
                stackedCard.transform.position = transform.position + Vector3.right * 0.4f * i - Vector3.up * 0.7f * i + Vector3.back * 0.001f * i;
            }

            coll.isTrigger = false;
        }
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = -5f;
        return mousePos;
    }

    private bool RecipeExists(int id, List<Card> stackedCards, CardDataManager.RecipeData[] recipeDataArray) {
        List<int> ingredientList = new();
        ingredientList.Add(id);
        foreach (Card card in stackedCards) {
            ingredientList.Add(card.id);
        }
        ingredientList.Sort(); //Sort in ascending order

        for (int i=0; i < recipeDataArray.Length; i++) {
            if (ingredientList.SequenceEqual(recipeDataArray[i].ingredients)) {
                combiningRecipe = recipeDataArray[i];
                return true;
            }
        }
        return false;
    }
}
