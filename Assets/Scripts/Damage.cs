using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Damage : MonoBehaviour
{
    public int side = 1;
    public int damage = 1;
    private SpriteRenderer spriteRenderer_BG;
    private SpriteRenderer spriteRenderer_Color;
    private TextMeshPro Text_Amount;
    private bool fade = false;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer_BG = transform.GetChild(1).GetComponent<SpriteRenderer>();
        spriteRenderer_Color = transform.GetChild(2).GetComponent<SpriteRenderer>();
        Text_Amount = transform.GetChild(0).GetComponent<TextMeshPro>();

        Text_Amount.text = damage.ToString();

        if (side == 1) {
            spriteRenderer_Color.color = new Color(0.63f, 1f, 0.5f, 1f);
        } else {
            spriteRenderer_Color.color = new Color(1f, 0.5f, 0.5f, 1f);
        }
        Invoke("StartFading", 0.25f);
        Destroy(gameObject, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (fade) {
            spriteRenderer_Color.color = new Color(spriteRenderer_Color.color.r, spriteRenderer_Color.color.g, spriteRenderer_Color.color.b, spriteRenderer_Color.color.a - Time.deltaTime * 4f);
            spriteRenderer_BG.color =  new Color(spriteRenderer_BG.color.r, spriteRenderer_BG.color.g, spriteRenderer_BG.color.b, spriteRenderer_BG.color.a - Time.deltaTime * 5f);
            Text_Amount.color = new Color(Text_Amount.color.r, Text_Amount.color.g, Text_Amount.color.b, Text_Amount.color.a - Time.deltaTime * 4f);
        }
    }

    private void StartFading()
    {
        fade = true;
    }
}
