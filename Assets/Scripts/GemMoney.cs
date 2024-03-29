using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemMoney : MonoBehaviour
{
    private bool canFly = false;
    private Gem gem;
    private Collider2D coll;
    private SpriteRenderer spriteRenderer;
    
    [Header("Audio")]
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        gem = FindObjectOfType<Gem>();
        coll = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        Invoke("BecomeCanFly", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (canFly) {
            transform.position += (gem.transform.position - transform.position).normalized * Time.deltaTime * 25f;
        }
    }

    private void BecomeCanFly()
    {
        canFly = true;
        coll.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Gem")) {
            gem.Text_Amount.text = (int.Parse(gem.Text_Amount.text) + 1).ToString();
            coll.enabled = false;
            spriteRenderer.enabled = false;
            audioSource.pitch = 1.2f;
            audioSource.Play();
            Destroy(gameObject, 0.3f);
        }
    }
}
