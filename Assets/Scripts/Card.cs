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

    [HideInInspector] public bool removed = false;

    public CardDataManager.RecipeData combiningRecipe = null;
    
    public bool stackable = true;
    public bool draggable = true;
    public bool isDragging = false;
    [HideInInspector] public BoxCollider2D coll;
    [HideInInspector] public Rigidbody2D rb;

    public List<GameObject> ListOfOverlapped = new();
    public List<GameObject> ListOfOverlappedEnemies = new();
    public List<GameObject> ListOfOverlappedFrame = new();

    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;
    [HideInInspector] public float lastTimeHealed = 0f;

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
    public SpriteRenderer CharacterSpriteRenderer;
    public SpriteRenderer ContentSpriteRenderer;

    [Header("Instantiated")]
    public GameObject CardPrefab;
    public GameObject GemMoney;
    public GameObject DamagePrefab;

    [Header("Battle")]
    public int battleID = -1; //The ID of the battle they are engaging in
    private float nextAttackDelay = 0;

    public SpriteRenderer HurtFilter;
    public SpriteRenderer WitherFilter;
    public SpriteRenderer FrozenFilter;
    public SpriteRenderer ShredFilter;

    public bool withering = false;
    public bool frozen = false;

    [Header("Rift")]
    [HideInInspector] public bool inRift = false;
    private int riftPos = -1;

    [Header("CraftedTimes")]
    private int craftedTimes = 0;

    [Header("Shader")]
    [SerializeField] private float _dissolveTime = 0.75f;

    private List<SpriteRenderer> _spriteRenderers;
    private List<Material> _materials;

    private int _alphaAmount = Shader.PropertyToID("_AlphaAmount");
    private int _outlineColor = Shader.PropertyToID("_OutlineColor");

    [Header("Coroutine Management")]
    public Coroutine lastCoroutine = null;

    [Header("Audio")]
    public List<AudioClip> audioClips;
    private AudioSource audioSource;

    [Header("Float")]
    public bool playFlipAudioOnStart = true;
    public bool floatingRandom = false;
    private Vector3 randomFloatDir;
    private int randomRotateDir = 1;
    private float randomDistance = 0.5f;

    // Start is called before the first frame update    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        
        if (playFlipAudioOnStart) {
            audioSource.pitch = 1f;
            audioSource.clip = audioClips[1];
            audioSource.Play();
        }

        if (floatingRandom) {
            id = Random.Range(2, 36);
            randomFloatDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            coll.enabled = false;
            randomRotateDir = (Random.Range(0, 2) == 0)?1:-1;
            randomDistance = Random.Range(0.1f, 0.7f);
            transform.localScale = Vector3.one * randomDistance;
        }

        combiningRecipe = null;
        cardDataManager = FindObjectOfType<CardDataManager>();
        boardManager = FindObjectOfType<BoardManager>();
        
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);
        boardManager.existingCardsList.Add(this);

        if (id == 37) {
            //Open Portal
            boardManager.riftFrame.UnlockRift();
            
            boardManager.existingCardsList.Remove(this);
            coll.enabled = false;
            Destroy(gameObject);
        }
        else {
            //Stage management
            if (!floatingRandom) {
                if (id == 15 && !boardManager.oniDiscovered) {
                    boardManager.oniDiscovered = true;
                    boardManager.curseFrame.SetActive(true);
                    boardManager.ProceedStage(4);
                } else if (id == 18 && boardManager.stage == 0) {
                    AudioManager.Instance.ChangeSong(1);
                    boardManager.stage++;
                    boardManager.ProceedStage(6);
                } else if (id == 28 && boardManager.stage == 1) {
                    boardManager.stage++;
                    boardManager.ProceedStage(10);
                } else if (id == 7 && boardManager.objectiveStage == 3) {
                    boardManager.ProceedStage(4);
                } else if (id == 16 && boardManager.objectiveStage == 4) {
                    boardManager.ProceedStage(5);
                } else if (cardInfo.type == 1 && id != 7 && id != 17 && boardManager.objectiveStage == 6) {
                    boardManager.ProceedStage(7);
                } else if (cardInfo.type == 3 && boardManager.objectiveStage == 7) {
                    boardManager.ProceedStage(8);
                } else if (id == 24 && boardManager.objectiveStage == 8) {
                    boardManager.ProceedStage(9);
                } else if (id == 36 && boardManager.objectiveStage == 10) {
                    boardManager.ProceedStage(11);
                }
            }            

            //Assign Color & Style
            AssignTypeStyle();
        }
        
        //Assign Sprites
        AssignContentSprite_And_BGSprites();

        //Assign Text & Tag Stats
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


        //Reset attacks and battle effects
        nextAttackDelay = cardInfo.attackCD;
        withering = false;
        frozen = false;
        ResetFilter(HurtFilter);
        ResetFilter(WitherFilter);
        ResetFilter(FrozenFilter);
        ResetFilter(ShredFilter);

        ListOfOverlapped.Clear();
        ListOfOverlappedEnemies.Clear();
        ListOfOverlappedFrame.Clear();
        
        //Shader part
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>().ToList();
        _spriteRenderers.Add(GetComponent<SpriteRenderer>());

        _materials = new();
        for (int i=0; i < _spriteRenderers.Count; i++) {
            _spriteRenderers[i].material.mainTexture = _spriteRenderers[i].sprite.texture;
            _materials.Add(_spriteRenderers[i].material);
        }

        StartCoroutine(Appear());

        //Ensure card is solo card (unattached)
        ResetCard();

        //If new discovery, add to recipe list
        for (int i=0; i<3; i++) {
            foreach (CardDataManager.RecipeData recipeData in boardManager.UndiscoveredRecipes_Stage[i]) {
                if (recipeData.drops[0] == id) {
                    boardManager.DiscoveredRecipes.Add(recipeData);
                    boardManager.UndiscoveredRecipes_Stage[i].Remove(recipeData);
                    FindObjectOfType<InGameCanvas>()?.AddRecipe(recipeData, true);
                    break;
                }
            }
        }
    }

    private IEnumerator Appear()
    {
        float elapsedTime = 0f;
        for (int i = 0; i < _materials.Count; i++) {
            _materials[i].SetColor(_outlineColor, GetColor(cardInfo.type));
        }

        while (elapsedTime < _dissolveTime) {
            elapsedTime += Time.deltaTime;

            Text_Name.color = Color.Lerp(new Color(Text_Name.color.r, Text_Name.color.g, Text_Name.color.b, 0f), new Color(Text_Name.color.r, Text_Name.color.g, Text_Name.color.b, 1f), elapsedTime/_dissolveTime);
            Text_Health.color = Color.Lerp(new Color(Text_Health.color.r, Text_Health.color.g, Text_Health.color.b, 0f), new Color(Text_Health.color.r, Text_Health.color.g, Text_Health.color.b, 1f), elapsedTime/_dissolveTime);
            Text_Price.color = Color.Lerp(new Color(Text_Price.color.r, Text_Price.color.g, Text_Price.color.b, 0f), new Color(Text_Price.color.r, Text_Price.color.g, Text_Price.color.b, 1f), elapsedTime/_dissolveTime);

            float lerpedDissolve = Mathf.Lerp(-0.1f, 1.1f, elapsedTime/_dissolveTime);

            for (int i = 0; i < _materials.Count; i++) {
                _materials[i].SetFloat(_alphaAmount, lerpedDissolve);
            }

            yield return null;
        }
    }

    private IEnumerator Vanish()
    {
        float elapsedTime = 0f;
        for (int i = 0; i < _materials.Count; i++) {
            _materials[i].SetColor(_outlineColor, GetColor(cardInfo.type));
        }

        while (elapsedTime < _dissolveTime) {
            elapsedTime += Time.deltaTime;

            Text_Name.color = Color.Lerp(new Color(Text_Name.color.r, Text_Name.color.g, Text_Name.color.b, 1f), new Color(Text_Name.color.r, Text_Name.color.g, Text_Name.color.b, 0f), elapsedTime/_dissolveTime);
            Text_Health.color = Color.Lerp(new Color(Text_Health.color.r, Text_Health.color.g, Text_Health.color.b, 1f), new Color(Text_Health.color.r, Text_Health.color.g, Text_Health.color.b, 0f), elapsedTime/_dissolveTime);
            Text_Price.color = Color.Lerp(new Color(Text_Price.color.r, Text_Price.color.g, Text_Price.color.b, 1f), new Color(Text_Price.color.r, Text_Price.color.g, Text_Price.color.b, 0f), elapsedTime/_dissolveTime);

            float lerpedDissolve = Mathf.Lerp(1.1f, -0.1f, elapsedTime/_dissolveTime);

            for (int i = 0; i < _materials.Count; i++) {
                _materials[i].SetFloat(_alphaAmount, lerpedDissolve);
            }

            yield return null;
        }
    }

    private Color GetColor(int type) {
        switch (type) {
            case 0:
                return Color.yellow;
            case 1:
                return Color.cyan;
            case 2:
                return Color.red;
            case 3:
                return Color.magenta;
            case 4:
                return Color.white;
            case 5:
                return Color.green;
            case 6:
                return Color.green;
            default:
                return Color.cyan;
        }
    }

    private void ResetFilter(SpriteRenderer sr)
    {
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
    }

    private void Die(Card card)
    {
        audioSource.pitch = 1.0f;
        audioSource.clip = audioClips[3];
        audioSource.Play();

        //Objectives
        if (id == 36 && boardManager.objectiveStage == 11) {
            boardManager.ProceedStage(12);
        }

        //Remove this from other cards' lists
        foreach (Card existingCard in boardManager.existingCardsList) {
            existingCard.ListOfOverlapped.Remove(card.gameObject);
            existingCard.ListOfOverlappedEnemies.Remove(card.gameObject);
        }

        //Remove from any participating battles
        if (battleID != -1) {
            boardManager.EndBattle(battleID);
        }

        //Drops
        if (card.cardInfo.drops != null && card.cardInfo.drops.Count > 0) {
            foreach(int id in card.cardInfo.drops)
                CreateCard(id);
        }

        //Check if all conjurators dead
        if (id == 7) {
            int conjuratorNum = 0;
            foreach(Card existingCard in boardManager.existingCardsList) {
                if (existingCard.cardInfo.id == 7)
                    conjuratorNum++;
            }
            if (conjuratorNum == 1) {
                //TODO: Lose Condition
                boardManager.inGameCanvas.Lost.SetActive(true);
                boardManager.DisableColliders();
                boardManager.gem.GetComponent<Collider2D>().enabled = false;
                
                Time.timeScale = 0.3f;
                AudioManager.Instance.spatialBlend = 0.7f;
                
                boardManager.lost = true;
            }
        }

        if (id == 36) {
            boardManager.AshiyaDouman_Defeated = true;
        }

        card.CardDestroy();
    }

    public void CardDestroy()
    {
        boardManager.existingCardsList.Remove(this);
        coll.enabled = false;

        //Disappear sequence
        removed = true;
        StopAllCoroutines();
        CancelInvoke();
        StartCoroutine(Vanish());

        //Delayed Destroy
        Destroy(gameObject, _dissolveTime);
    }

    private void AssignTypeStyle()
    {
        //Assign different styles depending on Type
        switch (cardInfo.type) {
            case 0: //Normal
                if (cardInfo.id == 0) {
                    CardBG.color = Color.white;
                }
                else if (cardInfo.id <= 6) {
                    CardBG.color = new Color(1f, 1f, 0.55f, 1f);
                }
                else {
                    CardBG.color = new Color(1f, 0.85f, 0.65f, 1f);
                }
                ChangeDrawnColor(new Color(0.77f, 0.22f, 0.07f, 0.9f), Color.white, new Color(0.32f, 0f, 0f, 1f));
                draggable = true;
                stackable = true;
                CardFrame.enabled = true;
                CharacterSpriteRenderer.enabled = false;
                PositionYValues(-0.1f);
                break;
            case 1: //Ally
                if (cardInfo.id == 7) {
                    CardBG.color = new Color(0.54f, 0.46f, 0.12f, 0.8f);
                    ChangeDrawnColor(new Color(1f, 0.97f, 0.87f, 0.9f), Color.black, Color.white);
                    stackable = true;
                }
                else if (cardInfo.id == 17) {
                    CardBG.color = new Color(0.8f, 0.95f, 1f, 0.7f);
                    ChangeDrawnColor(new Color(0.1f, 0.2f, 0.8f, 0.9f), Color.white, Color.black);
                    stackable = true;
                }
                else {
                    CardBG.color = new Color(0.66f, 0.75f, 0.92f, 0.7f);
                    ChangeDrawnColor(new Color(0.1f, 0.2f, 0.8f, 0.9f), Color.white, Color.black);
                    stackable = false;
                }
                draggable = true;
                CardFrame.enabled = false;
                CharacterSpriteRenderer.enabled = true;
                AssignCharacterSprite();
                PositionYValues(-0.27f);
                break;
            case 2: //Enemy
                if (cardInfo.id == 36) {
                    CardBG.color = new Color(0f, 0f, 0f, 0.9f);
                    ChangeDrawnColor(new Color(1f, 0.3f, 0.4f, 0.9f), Color.white, Color.gray);
                } else {
                    CardBG.color = new Color(1f, 0.75f, 0.9f, 0.9f);
                    ChangeDrawnColor(new Color(0.8f, 0.2f, 0.3f, 0.9f), Color.white, Color.black);
                }
                draggable = false;
                stackable = false;
                CardFrame.enabled = false;
                CharacterSpriteRenderer.enabled = true;
                AssignCharacterSprite();
                PositionYValues(-0.27f);

                if (!floatingRandom)
                    InvokeRepeating("ChasePlayers", 1.5f, cardInfo.attackCD / cardInfo.attack * 0.7f + 0.7f);
                break;
            case 3: //Spell
                CardBG.color = new Color(0.82f, 0.54f, 0.96f, 1f);
                ChangeDrawnColor(new Color(1f, 0.87f, 1f, 0.9f), Color.black, Color.black);
                draggable = true;
                stackable = true;
                CardFrame.enabled = true;
                CharacterSpriteRenderer.enabled = false;
                PositionYValues(-0.1f);
                break;
            case 4: //Aura
                CardBG.color = new Color(0f, 0f, 0f, 1f);
                ChangeDrawnColor(new Color(1f, 0.8f, 0.8f, 0.9f), Color.black, Color.white);
                draggable = true;
                stackable = true;
                CardFrame.enabled = true;
                CharacterSpriteRenderer.enabled = false;
                PositionYValues(-0.1f);
                break;
            case 5: //Rift
                CardBG.color = new Color(0.66f, 92f, 0.77f, 1f);
                ChangeDrawnColor(new Color(0.05f, 0.45f, 0.21f, 0.9f), Color.white, Color.black);
                draggable = true;
                stackable = true;
                CardFrame.enabled = true;
                CharacterSpriteRenderer.enabled = false;
                PositionYValues(-0.1f);
                break;
            case 6: //Unused
                CardBG.color = new Color(1f, 1f, 0.55f, 1f);
                ChangeDrawnColor(new Color(0.77f, 0.22f, 0.07f, 0.9f), Color.white, new Color(0.32f, 0f, 0f, 1f));
                draggable = false;
                stackable = false;
                CardFrame.enabled = false;
                CharacterSpriteRenderer.enabled = false;
                PositionYValues(-0.1f);
                break;
        }
    }

    private void ChangeDrawnColor(Color colorWeak, Color colorStats, Color colorStrong)
    {
        //Color cards with 3 colors
        Text_Name.color = colorStrong;
        CardFrame.color = colorWeak;
        CardOuterFrame.color = colorWeak;
        PriceTag.color = colorWeak;
        HealthTag.color = colorWeak;
        Text_Price.color = colorStats;
        Text_Health.color = colorStats;
        if (colorStats == Color.black) {
            Text_Price.fontSize = 2.8f;
            Text_Health.fontSize = 2.8f;
        } else {
            Text_Price.fontSize = 2.4f;
            Text_Health.fontSize = 2.4f;
        }

        if (cardInfo.type == 1 || cardInfo.type == 2)
            ContentSpriteRenderer.color = Color.white;
        else
            ContentSpriteRenderer.color = colorStrong;
    }

    private void PositionYValues(float y)
    {
        //Reposition y localPosition
        ContentSpriteRenderer.transform.localPosition = new Vector3(ContentSpriteRenderer.transform.localPosition.x, y, ContentSpriteRenderer.transform.localPosition.z);
        CharacterSpriteRenderer.transform.localPosition = new Vector3(CharacterSpriteRenderer.transform.localPosition.x, y, CharacterSpriteRenderer.transform.localPosition.z);
    }

    private void AssignContentSprite_And_BGSprites()
    {
        //Content Sprite
        Sprite sprite = boardManager.cardSpritesScript.cardSprites[id];

        if (sprite != null) {
            ContentSpriteRenderer.enabled = true;
            ContentSpriteRenderer.sprite = sprite;
        } else {
            ContentSpriteRenderer.enabled = false;
        }

        //BG Sprite
        if (cardInfo.type == 1 || cardInfo.type == 2)
            sprite = boardManager.cardSpritesScript.bgSprites[1];
        else
            sprite = boardManager.cardSpritesScript.bgSprites[0];

        if (sprite != null) {
            CardBG.enabled = true;
            CardBG.sprite = sprite;
        } else {
            CardBG.enabled = false;
        }

        //Outer Frame Sprite
        if (cardInfo.type == 1 || cardInfo.type == 2)
            sprite = boardManager.cardSpritesScript.outerFrameSprites[1];
        else
            sprite = boardManager.cardSpritesScript.outerFrameSprites[0];

        if (sprite != null) {
            CardOuterFrame.enabled = true;
            CardOuterFrame.sprite = sprite;
        } else {
            CardOuterFrame.enabled = false;
        }
    }
    private void AssignCharacterSprite()
    {
        Sprite sprite;
        if (cardInfo.id == 17) //Puppet
            sprite = boardManager.cardSpritesScript.characterSprites[0]; //Puppet without memo
        else if (cardInfo.type == 1) //Puppet with memo - Ally
            sprite = boardManager.cardSpritesScript.characterSprites[1]; //Puppet with memo
        else //Enemy
            sprite = boardManager.cardSpritesScript.characterSprites[2]; //Black Puppet
        
        if (sprite != null) {
            CharacterSpriteRenderer.enabled = true;
            CharacterSpriteRenderer.sprite = sprite;
        } else {
            CharacterSpriteRenderer.enabled = false;
        }
    }

    private void ChasePlayers()
    {
        //Map players and chase closest one
        if (battleID == -1 && !frozen) {
            StartCoroutine(ChaseMotion());
        }
    }

    private IEnumerator ChaseMotion()
    {
        //Motion for 1 Dash Action
        float timer = 0f;
        float t;

        //Get closest Attackable Ally as Target
        Card closestAttackableCard = null;
        float distance = 9999f;

        foreach (Card existingCard in boardManager.existingCardsList) {
            if (existingCard.cardInfo.type == 1 && existingCard.battleID == -1) {
                //Target must not be in battle
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
            transform.position += (Vector3)((Vector2)(closestAttackableCard.transform.position - transform.position)).normalized * Time.deltaTime * 9f;
            
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void RepeatAttack()
    {
        //Attack opponent player repeatedly
        InvokeRepeating("Attack", nextAttackDelay, cardInfo.attackCD);
    }

    private void Attack()
    {
        lastCoroutine = StartCoroutine(Hit(boardManager.GetOpponentCard(this, battleID), cardInfo.attackCD));
    }

    private IEnumerator Hit(Card opponentCard, float attackCD)
    {
        nextAttackDelay = attackCD;

        float timer = 0f;
        float t;

        int opponentDirMult = (opponentCard.transform.position.x > transform.position.x)?1:-1;
        float originalXPos = transform.position.x;
        float originalYPos = transform.position.y;
        float originalZPos = transform.position.z;

        while (timer < 0.3f) {
            nextAttackDelay -= Time.deltaTime;

            t = Mathf.SmoothStep(0f, 1f, timer);
            t = Mathf.SmoothStep(0f, 1f, t);
            
            //Animation
            if (!frozen) {
                transform.position = new Vector3(originalXPos + Mathf.Sin(t * Mathf.PI) * 4f * opponentDirMult, originalYPos + Mathf.Sin(t * Mathf.PI) * 0.8f, originalZPos - Mathf.Sin(t * Mathf.PI) * 0.2f);
                opponentCard.transform.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.3f);
                opponentCard.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * Mathf.PI) * 60f * opponentDirMult);
                
                if (timer > 0.15f)
                    opponentCard.HurtFilter.color = new Color(opponentCard.HurtFilter.color.r, opponentCard.HurtFilter.color.g, opponentCard.HurtFilter.color.b, 0.6f);
                
            }
            timer += Time.deltaTime;
            yield return null;
        }
        //Reset position and scales
        transform.position = new Vector3(originalXPos, originalYPos, originalZPos);
        opponentCard.transform.localScale = Vector3.one;
        opponentCard.transform.localRotation = Quaternion.identity;
        opponentCard.HurtFilter.color = new Color(opponentCard.HurtFilter.color.r, opponentCard.HurtFilter.color.g, opponentCard.HurtFilter.color.b, 0f);

        //Deal damage, Instantiate DamagePrefab
        if (!frozen) {
            if (cardInfo.type == 1)
                audioSource.pitch = 1.0f;
            else if (cardInfo.type == 2)
                audioSource.pitch = 0.9f;
            audioSource.clip = audioClips[2];
            audioSource.Play();

            opponentCard.cardInfo.currentHealth -= cardInfo.attack;
            GameObject damageVisual = Instantiate(DamagePrefab, opponentCard.transform.position + Vector3.back * 0.4f + Vector3.right * Random.Range(-0.6f, 0.6f) + Vector3.up * Random.Range(-1.2f, 1.2f), Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)));
            damageVisual.GetComponent<Damage>().side = cardInfo.type;
            damageVisual.GetComponent<Damage>().damage = cardInfo.attack;
        }
        
        //Keep lowering nextAttackDelay
        timer = 0f;

        while (timer < attackCD - 0.35f) {
            nextAttackDelay -= Time.deltaTime;

            timer += Time.deltaTime;
            yield return null;
        }

        nextAttackDelay = 0f;
    }


    public void ResetCard()
    {
        //Reset card status to a solo card
        isHost = true;
        prevCard = null;
        stackedCards.Clear();
        combiningRecipe = null;
        battleID = -1;
        craftedTimes = 0;
        
        ResetCombiningState();
    }

    // Update is called once per frame
    void Update()
    {
        if (!removed) {

            if (isHost && combiningRecipe == null) {
                if (RecipeExists(id, stackedCards, cardDataManager.recipeDatas.recipeDataArray)) {
                    //Start Combining
                    ProgressBG.SetActive(true);
                    lastCoroutine = StartCoroutine(Craft(combiningRecipe));
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

            //Keep updating stack's transform.position
            if (isHost) {
                RestackPosition();
            }

            //Keep updating currentHealth info
            if (cardInfo.type == 1 || cardInfo.type == 2) {
                Text_Health.text = cardInfo.currentHealth.ToString();
                

                if (coll.enabled && cardInfo.currentHealth <= 0) {
                    Die(this);
                }
            }
            
            //Heal every 10 sec (If not in battle)
            if (cardInfo.type == 1 && battleID == -1) {
                if (Time.time - lastTimeHealed > 10f) {
                    if (cardInfo.currentHealth < cardInfo.maxHealth) {
                        cardInfo.currentHealth++;
                        lastTimeHealed = Time.time;
                    }
                }
            }

            //Make card transform normal if not in battle
            if (battleID == -1 && !floatingRandom) {
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.identity;
                HurtFilter.color = new Color(HurtFilter.color.r, HurtFilter.color.g, HurtFilter.color.b, 0f);
            }

            //Warning: Debug Only Section
            if (Input.GetKeyDown(KeyCode.H)) {
                cardInfo.currentHealth = 0;
            }
            if (cardInfo.type == 1 && Input.GetKeyDown(KeyCode.G)) {
                cardInfo.currentHealth = 0;
            }
            if (cardInfo.type == 2 && Input.GetKeyDown(KeyCode.J)) {
                cardInfo.currentHealth = 0;
            }

        }

        if (floatingRandom) {
            transform.position += randomFloatDir * 5f * Time.deltaTime * randomDistance;
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + 40f * Time.deltaTime * randomRotateDir);
        }
    }

    private IEnumerator Craft(CardDataManager.RecipeData recipe)
    {
        //Craft recipe action
        float timer = 0;
        Vector3 fromPos = new Vector3(-0.45f, 0f, -0.1f);
        Vector3 toPos = new Vector3(0f, 0f, -0.1f);
        Vector3 fromScale = new Vector3(0.01f, 0.8f, 1f);
        Vector3 toScale = new Vector3(0.95f, 0.8f, 0.1f);

        Transform ProgressBarTransform = ProgressBG.transform.GetChild(0);
        while(timer < recipe.time) {
            //Progress bar visualization
            ProgressBarTransform.localPosition = Vector3.Lerp(fromPos, toPos, timer/recipe.time);
            ProgressBarTransform.localScale = Vector3.Lerp(fromScale, toScale, timer/recipe.time);

            timer += Time.deltaTime;
            yield return null;
        }

        //Recipe Complete
        ProgressBG.SetActive(false);
        combiningRecipe = null;

        //Create Card
        CreateCard(recipe.GetRandomResultID());

        //Update Objectives
        if (boardManager.objectiveStage == 1) {
            boardManager.ProceedStage(2);
        }

        //Destroy Ingredients
        bool ingredientsDestroyed = false;
        foreach (Card card in stackedCards) {
            if (recipe.protect.Contains(card.id))
            {
                if (!recipe.ingredients.Contains(0)) {
                    card.ResetCard();
                }
            }
            else {
                //Make blank charms destroy after 2 usages instead
                if (card.id == 0 && card.craftedTimes == 0) {
                    card.craftedTimes++;
                }
                else {
                    card.CardDestroy();
                    ingredientsDestroyed = true;
                }
            }
        }

        if (recipe.protect.Contains(id)) {
            if (!recipe.ingredients.Contains(0)) {
                ResetCard();
            }
        }
        else {
            //Make blank charms destroy after 2 usages instead
            if (id == 0 && craftedTimes == 0) {
                craftedTimes++;
            }
            else {
                CardDestroy();
                ingredientsDestroyed = true;
            }
        }

        if (ingredientsDestroyed) {
            foreach (Card stackedCard in stackedCards) {
                stackedCard?.ResetCard();
            }
            ResetCard();
        }

        combiningRecipe = null;
    }

    private bool AllCraftsDone(int craftsNum)
    {
        //Checks if all crafts of Blank Charm are done
        bool AllCraftsFinished = false;
        foreach (Card stackedCard in stackedCards)
        {
            if (stackedCard.id == 0 && stackedCard.craftedTimes >= craftsNum)
            {
                AllCraftsFinished = true;
            }
        }
        return AllCraftsFinished;
    }

    public void ResetCombiningState()
    {
        //Reset combining progress & Hide progress bar        
        if (lastCoroutine != null) {
            StopCoroutine(lastCoroutine);
            lastCoroutine = null;
        }
        
        ProgressBG.SetActive(false);
        combiningRecipe = null;
    }

    private void CreateCard(int cardID)
    {
        //Instantiate card and change ID
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
            audioSource.pitch = 1.0f;
            audioSource.clip = audioClips[0];
            audioSource.Play();

            transform.position += Vector3.up * 0.15f;
            dragStartPos = transform.position;
            dragStartMousePos = GetMousePos();
            
            //Remove from any participating battles
            if (battleID != -1) {
                boardManager.EndBattle(battleID);
            }
            
            //Remove from rift if original is from rift
            rb.bodyType = RigidbodyType2D.Dynamic;
            if (inRift) {
                inRift = false;
                RemoveFromRift(riftPos);
                riftPos = -1;
            }
        }
    }

    private void RemoveFromRift(int riftPos)
    {
        boardManager.riftFrame.riftCards[riftPos] = null;
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
            
            //Set CardStack to trigger state
            SetWholeStackIsTrigger(true);
        }
    }

    private void OnMouseUp()
    {
        if (draggable) {
            if (isDragging) {
                transform.position -= Vector3.up * 0.15f;

                //ALL IN ONE FUNCTION:
                //Joining Battle, Selling Cards, Joining Rift, Stack Effects ----- StackToClosestCollided()
                if (StackToClosestCollided()) {
                    isHost = false;
                    GetHost().RestackPosition();
                    audioSource.pitch = 0.9f;
                    audioSource.clip = audioClips[1];
                    audioSource.Play();
                }
                else {
                    isHost = true;
                    audioSource.pitch = 0.9f;
                    audioSource.clip = audioClips[0];
                    audioSource.Play();
                }

                isDragging = false;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

                RestackPosition();
                
                //Set CardStack back to collidable state
                SetWholeStackIsTrigger(false);
            }
        }
    }

    public void SetWholeStackIsTrigger(bool mode)
    {
        //Toggle the TriggerMode of: this and its stackedCards 
        coll.isTrigger = mode;

        foreach (Card card in stackedCards) {
            card.coll.isTrigger = mode;
        }
    }

    public void RestackPosition()
    {
        //Readjust the positioning of stacked cards
        int i = 0;
        foreach(Card stackedCard in stackedCards) {
            i++;
            stackedCard.transform.position = transform.position + Vector3.right * 0.4f * i - Vector3.up * 0.7f * i + Vector3.back * 0.004f * i;
        }
    }

    private bool StackToClosestCollided()
    {
        //Enemy Interactions
        if (cardInfo.type == 1 && battleID == -1 && ListOfOverlappedEnemies.Count > 0 && GetClosestBattleableEnemy(ListOfOverlappedEnemies) != null) {
            //Get Closest Available Enemy
            Card closestBattleableEnemy = GetClosestBattleableEnemy(ListOfOverlappedEnemies);
            
            //Start battle
            StartBattle(this, closestBattleableEnemy);
            return false;
        }

        //Spell Interactions
        if (cardInfo.type == 3 && ListOfOverlappedEnemies.Count > 0) {
            //Get closest enemy card
            float distance = 9999f;
            Card closestEnemyCard = null;

            foreach (GameObject cardGameObject in ListOfOverlappedEnemies) {
                float tempDistance = Vector2.Distance(transform.position, cardGameObject.transform.position);
                if (tempDistance < distance) {
                    if (!(id == 25 && cardGameObject.GetComponent<Card>().withering) && !(id == 26 && cardGameObject.GetComponent<Card>().frozen)) {
                        distance = tempDistance;
                        closestEnemyCard = cardGameObject.GetComponent<Card>();
                    }
                }
            }

            if (closestEnemyCard != null) {
                if (id == 25) {
                    //Apply Wither Effect
                    closestEnemyCard.Wither(cardInfo.attack, cardInfo.attackCD);
                    CardDestroy();
                }
                else if (id == 26) {
                    //Apply Freeze Effect
                    closestEnemyCard.Freeze(cardInfo.attackCD);
                    CardDestroy();
                }
                else if (id == 34) {
                    //Apply Shred Effect
                    closestEnemyCard.Shred(cardInfo.attack);
                    CardDestroy();
                }
            }
        }

        if (ListOfOverlappedFrame.Count > 0) {
            InteractableFrame OverlappedFrame = ListOfOverlappedFrame[0].GetComponent<InteractableFrame>();
            //Sell Or Buy
            if (OverlappedFrame.interactMode == InteractMode.Sell) {
                List<Card> ToBeSoldList = new();
                
                foreach(Card stackedCard in stackedCards) {
                    if (stackedCard.cardInfo.sellEffect != -1) {
                        //Record sellable stacked cards
                        ToBeSoldList.Add(stackedCard);
                    }
                }
                
                //Separate cards from each other to prevent bugs
                foreach(Card stackedCard in stackedCards) {
                    stackedCard.ResetCard();
                }
                ResetCard();

                bool sold = false;

                //Sell all recorded sellable stacked cards
                foreach(Card ToBeSold in ToBeSoldList) {
                    InstantiateMoney(ToBeSold);
                    ToBeSold.CardDestroy();
                    sold = true;
                }

                if (cardInfo.sellEffect != -1) {
                    //Check if current card is sellable, then sell it if true
                    InstantiateMoney(this);
                    CardDestroy();
                    sold = true;
                }

                if (sold) {
                    if (boardManager.objectiveStage == 2) {
                        boardManager.ProceedStage(3);
                    }
                    OverlappedFrame.audioSource.clip = OverlappedFrame.audioClips[0];
                    OverlappedFrame.audioSource.Play();
                }
            }
            
            //Enter Rift
            if (OverlappedFrame.interactMode == InteractMode.Rift) {
                if (cardInfo.type == 1 && cardInfo.id != 17 && stackedCards.Count == 0) { //If non-puppet ally only
                    if (OverlappedFrame.riftCards[0] == null) {
                        OverlappedFrame.riftCards[0] = this;
                        //Make Kinematic so that it doesn't get pushed
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        //Adjust positioning (Left)
                        transform.position = OverlappedFrame.transform.position + Vector3.down * 0.3f + Vector3.left * 2.5f;
                        
                        //Set inRift status and position inside the rift
                        inRift = true;
                        riftPos = 0;
                    }
                    else if (OverlappedFrame.riftCards[1] == null) {
                        OverlappedFrame.riftCards[1] = this;
                        //Make Kinematic so that it doesn't get pushed
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        //Adjust positioning (Center)
                        transform.position = OverlappedFrame.transform.position + Vector3.down * 0.3f;
                        
                        //Set inRift status and position inside the rift
                        inRift = true;
                        riftPos = 1;
                    }
                    else if (OverlappedFrame.riftCards[2] == null) {
                        OverlappedFrame.riftCards[2] = this;
                        //Make Kinematic so that it doesn't get pushed
                        rb.bodyType = RigidbodyType2D.Kinematic;
                        //Adjust positioning (Right)
                        transform.position = OverlappedFrame.transform.position + Vector3.down * 0.3f + Vector3.right * 2.5f;
                        
                        //Set inRift status and position inside the rift
                        inRift = true;
                        riftPos = 2;
                    }
                }
                ListOfOverlappedFrame.Clear();
            }

            ListOfOverlapped.Clear();
            ListOfOverlappedEnemies.Clear();
            return false;
        }

        //Stack
        if ((cardInfo.type != 1 && cardInfo.type != 2 && cardInfo.type != 3) || cardInfo.id == 7 || cardInfo.id == 17) {
            //Stackable type of card

            //Get closest stackable card
            float distance = 9999f;
            Card closestCard = null;

            foreach (GameObject cardGameObject in ListOfOverlapped) {
                float tempDistance = Vector2.Distance(transform.position, cardGameObject.transform.position);
                //Stackable card cannot be in Battle & cannot be in Rift
                if (tempDistance < distance && cardGameObject.GetComponent<Card>().battleID == -1 && !cardGameObject.GetComponent<Card>().inRift) {
                    distance = tempDistance;
                    closestCard = cardGameObject.GetComponent<Card>();
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

            //Clear overlapping memory
            ListOfOverlapped.Clear();
            ListOfOverlappedEnemies.Clear();
            ListOfOverlappedFrame.Clear();

            if (prevCard != null)
                return true;
            return false;
        }
        else {
            //Unstackable type of card
            return false;
        }
    }

    public void Freeze(float attackCD)
    {
        audioSource.pitch = 1.0f;
        audioSource.clip = audioClips[4];
        audioSource.Play();

        frozen = true;
        StartCoroutine(FreezeEffects(attackCD));
    }

    private IEnumerator FreezeEffects(float attackCD)
    {
        float timer = 0f;

        while (timer < attackCD) {
            FrozenFilter.color = Color.Lerp(new Color(FrozenFilter.color.r, FrozenFilter.color.g, FrozenFilter.color.b, 0.9f), new Color(FrozenFilter.color.r, FrozenFilter.color.g, FrozenFilter.color.b, 0.7f), (timer-0.3f)/attackCD);
            timer += Time.deltaTime;
            yield return null;
        }

        FrozenFilter.color = new Color(FrozenFilter.color.r, FrozenFilter.color.g, FrozenFilter.color.b, 0f);
        frozen = false;
    }

    public void Wither(int attack, float attackCD)
    {
        audioSource.pitch = 1.0f;
        audioSource.clip = audioClips[5];
        audioSource.Play();

        withering = true;
        StartCoroutine(WitherEffects(attack, attackCD, 3));
    }

    private IEnumerator WitherEffects(int attack, float attackCD, int repeatTimes) {
        if (repeatTimes == 0) {
            withering = false;
            yield break;
        }

        float timer = 0f;

        while (timer < attackCD) {
            WitherFilter.color = new Color(WitherFilter.color.r, WitherFilter.color.g, WitherFilter.color.b, Mathf.Sin(timer/attackCD * Mathf.PI) * 0.3f + 0.4f);
            timer += Time.deltaTime;
            yield return null;
        }
        
        audioSource.pitch = 0.25f;
        audioSource.clip = audioClips[2];
        audioSource.Play();

        cardInfo.currentHealth -= attack;
        WitherFilter.color = new Color(WitherFilter.color.r, WitherFilter.color.g, WitherFilter.color.b, 0f);

        GameObject damageVisual = Instantiate(DamagePrefab, transform.position + Vector3.back * 0.4f + Vector3.right * Random.Range(-0.6f, 0.6f) + Vector3.up * Random.Range(-1.2f, 1.2f), Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)));
        damageVisual.GetComponent<Damage>().side = 1;
        damageVisual.GetComponent<Damage>().damage = attack;

        StartCoroutine(WitherEffects(attack, attackCD, repeatTimes-1));
    }

    public void Shred(int attack)
    {
        audioSource.pitch = 1.0f;
        audioSource.clip = audioClips[6];
        audioSource.Play();

        cardInfo.currentHealth -= attack;

        GameObject damageVisual = Instantiate(DamagePrefab, transform.position + Vector3.back * 0.4f + Vector3.right * Random.Range(-0.6f, 0.6f) + Vector3.up * Random.Range(-1.2f, 1.2f), Quaternion.Euler(0f, 0f, Random.Range(-45f, 45f)));
        damageVisual.GetComponent<Damage>().side = 1;
        damageVisual.GetComponent<Damage>().damage = attack;
        
        StartCoroutine(ShredEffects());
    }

    private IEnumerator ShredEffects() {
        float timer = 0f;

        while (timer < 0.6f) {
            ShredFilter.color = Color.Lerp(new Color(1f, 1f, 1f, 2f), new Color(1f, 1f, 1f, 0f), timer/0.6f);
            timer += Time.deltaTime;
            yield return null;
        }
        ShredFilter.color = new Color(ShredFilter.color.r, ShredFilter.color.g, ShredFilter.color.b, 0f);
    }

    private Card GetClosestBattleableEnemy(List<GameObject> listOfOverlappedEnemies)
    {
        //Gets the closest enemy that isn't in battle
        float distance = 9999f;
        Card closestEnemyCard = null;

        foreach (GameObject enemyCardGameObject in listOfOverlappedEnemies) {
            float tempDistance = Vector2.Distance(transform.position, enemyCardGameObject.transform.position);
            if (tempDistance < distance && enemyCardGameObject.GetComponent<Card>().battleID == -1) {
                distance = tempDistance;
                closestEnemyCard = enemyCardGameObject.GetComponent<Card>();;
            }
        }

        return closestEnemyCard;
    }

    private void InstantiateMoney(Card toBeSold)
    {
        //Instantiates tiny money gems that fly to the large gem
        for (int i=0; i<toBeSold.cardInfo.sellPrice; i++) {
            Instantiate(GemMoney, toBeSold.transform.position + Random.Range(-0.1f, 0.1f) * Vector3.up + Random.Range(-0.1f, 0.1f) * Vector3.right, Quaternion.identity);
        }
    }

    public void RecursivelyRemoveFromStack(Card removedCard)
    {
        //Remove the target from the stack
        stackedCards.Remove(removedCard);
        //Remove the target's stack from the stack
        foreach (Card removedCard_StackedCard in removedCard.stackedCards) {
            stackedCards.Remove(removedCard_StackedCard);
        }

        //Repeat the process for the previous card, removing the TARGET card and its stack
        prevCard?.RecursivelyRemoveFromStack(removedCard);

        return;
    }

    public void RecursivelyAddToStack(Card addedCard)
    {
        stackedCards.Clear();

        //Add the target card to the stack
        stackedCards.Add(addedCard);
        //Add the target card's stack to the stack
        foreach (Card addedCard_StackedCard in addedCard.stackedCards) {
            stackedCards.Add(addedCard_StackedCard);
        }

        //Repeat the process for the previous card, adding the CURRENT card and its stack
        prevCard?.RecursivelyAddToStack(this);

        return;
    }

    public Card GetHost()
    {
        //Gets the host card
        //by Recursively finding the previous card
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
        //Check if the Recipe of the current stack exists in the JSON data array
        List<int> ingredientList = new();
        ingredientList.Add(id);
        foreach (Card card in stackedCards) {
            ingredientList.Add(card.id);
        }
        ingredientList.Sort(); //Sort in ascending order

        for (int i=0; i < recipeDataArray.Length; i++) {
            //The 2 arrays must be identical
            if (ingredientList.SequenceEqual(recipeDataArray[i].ingredients)) {
                combiningRecipe = recipeDataArray[i];
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Card otherCard = other.transform.GetComponent<Card>();
        if (isDragging) {
            //Record Stackable Cards
            if (otherCard != null && otherCard.stackable && !stackedCards.Contains(otherCard)) {
                if ((otherCard.cardInfo.type != 1 && otherCard.cardInfo.type != 2 && otherCard.cardInfo.type != 3) || otherCard.cardInfo.id == 7 || otherCard.cardInfo.id == 17)
                    ListOfOverlapped.Add(other.gameObject);
            }

            //Record Ememies
            if (otherCard != null && otherCard.cardInfo.type == 2) {
                ListOfOverlappedEnemies.Add(other.gameObject);
            }

            //Record Frames
            if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
                ListOfOverlappedFrame.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Record Stackable Cards
        Card otherCard = other.transform.GetComponent<Card>();
        if (otherCard != null) {
            if ((otherCard.cardInfo.type != 1 && otherCard.cardInfo.type != 2 && otherCard.cardInfo.type != 3) || otherCard.cardInfo.id == 7 || otherCard.cardInfo.id == 17)
                ListOfOverlapped.Remove(other.gameObject);
        }

        //Record Ememies
        if (otherCard != null && otherCard.cardInfo.type == 2) {
            ListOfOverlappedEnemies.Remove(other.gameObject);
        }

        //Record Frames
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
            ListOfOverlappedFrame.Remove(other.gameObject);
        }
    }
    

    private void OnCollisionEnter2D(Collision2D other)
    {
        //Check from Ally cards if any collisions with enemies
            //(Check from Ally because in case Ally is in rift and is Kinematic)
        Card otherCard = other.transform.GetComponent<Card>();
        if (cardInfo.type == 1 && battleID == -1) {
            if (otherCard != null && otherCard.cardInfo.type == 2 && otherCard.battleID == -1)
                StartBattle(this, otherCard);
                
        }

        if (draggable) {
            //Record Stackable Cards
            if (otherCard != null && otherCard.stackable && !stackedCards.Contains(otherCard)) {
                if ((otherCard.cardInfo.type != 1 && otherCard.cardInfo.type != 2 && otherCard.cardInfo.type != 3) || otherCard.cardInfo.id == 7 || otherCard.cardInfo.id == 17)
                    ListOfOverlapped.Add(other.gameObject);
            }

            //Record Ememies
            if (otherCard != null && otherCard.cardInfo.type == 2) {
                ListOfOverlappedEnemies.Add(other.gameObject);
            }

            //Record Frames
            if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
                ListOfOverlappedFrame.Add(other.gameObject);
            }
        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        //Record Stackable Cards
        Card otherCard = other.transform.GetComponent<Card>();
        if (otherCard != null) {
            if ((otherCard.cardInfo.type != 1 && otherCard.cardInfo.type != 2 && otherCard.cardInfo.type != 3) || otherCard.cardInfo.id == 7 || otherCard.cardInfo.id == 17)
                ListOfOverlapped.Remove(other.gameObject);
        }

        //Record Ememies
        if (otherCard != null && otherCard.cardInfo.type == 2) {
            ListOfOverlappedEnemies.Remove(other.gameObject);
        }

        //Record Frames
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
            ListOfOverlappedFrame.Remove(other.gameObject);
        }
    }
    
    public void StartBattle(Card goodCard, Card evilCard)
    {
        goodCard.ResetCombiningState();
        
        goodCard.IsolateCard();

        //Remove from rift
        if (goodCard.inRift) {
            goodCard.inRift = false;
            goodCard.RemoveFromRift(riftPos);
            goodCard.riftPos = -1;
        }
        
        //Set to Kinematic so it is not pushed around
        goodCard.rb.bodyType = RigidbodyType2D.Kinematic;
        evilCard.rb.bodyType = RigidbodyType2D.Kinematic;
        
        //Set the battleID to the next available ID
        goodCard.battleID = boardManager.BattleIDCounter;
        evilCard.battleID = boardManager.BattleIDCounter;
        
        //Clear all overlapping memory
        goodCard.ListOfOverlapped.Clear();
        goodCard.ListOfOverlappedFrame.Clear();
        goodCard.ListOfOverlappedEnemies.Clear();
        
        evilCard.ListOfOverlapped.Clear();
        evilCard.ListOfOverlappedFrame.Clear();
        evilCard.ListOfOverlappedEnemies.Clear();

        //Start battle between 2 cards
        boardManager.StartBattle(goodCard, evilCard);
    }

    public void IsolateCard()
    {
        //Remove from top stack
        if (!isHost) {
            isHost = true;

            GetHost().ResetCombiningState();
            
            //Recursively remove previous cards' partial stackedCards

            prevCard.RecursivelyRemoveFromStack(this);
            prevCard = null;
        }

        //Remove bottom stack
        if (stackedCards != null && stackedCards.Count > 0) {
            stackedCards[0].isHost = true;
            stackedCards[0].prevCard = null;
            stackedCards.Clear();
        }
    }
}
