using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using TMPro;

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
    public List<GameObject> ListOfOverlappedFrame = new();

    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;

    [Header("Stats/Progress")]
    public GameObject ProgressBG;
    public SpriteRenderer CardBG;
    public SpriteRenderer CardOuterFrame;
    public SpriteRenderer CardFrame;
    public SpriteRenderer HealthTag;
    public SpriteRenderer PriceTag;
    public TextMeshPro Text_Name;
    public TextMeshPro Text_Price;
    public TextMeshPro Text_Health;

    [Header("Instantiated")]
    public GameObject CardPrefab;

    // Start is called before the first frame update    
    void Start()
    {
        if (id == 37) {
            Destroy(gameObject);
            //ToDo: Open Portal
        }

        combiningRecipe = null;
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);

        if (cardInfo.currentHealth == 0) {
            Text_Health.text = "";
            HealthTag.enabled = false;
        } else {
            Text_Health.text = cardInfo.currentHealth.ToString();
        }
        Text_Name.text = cardInfo.name;
        Text_Price.text = cardInfo.sellPrice.ToString();

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        ResetCard();

        Debug.LogError(stackedCards.Count);
    }

    public void ResetCard()
    {
        isHost = true;
        prevCard = null;
        stackedCards.Clear();
        combiningRecipe = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (isHost && combiningRecipe == null) {
            Debug.Log("CombiningRecipe is null");
            if (RecipeExists(id, stackedCards, cardDataManager.recipeDatas.recipeDataArray)) {
                //TODO: Start Combining
                ProgressBG.SetActive(true);
                StartCoroutine(Craft(combiningRecipe));
            }
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

    private IEnumerator Craft(CardDataManager.RecipeData recipe)
    {
        float timer = 0;
        Vector3 fromPos = new Vector3(-0.45f, 0f, -0.1f);
        Vector3 toPos = new Vector3(0f, 0f, -0.1f);
        Vector3 fromScale = new Vector3(0.01f, 0.8f, 1f);
        Vector3 toScale = new Vector3(0.95f, 0.8f, 0.1f);

        Transform ProgressBarTransform = ProgressBG.transform.GetChild(0);
        while(timer < recipe.time) {
            ProgressBarTransform.localPosition = Vector3.Lerp(fromPos, toPos, timer/recipe.time);
            ProgressBarTransform.localScale = Vector3.Lerp(fromScale, toScale, timer/recipe.time);

            timer += Time.deltaTime;
            yield return null;
        }

        ProgressBG.SetActive(false);
        combiningRecipe = null;

        CreateCard(recipe.GetRandomResultID());
        foreach (Card card in stackedCards) {
            if (recipe.protect.Contains(card.id)) {
                card.ResetCard();
            }
            else {
                Destroy(card.gameObject);
            }
        }

        if (recipe.protect.Contains(id)) {
            ResetCard();
        }
        else {
            Destroy(gameObject);
        }
    }

    private void ResetCombiningState()
    {
        StopAllCoroutines();
        ProgressBG.SetActive(false);
        combiningRecipe = null;
    }

    private void CreateCard(int cardID)
    {
        //TODO: Instantiate card and change ID
        GameObject NewCard = Instantiate(CardPrefab, stackedCards[^1].transform.position + Vector3.down * 0.1f, Quaternion.identity);
        NewCard.GetComponent<Card>().id = cardID;
    }

    private void OnMouseDown()
    {
        transform.position += Vector3.up * 0.15f;
        dragStartPos = transform.position;
        dragStartMousePos = GetMousePos();
    }

    private void OnMouseDrag()
    {
        if (!isHost) {
            isHost = true;

            GetHost().ResetCombiningState();
            
            //Recursively remove previous cards' partial stackedCards

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
        if (ListOfOverlappedFrame.Count > 0) {
            //Sell Or Buy
            if (ListOfOverlappedFrame[0].GetComponent<InteractableFrame>().interactMode == InteractMode.Sell) {
                
            }
            return false;
        }

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

            Debug.LogError("ASDASDAD");
        }

        ListOfOverlapped.Clear();
        ListOfOverlappedFrame.Clear();

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
            if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
                ListOfOverlappedFrame.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.transform.GetComponent<Card>() != null) {
            ListOfOverlapped.Remove(other.gameObject);
        }
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
            ListOfOverlappedFrame.Remove(other.gameObject);
        }
    }
}
