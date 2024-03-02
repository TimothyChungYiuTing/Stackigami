using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Battle {
    public int id;
    public List<Card> participants;

    public Battle(int id, List<Card> participants)
    {
        this.id = id;
        this.participants = participants;
    }
}
public class BoardManager : MonoBehaviour
{
    public Gem gem;
    public CardSprites cardSpritesScript;
    public List<Card> existingCardsList = new();
    public List<Battle> battleList = new();

    public GameObject curseFrame;

    public bool oniDiscovered = false;
    public int stage = 0; //0: Start, 1: Pentagram discovered, 2: Rift 1 discovered

    public void StartBattle(Card goodCard, Card evilCard)
    {
        //Start battle between 2 cards

        Debug.Log("Battle started between " + goodCard.id + " & " + evilCard.id);
    }

    // Start is called before the first frame update
    void Awake()
    {
        gem = FindObjectOfType<Gem>();
        cardSpritesScript = FindObjectOfType<CardSprites>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
