using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameCanvas : MonoBehaviour
{
    public Button ToggleButton;
    public RectTransform Box;
    public TextMeshProUGUI Text_Objective;
    public Image Notification;

    public bool opened = true;
    private bool notificationStartDisappearing = false;

    private Vector3 ButtonOpen;
    private Vector3 ButtonClose;
    private Vector3 BoxOpen;
    private Vector3 BoxClose;

    public float animTimeLength;

    public RectTransform recipeContent;
    public List<RectTransform> RecipeTransforms;

    [Header("Instantiated")]
    public GameObject recipePrefab;

    [Header("Audio")]
    public List<AudioClip> audioClips;
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        ButtonOpen = new Vector3(-500, 0, 0);
        ButtonClose = new Vector3(-10, 0, 0);
        BoxOpen = new Vector3(0, 0, 0);
        BoxClose = new Vector3(490, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (opened && Notification.enabled && !notificationStartDisappearing) {
            notificationStartDisappearing = true;
            Invoke("Notification_Disappear", 15f);
        }
    }

    public void ToggleRecipes()
    {
        audioSource.clip = audioClips[0];
        audioSource.Play();

        opened = !opened;
        //Debug.Log("ToggleRecipes");
        StartCoroutine(MoveRecipeBox());
        
        if (!opened) {
            AllSeen();
        }
    }

    public void AllSeen()
    {
        foreach (RectTransform recipeRect in RecipeTransforms) {
            recipeRect.GetComponent<Recipe>().Seen();
        }
    }
    
    private void Notification_Disappear()
    {
        Notification.enabled = false;
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

    public void AddRecipe(CardDataManager.RecipeData recipeData, bool made)
    {
        //Instantiate new recipe UI
        GameObject newRecipe = Instantiate(recipePrefab, recipeContent.transform.position, Quaternion.identity);
        newRecipe.GetComponent<RectTransform>().SetParent(recipeContent.transform); //Assign parent

        //Add to the list of recipe transforms
        RecipeTransforms.Add(newRecipe.GetComponent<RectTransform>());

        //Shift all existing recipes downwards (This makes newest recipes stay on top)
        foreach(RectTransform rect in RecipeTransforms) {
            rect.anchoredPosition += Vector2.down * 250f;
        }

        //Reposition new recipe
        RecipeTransforms[^1].anchoredPosition = new Vector3(3.5f, -50f);
        RecipeTransforms[^1].localScale = Vector3.one;
        
        //Resize the Content for smart navigation
        recipeContent.sizeDelta = new Vector2(recipeContent.sizeDelta.x, 50f + 250f * RecipeTransforms.Count);

        newRecipe.GetComponent<Recipe>().Setup(recipeData, made);

        Notification.enabled = true;
        notificationStartDisappearing = false;
    }
}
