using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    private bool cardSelected;
    private bool wrongCardSelected;
    public Animator animator;

    public bool WrongCardSelected
    {
        get { return wrongCardSelected; }
        set { wrongCardSelected = value; }
    }

    public bool CardSelected
    {
        get { return cardSelected; }
        set { cardSelected = value; }
    }

    public enum CardType
    {
        A,
        B,
        C,
        D,
        E,
        F
    }

    public CardType cardType;
    private GameHandler gameHandler;
    private Renderer childMeshMaterial;
    public SpriteRenderer cardImage;

    // Start is called before the first frame update
    void Start()
    {
        cardImage = transform.Find("Mesh/Image").GetComponent<SpriteRenderer>();
        gameHandler = GameObject.Find("SessionHandler").GetComponent<GameHandler>();
        childMeshMaterial = transform.Find("Mesh").GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        animator.enabled = false;
        CreateCardText();
    }

    private void Update()
    {
        if (gameHandler.isDistributed && !gameHandler.cardsShown)
        {
            animator.enabled = true;
            animator.SetTrigger("ShowCards");
        }

        if (CardSelected)
            childMeshMaterial.material = (Material)Resources.Load("M_Selected");
        else
            childMeshMaterial.material = (Material)Resources.Load("M_Normal");

        if (WrongCardSelected)
        {
            gameHandler.stopSelection = true;
            animator.enabled = true;
            animator.SetTrigger("SelectedWrong");
            if (!gameHandler.dLightAudioSource.isPlaying)
                gameHandler.dLightAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_Wrong"));
        }
    }

    public void ShowCardsPhase()
    {
        animator.ResetTrigger("ShowCards");
        gameHandler.cardsShown = true;
        gameHandler.stopSelection = false;
        animator.enabled = false;
    }

    public void PlayCardSFX()
    {
        gameHandler.dLightAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_CardFlip"));
    }

    private void OnMouseDown()
    {
        if (!gameHandler.stopSelection && gameHandler.cardsShown)
        {
            if (gameObject.CompareTag("Card") && gameHandler.selectedCards.Count < 2 && gameHandler.Lives > 0 &&
                !gameObject.GetComponent<Card>().CardSelected)
            {
                cardSelected = true;
                gameHandler.selectedCards.Add(gameObject);
            }
        }
    }

    private void CreateCardText()
    {
        transform.Find("Mesh/Text").GetComponent<TextMeshPro>().text = cardType.ToString();
    }

    public void WrongMatSelected()
    {
        animator.enabled = false;
        childMeshMaterial.material = (Material)Resources.Load("M_Normal");
        WrongCardSelected = false;
        gameHandler.stopSelection = false;
    }

    public void TriggerAnimation()
    {
        if (!gameHandler.dLightAudioSource.isPlaying)
            gameHandler.dLightAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_Correct"));
        animator.SetTrigger("DestroyCard");
    }

    public void DestroyCard()
    {
        Destroy(gameObject);
    }
}