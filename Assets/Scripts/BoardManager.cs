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
