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
    private CardDataManager cardDataManager;
    public BoardManager boardManager;
    public Card prevCard = null;
    public List<Card> stackedCards = new();

    public CardDataManager.RecipeData combiningRecipe = null;
    
    public bool stackable = true;
    public bool draggable = true;
    public bool isDragging = false;
    private BoxCollider2D coll;
    private Rigidbody2D rb;

    public List<GameObject> ListOfOverlapped = new();
    public List<GameObject> ListOfOverlappedFrame = new();

    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;
    private float lastTimeHealed = 0f;

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
    public SpriteRenderer ContentSpriteRenderer;

    [Header("Instantiated")]
    public GameObject CardPrefab;
    public GameObject GemMoney;

    [Header("Battle")]
    public int battleID = -1; //The ID of the battle they are engaging in

    // Start is called before the first frame update    
    void Start()
    {
        if (id == 37) {
            CardDestroy(this);
            //ToDo: Open Portal
        }

        combiningRecipe = null;
        cardDataManager = FindObjectOfType<CardDataManager>();
        boardManager = FindObjectOfType<BoardManager>();
        
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);
        boardManager.existingCardsList.Add(this);

        if (id == 0) {
            CardBG.color = Color.white;
        } else {
            AssignTypeStyle();
        }

        AssignContentSprite();

        if (cardInfo.maxHealth == 0) {
            Text_Health.text = "";
            HealthTag.enabled = false;
        } else {
            Text_Health.text = cardInfo.currentHealth.ToString();
            HealthTag.enabled = true;
        }
        Text_Name.text = cardInfo.name;

        if (cardInfo.type == 1 || cardInfo.type == 2 || cardInfo.type == 3) {
            PriceTag.enabled = true;
            //if (cardInfo.attack > 0) {
                Text_Price.text = cardInfo.attack.ToString();
            //}
        }
        else {
            PriceTag.enabled = true;
            Text_Price.text = cardInfo.sellPrice.ToString();
        }

        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();

        ResetCard();

        //Debug.LogError(stackedCards.Count);
    }

    private void Die(Card card)
    {
        //Drops
        if (card.cardInfo.drops != null && card.cardInfo.drops.Count > 0) {
            int randDropID = card.cardInfo.drops[Random.Range(0, card.cardInfo.drops.Count)];
            Debug.Log(randDropID);
            CreateCard(randDropID);
        }

        //Check if all conjurators dead
        if (id == 7) {
            int conjuratorNum = 0;
            foreach(Card existingCard in boardManager.existingCardsList) {
                if (existingCard.cardInfo.id == 7)
                    conjuratorNum++;
            }
            if (conjuratorNum == 1) {
                //TODO: Lose Condition, Restart
            }
        }

        CardDestroy(card);
    }

    private void CardDestroy(Card card)
    {
        boardManager.existingCardsList.Remove(card);
        card.coll.enabled = false;
        //TODO: Disappear sequence
        //Delaye Destroy
        Destroy(card.gameObject);
    }

    private void AssignTypeStyle()
    {
        switch (cardInfo.type) {
            case 0:
                if (cardInfo.id <= 6) {
                    CardBG.color = new Color(1f, 1f, 0.55f, 1f);
                }
                else {
                    CardBG.color = new Color(1f, 0.85f, 0.65f, 1f);
                }
                draggable = true;
                stackable = true;
                break;
            case 1:
                CardBG.color = new Color(0.8f, 0.95f, 1f, 0.6f);
                draggable = true;
                stackable = true;
                break;
            case 2:
                CardBG.color = new Color(1f, 0.75f, 0.9f, 0.6f);
                draggable = false;
                stackable = false;
                InvokeRepeating("ChasePlayers", 1.5f, cardInfo.attackCD * 2f);
                break;
            case 3:
                CardBG.color = new Color(1f, 0.7f, 0.95f, 1f);
                draggable = true;
                stackable = true;
                break;
            case 4:
                CardBG.color = new Color(0f, 0f, 0f, 1f);
                draggable = true;
                stackable = true;
                break;
            case 5:
                CardBG.color = new Color(0.8f, 1f, 0.95f, 1f);
                draggable = true;
                stackable = true;
                break;
            case 6:
                draggable = false;
                stackable = false;
                break;
        }
    }
    private void AssignContentSprite()
    {
        Sprite sprite = boardManager.cardSpritesScript.cardSprites[id];

        if (sprite != null) {
            ContentSpriteRenderer.enabled = true;
            ContentSpriteRenderer.sprite = sprite;
        } else {
            ContentSpriteRenderer.enabled = false;
        }
    }

    private void ChasePlayers()
    {
        //TODO: Map players and chase closest one
        if (battleID == -1) {
            StartCoroutine(ChaseMotion());
        }
    }

    private IEnumerator ChaseMotion()
    {
        float timer = 0f;
        float t;

        Card closestAttackableCard = null;
        float distance = 9999f;

        foreach (Card existingCard in boardManager.existingCardsList) {
            if (existingCard.cardInfo.type == 1) {
                float tempDistance = Vector2.Distance(transform.position, existingCard.transform.position);
                if (tempDistance < distance) {
                    distance = tempDistance;
                    closestAttackableCard = existingCard;
                }
            }
        }

        if (closestAttackableCard == null)
            yield break;

        while (timer < 0.2f) {
            t = Mathf.SmoothStep(0f, 1f, timer);
            t = Mathf.SmoothStep(0f, 1f, t);
            //Get closer to player with smoothlerp
            if (battleID == -1 && closestAttackableCard != null)
            transform.position += (Vector3)((Vector2)(closestAttackableCard.transform.position - transform.position)).normalized * Time.deltaTime * 8f;
            
            timer += Time.deltaTime;
            yield return null;
        }
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
            //Debug.Log("CombiningRecipe is null");
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

        if (cardInfo.type == 1 || cardInfo.type == 2) {
            Text_Health.text = cardInfo.currentHealth.ToString();
            

            if (coll.enabled && cardInfo.currentHealth <= 0) {
                Die(this);
            }
        }
        

        if (cardInfo.type == 1 && battleID == -1) {
            if (Time.time - lastTimeHealed > 10f) {
                if (cardInfo.currentHealth < cardInfo.maxHealth) {
                    cardInfo.currentHealth++;
                    lastTimeHealed = Time.time;
                }
            }
        }

        //Debug only
        if (Input.GetKeyDown(KeyCode.H)) {
            cardInfo.currentHealth = 0;
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
                CardDestroy(card);
            }
        }

        if (recipe.protect.Contains(id)) {
            ResetCard();
        }
        else {
            CardDestroy(this);
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
        GameObject NewCard;
        if (stackedCards.Count > 0)
            NewCard = Instantiate(CardPrefab, stackedCards[^1].transform.position + Vector3.down * 0.1f, Quaternion.identity);
        else
            NewCard = Instantiate(CardPrefab, transform.position + Vector3.down * 0.1f, Quaternion.identity);
        NewCard.GetComponent<Card>().id = cardID;
    }

    private void OnMouseDown()
    {
        if (draggable) {
            transform.position += Vector3.up * 0.15f;
            dragStartPos = transform.position;
            dragStartMousePos = GetMousePos();
        }
    }

    private void OnMouseDrag()
    {
        if (draggable) {
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
    }

    private void OnMouseUp()
    {
        if (draggable) {
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
            stackedCard.transform.position = transform.position + Vector3.right * 0.4f * i - Vector3.up * 0.7f * i + Vector3.back * 0.004f * i;
        }
    }

    private bool StackToClosestCollided()
    {
        if (ListOfOverlappedFrame.Count > 0) {
            //Sell Or Buy
            if (ListOfOverlappedFrame[0].GetComponent<InteractableFrame>().interactMode == InteractMode.Sell) {
                int sumOfSold = 0;
                List<Card> ToBeSoldList = new();

                if (cardInfo.sellEffect != -1) {
                    sumOfSold += cardInfo.sellPrice;
                }

                foreach(Card stackedCard in stackedCards) {
                    if (stackedCard.cardInfo.sellEffect != -1) {
                        sumOfSold += stackedCard.cardInfo.sellPrice;
                        ToBeSoldList.Add(stackedCard);
                    }
                }

                //boardManager.gem.Text_Amount.text = (int.Parse(boardManager.gem.Text_Amount.text) + sumOfSold).ToString();
                foreach(Card stackedCard in stackedCards) {
                    stackedCard.ResetCard();
                }
                ResetCard();

                foreach(Card ToBeSold in ToBeSoldList) {
                    InstantiateMoney(ToBeSold);
                    CardDestroy(ToBeSold);
                }

                if (cardInfo.sellEffect != -1) {
                    InstantiateMoney(this);
                    CardDestroy(this);
                }
            }
            return false;
        }

        //Stack

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

            hostCard.ResetCombiningState();
        }

        ListOfOverlapped.Clear();
        ListOfOverlappedFrame.Clear();

        if (prevCard != null)
            return true;
        return false;
    }

    private void InstantiateMoney(Card toBeSold)
    {
        for (int i=0; i<toBeSold.cardInfo.sellPrice; i++) {
            Instantiate(GemMoney, toBeSold.transform.position + Random.Range(-0.1f, 0.1f) * Vector3.up + Random.Range(-0.1f, 0.1f) * Vector3.right, Quaternion.identity);
        }
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
            if (otherCard != null && otherCard.stackable && !stackedCards.Contains(otherCard)) {
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
