using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public int id = 0;
    public CardInfo cardInfo;
    public CardDataManager cardDataManager;


    // Start is called before the first frame update    
    void Start()
    {
        cardDataManager = FindObjectOfType<CardDataManager>();
        cardInfo = new CardInfo(id, cardDataManager.cardDatas.cardDataArray);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDrag()
    {
        transform.position = GetMousePos();
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
}
