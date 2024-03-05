using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class Gem : MonoBehaviour
{
    private Vector3 dragStartPos;
    private Vector3 dragStartMousePos;
    
    public bool isDragging = false;

    private Collider2D coll;
    public TextMeshPro Text_Amount;

    private InteractableFrame incantFrame = null;
    private InteractableFrame inspirationFrame = null;
    private InteractableFrame curseFrame = null;
    
    [Header("Audio")]
    private AudioSource audioSource;
    

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.K) && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.U) && Input.GetKeyDown(KeyCode.F)) {
            Text_Amount.text = "999";
        }
        if (Input.GetKeyDown(KeyCode.K) && Input.GetKey(KeyCode.C) && Input.GetKey(KeyCode.U) && Input.GetKey(KeyCode.F)) {
            Text_Amount.text = "3";
        }
    }
    

    private void OnMouseDown()
    {
        transform.position += Vector3.up * 0.15f;
        dragStartPos = transform.position;
        dragStartMousePos = GetMousePos();

        audioSource.volume = 1f;
        audioSource.pitch = 1.1f;
        audioSource.Play();
    }

    private void OnMouseDrag()
    {
        //Make Gem isDragging and not interact
        transform.position = dragStartPos + GetMousePos() - dragStartMousePos;
        transform.position = new Vector3(transform.position.x, transform.position.y, -5f);
        isDragging = true;

        coll.isTrigger = true;
    }

    private void OnMouseUp()
    {
        if (isDragging) {
            audioSource.volume = 1f;
            audioSource.pitch = 1f;
            audioSource.Play();

            isDragging = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

            coll.isTrigger = false;

            if (incantFrame != null) {
                if (int.Parse(Text_Amount.text) >= 3) {
                    Text_Amount.text = (int.Parse(Text_Amount.text) - 3).ToString();
                    incantFrame.OpenPack();
                }
            }
            else if (inspirationFrame != null) {
                if (int.Parse(Text_Amount.text) >= 4) {
                    Text_Amount.text = (int.Parse(Text_Amount.text) - 4).ToString();
                    inspirationFrame.OpenPack();
                }
            }
            else if (curseFrame != null) {
                if (int.Parse(Text_Amount.text) >= 7) {
                    Text_Amount.text = (int.Parse(Text_Amount.text) - 7).ToString();
                    curseFrame.OpenPack();
                }
            }
        }
    }

    private Vector3 GetMousePos()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //mousePos.z = -5f;
        return mousePos;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
            InteractableFrame IF = other.GetComponent<InteractableFrame>();
            if (IF.interactMode == InteractMode.Incant) {
                incantFrame = IF;
            }
            if (IF.interactMode == InteractMode.Inspiration) {
                inspirationFrame = IF;
            }
            if (IF.interactMode == InteractMode.Curse) {
                curseFrame = IF;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
            InteractableFrame IF = other.GetComponent<InteractableFrame>();
            if (IF.interactMode == InteractMode.Incant) {
                incantFrame = null;
            }
            if (IF.interactMode == InteractMode.Inspiration) {
                inspirationFrame = null;
            }
            if (IF.interactMode == InteractMode.Curse) {
                curseFrame = null;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        audioSource.volume = 0.7f;
        audioSource.pitch = Random.Range(1.2f, 1.3f);
        audioSource.Play();
    }
}
