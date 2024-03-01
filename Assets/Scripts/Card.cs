using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Card : MonoBehaviour
{
    public int id = 0;
    public bool isHost = true;
    public CardInfo cardInfo;
    public CardDataManager cardDataManager;
    public Card prevCard = null;
    public List<Card> stackedCards = new();

    public CardDataManager.RecipeData combiningRecipe = null;
    
    public bool isDragging = false;
    private BoxCollider2D coll;
    private Rigidbody2D rb;

    public List<GameObject> ListOfOverlapped = new();

    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;

    // Start is called before the first frame update    
    void Start()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isHost && combiningRecipe == null && RecipeExists(id, stackedCards, cardDataManager.recipeDatas.recipeDataArray)) {
            //TODO: Start Combining
        }

        //Reset Collider size and position to prevent hitting own stack
        if (stackedCards.Count == 0) {
            coll.offset = new Vector2(0f, 0f);
            coll.size = new Vector2(1.7f, 3.4f);
        }
        else {
            coll.offset = new Vector2(0f, 1.4f);
            coll.size = new Vector2(1.7f, 0.6f);
        }

        if (isHost) {
            RestackPosition();
        }
    }
    private void OnMouseDown()
    {
        dragStartPos = transform.position;
        dragStartMousePos = GetMousePos();
    }

    private void OnMouseDrag()
    {
        if (!isHost) {
            isHost = true;
            //TODO: Recursively remove previous cards' partial stackedCards
            prevCard.RecursivelyRemoveFromStack(this);

            prevCard = null;
        }
        
        RestackPosition();

        //Make Card isDragging and not interact
        transform.position = dragStartPos + GetMousePos() - dragStartMousePos;
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
        isDragging = true;
        SetWholeStackIsTrigger(true);
    }

    private void OnMouseUp()
    {
        if (isDragging) {
            //TODO: Stack Check, and Stack
            //TODO: If Stack, GhostCard, if not, RigidCard
            if (StackToClosestCollided()) {
                isHost = false;
                GetHost().RestackPosition();
            }
            else {
                isHost = true;
            }

            isDragging = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

            RestackPosition();

            SetWholeStackIsTrigger(false);
        }
    }

    public void SetWholeStackIsTrigger(bool mode)
    {
        coll.isTrigger = mode;

        foreach (Card card in stackedCards) {
            card.coll.isTrigger = mode;
        }
    }

    public void RestackPosition()
    {
        int i = 0;
        foreach(Card stackedCard in stackedCards) {
            i++;
            stackedCard.transform.position = transform.position + Vector3.right * 0.4f * i - Vector3.up * 0.7f * i + Vector3.back * 0.002f * i;
        }
    }

    private bool StackToClosestCollided()
    {
        float distance = 9999f;
        Card closestCard = null;

        foreach (GameObject cardGameObject in ListOfOverlapped) {
            float tempDistance = Vector2.Distance(transform.position, cardGameObject.transform.position);
            if (tempDistance < distance) {
                distance = tempDistance;
                closestCard = cardGameObject.GetComponent<Card>();;
            }
        }
        Card hostCard = null;

        if (closestCard != null) {
            hostCard = closestCard.GetHost();

            //Assign prev card
            if (hostCard.stackedCards.Count > 0)
                prevCard = hostCard.stackedCards[^1];
            else
                prevCard = hostCard;

            //Assign new stacks to each card
            prevCard.RecursivelyAddToStack(this);
        }

        ListOfOverlapped.Clear();

        if (prevCard != null)
            return true;
        return false;
    }

    public void RecursivelyRemoveFromStack(Card removedCard)
    {
        stackedCards.Remove(removedCard);
        foreach (Card removedCard_StackedCard in removedCard.stackedCards) {
            stackedCards.Remove(removedCard_StackedCard);
        }

        prevCard?.RecursivelyRemoveFromStack(removedCard);

        return;
    }

    public void RecursivelyAddToStack(Card addedCard)
    {
        stackedCards.Clear();

        stackedCards.Add(addedCard);
        foreach (Card addedCard_StackedCard in addedCard.stackedCards) {
            stackedCards.Add(addedCard_StackedCard);
        }

        prevCard?.RecursivelyAddToStack(this);

        return;
    }

    public Card GetHost()
    {
        if (prevCard == null)
            return this;
        return prevCard.GetHost();
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //mousePos.z = -5f;
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDragging) {
            //Debug.LogError(other.gameObject);
            Card otherCard = other.transform.GetComponent<Card>();
            if (otherCard != null && !stackedCards.Contains(otherCard)) {
                //Debug.LogError("Overlapping Card!");
                ListOfOverlapped.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.GetComponent<Card>() != null) {
            ListOfOverlapped.Remove(other.gameObject);
        }

    }
}
