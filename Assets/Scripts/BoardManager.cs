using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public Gem gem;
    // Start is called before the first frame update
    void Start()
    {
        gem = FindObjectOfType<Gem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
