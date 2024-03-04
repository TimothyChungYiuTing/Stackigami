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
    public List<GameObject> RiftPositionsHint;

    [Header("Instantiated")]
    public GameObject PackPrefab;
    public GameObject CardPrefab;

    
    [Header("Rift")]
    public bool isExploring = false;
    public float riftNeededTime = 20f;
    public GameObject ProgressBG;
    public List<int> PossibleRiftOutcomes;
    private bool riftUnlocked = false;

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
        if (interactMode == InteractMode.Rift) {
            if (!isExploring && (riftCards[0] != null || riftCards[1] != null || riftCards[2] != null)) {
                isExploring = true;
                StartCoroutine(Explore(riftNeededTime));
            }
            else if (isExploring && riftCards[0] == null && riftCards[1] == null && riftCards[2] == null) {
                isExploring = false;
                StopAllCoroutines();
                ProgressBG.SetActive(false);
            }
        }
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
            //3 Cards, 2-3 Inspirations
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
        if (!riftUnlocked) {
            riftUnlocked = true;
            AudioManager.Instance.ChangeSong(2);
            Text_Words.text = "Seimei's Palace";
            Text_Words.transform.localPosition = new Vector3(0f, 1.8f, -3f);
            coll.enabled = true;
            foreach (GameObject hint in RiftPositionsHint) {
                hint.SetActive(true);
            }
            Invoke("CollTriggerTrue", 0.4f);
        }
    }
    
    private IEnumerator Explore(float neededTime)
    {
        float timer = 0;
        Vector3 fromPos = new Vector3(-0.49f, 0f, -0.1f);
        Vector3 toPos = new Vector3(0f, 0f, -0.1f);
        Vector3 fromScale = new Vector3(0.005f, 0.8f, 1f);
        Vector3 toScale = new Vector3(0.99f, 0.8f, 0.1f);
        
        ProgressBG.SetActive(true);

        int combiningID = PossibleRiftOutcomes[Random.Range(0, PossibleRiftOutcomes.Count)];

        Transform ProgressBarTransform = ProgressBG.transform.GetChild(0);
        while(timer < neededTime) {
            ProgressBarTransform.localPosition = Vector3.Lerp(fromPos, toPos, timer/neededTime);
            ProgressBarTransform.localScale = Vector3.Lerp(fromScale, toScale, timer/neededTime);

            if (riftCards[0] != null)
                timer += Time.deltaTime;
            if (riftCards[1] != null)
                timer += Time.deltaTime;
            if (riftCards[2] != null)
                timer += Time.deltaTime;

            yield return null;
        }

        ProgressBG.SetActive(false);
        CreateCard(combiningID);

        StartCoroutine(Explore(neededTime));
    }
    
    private void CreateCard(int cardID)
    {
        //Instantiate card and change ID
        GameObject NewCard = Instantiate(CardPrefab, transform.position + Vector3.up * 4f, Quaternion.identity);
        NewCard.GetComponent<Card>().id = cardID;
    }
    
}
