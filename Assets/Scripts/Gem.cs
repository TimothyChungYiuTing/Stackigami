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

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    private void OnMouseDown()
    {
        transform.position += Vector3.up * 0.15f;
        dragStartPos = transform.position;
        dragStartMousePos = GetMousePos();
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
            isDragging = false;
            transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.1f - 2.5f);

            coll.isTrigger = false;

            if (incantFrame != null) {
                if (int.Parse(Text_Amount.text) >= 3) {
                    Text_Amount.text = (int.Parse(Text_Amount.text) - 3).ToString();
                    incantFrame.OpenPack();
                    incantFrame = null;
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
        if (isDragging) {
            if (other.gameObject.layer == LayerMask.NameToLayer("InteractableFrame")) {
                InteractableFrame IF = other.GetComponent<InteractableFrame>();
                if (IF.interactMode == InteractMode.Incant) {
                    incantFrame = IF;
                }
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
        }
    }
}
