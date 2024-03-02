using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class InGameCanvas : MonoBehaviour
{
    public Button ToggleButton;
    public RectTransform Box;

    public bool opened = true;

    private Vector3 ButtonOpen;
    private Vector3 ButtonClose;
    private Vector3 BoxOpen;
    private Vector3 BoxClose;

    public float animTimeLength;

    public RectTransform recipeContent;
    public List<RectTransform> Recipes;
    
    // Start is called before the first frame update
    void Start()
    {
        ButtonOpen = new Vector3(-500, 0, 0);
        ButtonClose = new Vector3(-10, 0, 0);
        BoxOpen = new Vector3(0, 0, 0);
        BoxClose = new Vector3(490, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleRecipes()
    {
        opened = !opened;
        //Debug.Log("ToggleRecipes");
        StartCoroutine(MoveRecipeBox());
    }

    private IEnumerator MoveRecipeBox()
    {
        float timer = 0;
        RectTransform buttonRect = ToggleButton.transform.GetComponent<RectTransform>();

        while (timer < animTimeLength) {
            //Transformation
            if (opened) {
                buttonRect.anchoredPosition = Vector3.Lerp(ButtonClose, ButtonOpen, timer/animTimeLength);
                Box.anchoredPosition = Vector3.Lerp(BoxClose, BoxOpen, timer/animTimeLength);
            }
            else {
                buttonRect.anchoredPosition = Vector3.Lerp(ButtonOpen, ButtonClose, timer/animTimeLength);
                Box.anchoredPosition = Vector3.Lerp(BoxOpen, BoxClose, timer/animTimeLength);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (opened) {
            buttonRect.anchoredPosition = ButtonOpen;
            Box.anchoredPosition = BoxOpen;
        }
        else {
            buttonRect.anchoredPosition = ButtonClose;
            Box.anchoredPosition = BoxClose;
        }
    }
}
