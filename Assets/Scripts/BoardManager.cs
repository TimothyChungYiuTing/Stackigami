using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Battle {
    public int id;
    public List<Card> participants;
    public GameObject battleFrame;

    public Battle(int id, List<Card> participants, GameObject battleFrame)
    {
        this.id = id;
        this.participants = participants;
        this.battleFrame = battleFrame;
    }
}
public class BoardManager : MonoBehaviour
{
    public Gem gem;
    public CardSprites cardSpritesScript;
    public List<Card> existingCardsList = new();

    public GameObject curseFrame;
    public InteractableFrame riftFrame;
    public InGameCanvas inGameCanvas;

    public bool oniDiscovered = false;
    public int stage = 0; //0: Start, 1: Pentagram discovered, 2: Rift 1 discovered
    public int objectiveStage = 0;
    public List<String> objectiveTexts;
    public List<Color> objectiveColors;
    public bool AshiyaDouman_Defeated = false;


    [Header("Instantiated")]
    public GameObject BattleFramePrefab;


    [Header("Battle")]
    public int BattleIDCounter = 0;
    public List<Battle> battleList = new();

    [Header("Recipe Management")]
    public List<CardDataManager.RecipeData>[] UndiscoveredRecipes_Stage = new List<CardDataManager.RecipeData>[3];
    public List<CardDataManager.RecipeData> DiscoveredRecipes = new();

    [Header("Recipe Management")]
    public List<AudioClip> audioClips;
    private AudioSource audioSource;

    public void ProceedStage(int objectiveStageIndex)
    {
        objectiveStage = objectiveStageIndex;
        inGameCanvas.Text_Objective.text = objectiveTexts[objectiveStageIndex];
        inGameCanvas.Text_Objective.color = objectiveColors[objectiveStageIndex];
    }

    public void StartBattle(Card goodCard, Card evilCard)
    {
        //Start battle between 2 cards
            //Debug.Log("Battle started between " + goodCard.id + " & " + evilCard.id);

        GameObject createdBattleFrame;

        //Create the BattleFrame at either left or right according to the encounter direction
        if (evilCard.transform.position.x < goodCard.transform.position.x) {
            createdBattleFrame = Instantiate(BattleFramePrefab, evilCard.transform.position + Vector3.right, Quaternion.identity);
            goodCard.transform.position = evilCard.transform.position + Vector3.right * 2f;
        }
        else {
            createdBattleFrame = Instantiate(BattleFramePrefab, evilCard.transform.position + Vector3.left, Quaternion.identity);
            goodCard.transform.position = evilCard.transform.position + Vector3.left * 2f;
        }

        //Elevate goodCard to avoid getting softlocked bug
        goodCard.transform.position = new Vector3(goodCard.transform.position.x, goodCard.transform.position.y, -3.3f);

        //Record all participants
        List<Card> participants = new() { goodCard, evilCard };

        //Assign all the battle data into a new Battle
        battleList.Add(new Battle(BattleIDCounter, participants, createdBattleFrame));

        //Increment the Battle ID counter
        BattleIDCounter++;

        //Start repeating attacks
        goodCard.RepeatAttack();
        evilCard.RepeatAttack();
    }

    public void EndBattle(int battleID)
    {
        //Find the battle with this battleID
        Battle battleToEnd = null;
        foreach(Battle battle in battleList) {
            if (battle.id == battleID) {
                battleToEnd = battle;
            }
        }

        if (battleToEnd != null) {
            //Make the battleID of the goodCard and badCard -1
            foreach(Card participant in battleToEnd.participants) {
                participant.lastTimeHealed = Time.time;

                participant.battleID = -1;
                participant.rb.bodyType = RigidbodyType2D.Dynamic;
                
                if (participant.lastCoroutine != null) {
                    participant.StopCoroutine(participant.lastCoroutine);
                    participant.lastCoroutine = null;
                }
                participant.CancelInvoke("Attack");
                participant.transform.localScale = Vector3.one;
                participant.transform.localRotation = Quaternion.identity;
                participant.HurtFilter.color = new Color(participant.HurtFilter.color.r, participant.HurtFilter.color.g, participant.HurtFilter.color.b, 0f);
            }
            //Remove the BattleFrame
            Destroy(battleToEnd.battleFrame);
            //Remove the Battle
            battleList.Remove(battleToEnd);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        gem = FindObjectOfType<Gem>();
        cardSpritesScript = FindObjectOfType<CardSprites>();
        audioSource = GetComponent<AudioSource>();

        objectiveTexts = new() {
            "Objective:\tOpen an Incant Pack",
            "Objective:\tComplete a Recipe",
            "Objective:\tSell a Card",
            "Objective:\tCreate a 2nd Conjurer",
            "Objective:\tDefeat an Oni",
            "Objective:\tCraft a Pentagram",
            "Objective:\tCreate a Shikigami",
            "Objective:\tCraft a Spell",
            "Objective:\tDefeat Hebi Onna",
            "Objective:\tDiscover the Rift",
            "Objective:\tSummon Ashiya Douman",
            "Objective:\tDefeat Ashiya Douman",
            "Objective:\tDefeat all enemies",
            "Congratulations! You Win!",
        };

        objectiveColors = new() {
            Color.green,
            Color.green,
            Color.green,
            Color.yellow,
            Color.magenta,
            Color.yellow,
            Color.yellow,
            Color.green,
            Color.magenta,
            Color.yellow,
            Color.cyan,
            Color.magenta,
            Color.magenta,
            Color.green,
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (AshiyaDouman_Defeated && AllEnemiesDefeated()) {
            //TODO: Win Condition
            AshiyaDouman_Defeated = false;
            Debug.LogError("WIN!");
            ProceedStage(13);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            List<int> uniqueIDs = new();
            List<List<Card>> ListOfSameIDCardLists = new();
            bool hasChanged = false;

            //Organizes matching stacks
            foreach(Card card in existingCardsList) {
                if (card.battleID == -1 && !card.inRift && !card.removed && card.stackable && card.prevCard == null && card.stackedCards.Count > 0) {
                    //Check if id does not exist, then Check if all children cards are same. If same, add all cards a single SameIDCardList to be added to ListOfSameIDCardLists
                    bool allCardsSame = true;
                    foreach(Card stackedCard in card.stackedCards) {
                        if (stackedCard.id != card.id) {
                            allCardsSame = false;
                        }
                    }

                    if (allCardsSame) {
                        if (!uniqueIDs.Contains(card.id)) {
                            //Create new list with self and stackedCards
                            List<Card> newCardList = new() { card };
                            newCardList.AddRange(card.stackedCards);
                            uniqueIDs.Add(card.id);
                            ListOfSameIDCardLists.Add(newCardList);
                        } else {
                            hasChanged = true;
                            //Add self and stackedCards to list
                            List<Card> newCardList = new() { card };
                            newCardList.AddRange(card.stackedCards);
                            
                            for (int i=0; i<uniqueIDs.Count; i++) {
                                if (uniqueIDs[i] == card.id) {
                                    ListOfSameIDCardLists[i].AddRange(newCardList);
                                }
                            }
                        }
                    }
                }
            }

            //Organizes solo cards
            foreach(Card card in existingCardsList) {
                //Get all cards that can be autostacked
                if (card.battleID == -1 && !card.inRift && !card.removed && card.stackable && card.prevCard == null && card.stackedCards.Count == 0) {
                    if (!uniqueIDs.Contains(card.id)) {
                        //Create new list
                        List<Card> newCardList = new() { card };
                        uniqueIDs.Add(card.id);
                        ListOfSameIDCardLists.Add(newCardList);
                    }
                    else {
                        //Add to list
                        hasChanged = true;
                        for (int i=0; i<uniqueIDs.Count; i++) {
                            if (uniqueIDs[i] == card.id) {
                                ListOfSameIDCardLists[i].Add(card);
                            }
                        }
                    }
                }
            }

            //Auto stack these cards in the lists
            foreach (List<Card> SameIDCardList in ListOfSameIDCardLists) {
                for(int i = 0; i < SameIDCardList.Count; i++) {
                    if (i == 0) {
                        SameIDCardList[i].isHost = true;
                        SameIDCardList[i].prevCard = null;
                    }
                    else {
                        SameIDCardList[i].isHost = false;
                        SameIDCardList[i].prevCard = SameIDCardList[i-1];
                    }

                    if (i+1 < SameIDCardList.Count)
                        SameIDCardList[i].stackedCards = SameIDCardList.GetRange(i+1, SameIDCardList.Count - (i+1));
                }
            }

            if (hasChanged) {
                audioSource.clip = audioClips[0];
                audioSource.Play();
            }
        }
    }

    private bool AllEnemiesDefeated()
    {
        foreach(Card card in existingCardsList) {
            if (card.cardInfo.type == 2)
                return false;
        }
        return true;
    }

    public Card GetOpponentCard(Card selfCard, int battleID)
    {
        //Find the battle with this battleID
        Battle targetBattle = null;

        foreach(Battle battle in battleList) {
            if (battle.id == battleID) {
                targetBattle = battle;
            }
        }
        
        List<Card> OpponentCards = new();

        //Return all opponent cards
        foreach (Card participant in targetBattle.participants) {
            if (participant.cardInfo.type != selfCard.cardInfo.type) {
                OpponentCards.Add(participant);
            }
        }

        //Return one of the opponent cards
        if (OpponentCards.Count > 0)
            return OpponentCards[UnityEngine.Random.Range(0, OpponentCards.Count)];
        
        return null;
    }

    public void ReadUndiscoveredRecipes(CardDataManager.RecipeData[] recipeDatas) 
    {
        //Initialize
        for (int i=0; i<UndiscoveredRecipes_Stage.Length; i++) {
            UndiscoveredRecipes_Stage[i] = new();
        }

        //Put recipes into lists according to stage
        DiscoveredRecipes = new() { recipeDatas[0] };

        for (int i=1; i < recipeDatas.Length; i++) {
            UndiscoveredRecipes_Stage[recipeDatas[i].recipeStage].Add(recipeDatas[i]);
        }
    }
}
