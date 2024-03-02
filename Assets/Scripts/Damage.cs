using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public int side = 1;
    private SpriteRenderer spriteRenderer;
    private TextMeshPro amount;
    private bool fade = false;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        amount = transform.GetChild(0).GetComponent<TextMeshPro>();

        if (side == 1) {
            spriteRenderer.color = new Color(0.63f, 1f, 0.5f, 1f);
        } else {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 1f);
        }
        Invoke("StartFading", 0.25f);
        Destroy(gameObject, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (fade) {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - Time.deltaTime * 4f);
            amount.color = new Color(amount.color.r, amount.color.g, amount.color.b, amount.color.a - Time.deltaTime * 4f);
        }
    }

    private void StartFading()
    {
        fade = true;
    }
}
