using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;

public enum InteractMode {Sell, Incant, Inspiration, Curse, Rift}
public class InteractableFrame : MonoBehaviour
{
    public InteractMode interactMode;

    private Collider2D coll;
    private BoardManager boardManager;
    private TextMeshPro Text_Words;
    
    public Card[] riftCards = new Card[3];

    [Header("Instantiated")]
    public GameObject PackPrefab;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        boardManager = FindObjectOfType<BoardManager>();
        Text_Words = transform.GetChild(0).GetComponent<TextMeshPro>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPack()
    {
        coll.isTrigger = false;
        Invoke("CollTriggerTrue", 0.3f);
        if (interactMode == InteractMode.Incant) {
            CreatePack(0);
            //5 Cards, 50% blank charm
        }
        else if (interactMode == InteractMode.Inspiration) {
            CreatePack(1);
            //2 Cards, 1-2 Inspirations
        }
        else if (interactMode == InteractMode.Curse) {
            CreatePack(2);
            //4 Cards, Higher chance of better cards, with chances of summoning enemies
        }
    }

    private void CreatePack(int packID)
    {
        GameObject NewPack = Instantiate(PackPrefab, transform.position + Vector3.down * 3f, Quaternion.identity);
        NewPack.GetComponent<BoosterPack>().packID = packID;
    }

    private void CollTriggerTrue()
    {
        coll.isTrigger = true;
    }

    public void UnlockRift()
    {
        Text_Words.text = "Seimei's Palace";
        Text_Words.transform.localPosition = new Vector3(0f, 1.4f, -0.002f);
        coll.enabled = true;
        Invoke("CollTriggerTrue", 0.4f);
    }
}
