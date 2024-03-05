using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterPack : MonoBehaviour
{
    public int packID = 0;
    public List<Sprite> packSprites;
    public float shakeDuration = 0.3f;

    private BoardManager boardManager;
    private InGameCanvas inGameCanvas;

    public bool isDragging = false;
    private Rigidbody2D rb;
    private Collider2D coll;
    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;
    private float pickUpTime = -10f;
    private int cardAmountID = 0;

    private List<int> packContentsID = new();

    [Header("Instantiated")]
    public GameObject CardPrefab;

    [Header("Shader")]
    [SerializeField] private float _dissolveTime = 0.75f;

    private SpriteRenderer _spriteRenderer;
    private Material _material;

    private int _alphaAmount = Shader.PropertyToID("_AlphaAmount");
    private int _outlineColor = Shader.PropertyToID("_OutlineColor");

    // Start is called before the first frame update
    void Start()
    {
        boardManager = FindObjectOfType<BoardManager>();
        inGameCanvas = FindObjectOfType<InGameCanvas>();
        GetComponent<SpriteRenderer>().sprite = packSprites[packID];
        coll = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        packContentsID = GetPackContent(packID);
        
        StartCoroutine(Shake(shakeDuration));
        
        //Shader part
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.material.mainTexture = _spriteRenderer.sprite.texture;
        _material = _spriteRenderer.material;
    }

    private void VanishEvent()
    {
        StartCoroutine(Vanish());
    }
    
    private IEnumerator Vanish()
    {
        float elapsedTime = 0f;
        _material.SetColor(_outlineColor, Color.white);

        while (elapsedTime < _dissolveTime) {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(1.1f, -0.1f, elapsedTime/_dissolveTime);

            _material.SetFloat(_alphaAmount, lerpedDissolve);

            yield return null;
        }
    }

    private List<int> GetPackContent(int packID)
    {
        List<int> packContents = new();
        if (packID == 0) {
            //5 Cards, 50% blank charm
            
            for (int i=0; i<5; i++) {    
                if (Random.Range(0, 2) == 0)
                    packContents.Add(0);
                else
                    packContents.Add(Random.Range(2, 7));
            }
        }
        else if (packID == 1) {
            //3 Cards, 1-2 Inspirations
            for (int i=0; i<3; i++) {    
                if (Random.Range(0, 3) == 0)
                    packContents.Add(0);
                else
                    packContents.Add(Random.Range(2, 7));
            }

            //Create 1 inspiration
            packContents.Add(-1);

            //67% to create 1 more inspiration
            if (Random.Range(0, 3) != 0)
                packContents.Add(-1);
        }
        else if (packID == 2) {
            //4 Cards, Higher chance of better cards, with chances of summoning enemies

            for (int i=0; i<4; i++) { 
                //20% chance of enemy encounter
                if (Random.Range(0, 5) == 0) {
                    if (boardManager.stage < 2)
                        packContents.Add(15);
                    else {
                        if (Random.Range(0, 5) == 0) {
                            packContents.Add(15);
                        }
                        else {
                            packContents.Add(23);
                        }
                    }
                }
                else {
                    if (Random.Range(0, 5) == 0)
                        packContents.Add(0);
                    else if (Random.Range(0, 3) == 0)
                        packContents.Add(Random.Range(8, 15));
                    else
                        packContents.Add(Random.Range(2, 7));
                }
            }
        }

        return packContents;
    }

    private void OnMouseDown()
    {
        if (cardAmountID < packContentsID.Count) {
            transform.position += Vector3.up * 0.15f;
            dragStartPos = transform.position;
            dragStartMousePos = GetMousePos();

            pickUpTime = Time.time;
        }
    }

    private void OnMouseDrag()
    {
        //Make Gem isDragging and not interact
        if (cardAmountID < packContentsID.Count) {
            transform.position = dragStartPos + GetMousePos() - dragStartMousePos;
            transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
            isDragging = true;

            coll.isTrigger = true;
        }
    }

    private void OnMouseUp()
    {
        if (isDragging) {
            if (cardAmountID < packContentsID.Count) {
                transform.position -= Vector3.up * 0.15f;
                
                isDragging = false;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

                coll.isTrigger = false;

                if (Time.time - pickUpTime < 0.4f && Vector3.Distance(GetMousePos(), dragStartMousePos) < 1.0f) {
                    Debug.Log("Clicked on PACK");

                    OpenPack();

                    CancelInvoke("BackToDynamic");
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    Invoke("BackToDynamic", 0.2f);
                }
            }
        }
    }

    private void OpenPack()
    {
        if (boardManager.objectiveStage == 0) {
            boardManager.ProceedStage(1);
        }

        //Open the pack to create either a card or an inspiration
        StartCoroutine(Shake(shakeDuration));

        CreateCardOrInspiration(packContentsID[cardAmountID], transform.position + Vector3.back * 0.1f, cardAmountID);

        cardAmountID++;
        if (cardAmountID == packContentsID.Count) {
            BoosterDestroy(gameObject);
        }
    }

    private void BoosterDestroy(GameObject gameObject)
    {
        Invoke("VanishEvent", 0.1f);

        //Delayed Destroy
        Destroy(gameObject, shakeDuration + _dissolveTime);
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //mousePos.z = -5f;

        return mousePos;
    }

    private void CreateCardOrInspiration(int cardID, Vector3 createPos, int directionSeed)
    {
        //Create Card
        if (cardID >= 0) {
            Vector3 offset;
            if (directionSeed % 4 == 0) {
                offset = Vector3.right;
            }
            else if (directionSeed % 4 == 1) {
                offset = Vector3.down * 2.5f;
            }
            else if (directionSeed % 4 == 2) {
                offset = Vector3.left;
            }
            else {
                offset = Vector3.up * 2.5f;
            }
            GameObject NewCard = Instantiate(CardPrefab, createPos + offset, Quaternion.identity);

            //Around 12.5% of the time being a phantom
            if (Random.Range(0, 8) == 0) {
                NewCard.GetComponent<Card>().id = 8;
            }
            else {
                NewCard.GetComponent<Card>().id = cardID;
            }
        }
        else {
            //Create Inspiration

            //Fetch Available Undiscovered Inspirations
            List<CardDataManager.RecipeData> AvailableRecipes = new();
            AvailableRecipes.AddRange(boardManager.UndiscoveredRecipes_Stage[0]);

            for (int i=1; i<3; i++) {
                //Only allow higher level recipes if stage reached
                if (boardManager.stage >= i) {
                    AvailableRecipes.AddRange(boardManager.UndiscoveredRecipes_Stage[i]);
                }
            }

            if (AvailableRecipes.Count > 0) {
                CardDataManager.RecipeData randomRecipeData = AvailableRecipes[Random.Range(0, AvailableRecipes.Count)];

                boardManager.DiscoveredRecipes.Add(randomRecipeData);
                for (int i=0; i<3; i++) {
                    boardManager.UndiscoveredRecipes_Stage[i].Remove(randomRecipeData);
                }

                //TODO: Show in UI
                inGameCanvas.AddRecipe(boardManager.DiscoveredRecipes[^1], false);

            } else {
                //TODO: Tell player that all Available Recipes are discovered
                
            }
        }
    }

    private void BackToDynamic()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private IEnumerator Shake(float shakeDuration)
    {
        float timer = 0f;
        
        while (timer < shakeDuration) {
            
            transform.localRotation = Quaternion.Euler(0f, 0f, 5f * Mathf.Sin(timer/shakeDuration * Mathf.PI * 6));

            timer+= Time.deltaTime;
            yield return null;
        }

        transform.localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
