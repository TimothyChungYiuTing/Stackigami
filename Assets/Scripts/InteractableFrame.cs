using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public enum InteractMode {Sell, Incant, Inspiration, Curse, Rift}
public class InteractableFrame : MonoBehaviour
{
    public InteractMode interactMode;

    [Header("Instantiated")]
    public GameObject CardPrefab;

    private Collider2D coll;
    private BoardManager boardManager;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        boardManager = FindObjectOfType<BoardManager>();
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
            //3 Cards
            
            for (int i=0; i<5; i++) {    
                if (Random.Range(0, 2) == 0)
                    CreateCard(0, transform.position + Vector3.back * 0.1f);
                else
                    CreateCard(Random.Range(2, 7), transform.position + Vector3.back * 0.1f);
            }
        }
        else if (interactMode == InteractMode.Inspiration) {
            //2 Cards, 1-2 Inspirations
            for (int i=0; i<2; i++) {    
                if (Random.Range(0, 2) == 0)
                    CreateCard(0, transform.position + Vector3.back * 0.1f);
                else
                    CreateCard(Random.Range(2, 7), transform.position + Vector3.back * 0.1f);
            }
            CreateInspiration(boardManager.stage);

            //50% to create 1 more inspiration
            if (Random.Range(0, 2) == 0)
                CreateInspiration(boardManager.stage);
        }
        else if (interactMode == InteractMode.Curse) {
            //4 Cards, Higher chance of better cards, with chances of summoning enemies

            for (int i=0; i<4; i++) { 
                //10% chance of enemy encounter
                if (Random.Range(0, 10) == 0) {
                    if (boardManager.stage < 2)
                        CreateCard(15, transform.position + Vector3.back * 0.1f);
                    else {
                        if (Random.Range(0, 4) == 0) {
                            CreateCard(15, transform.position + Vector3.back * 0.1f);
                        }
                        else {
                            CreateCard(23, transform.position + Vector3.back * 0.1f);
                        }
                    }
                }
                else {
                    if (Random.Range(0, 5) == 0)
                        CreateCard(0, transform.position + Vector3.back * 0.1f);
                    else if (Random.Range(0, 3) == 0)
                        CreateCard(Random.Range(8, 11), transform.position + Vector3.back * 0.1f);
                    else
                        CreateCard(Random.Range(2, 7), transform.position + Vector3.back * 0.1f);
                }
            }
        }
    }

    private void CreateInspiration(int stage)
    {
        //TODO: Unlock recipe
    }

    private void CreateCard(int cardID, Vector3 createPos)
    {
        //TODO: Instantiate card and change ID
        GameObject NewCard = Instantiate(CardPrefab, createPos + Random.Range(-0.1f, 0.1f) * Vector3.up + Random.Range(-0.1f, 0.1f) * Vector3.right, Quaternion.identity);
        NewCard.GetComponent<Card>().id = cardID;
    }

    private void CollTriggerTrue()
    {
        coll.isTrigger = true;
    }
}
