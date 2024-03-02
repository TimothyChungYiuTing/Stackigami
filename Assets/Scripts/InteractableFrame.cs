using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractMode {Sell, Incant, Inspiration, Rift}
public class InteractableFrame : MonoBehaviour
{
    public InteractMode interactMode;

    [Header("Instantiated")]
    public GameObject CardPrefab;

    private Collider2D coll;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPack()
    {
        coll.isTrigger = false;
        Invoke("CollTriggerTrue", 0.3f);
        for (int i=0; i<5; i++) {
            if (Random.Range(0, 2) == 0)
                CreateCard(0, transform.position + Vector3.back * 0.1f);
            else
                CreateCard(Random.Range(2, 7), transform.position + Vector3.back * 0.1f);
        }
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
