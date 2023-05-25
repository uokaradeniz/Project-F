using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameHandler : MonoBehaviour
{
    public List<GameObject> selectedCards;
    private int lives = 3;
    public List<Transform> cardPositions;

    private TextMeshProUGUI lifeText;
    private TextMeshProUGUI loseText;
    private TextMeshProUGUI winText;
    private TextMeshProUGUI counterText;
    private GameObject playAgainButton;

    public static int cardCount;
    public AudioSource sHandlerAudioSource;
    public AudioSource dLightAudioSource;
    public AudioSource eventHandlerAudioSource;

    private Transform startingPos;
    public float distributionTime;
    public bool isDistributed;
    public bool cardsShown;
    public bool gameWon;
    public bool gameLost;

    public bool stopSelection;
    public int gameDuration;
    private Animator animator;

    private int counter = 1;

    public int Lives
    {
        get { return lives; }
        set
        {
            lives = value;
            if (lives <= 0)
                lives = 0;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        loseText = GameObject.Find("LoseText").GetComponent<TextMeshProUGUI>();
        winText = GameObject.Find("WinText").GetComponent<TextMeshProUGUI>();
        lifeText = GameObject.Find("LifeText").GetComponent<TextMeshProUGUI>();
        playAgainButton = GameObject.Find("Bt_PlayAgain");
        playAgainButton.SetActive(false);
        startingPos = GameObject.Find("Starting Position").GetComponent<Transform>();
        animator = GameObject.Find("Canvas").GetComponent<Animator>();
        sHandlerAudioSource = GetComponent<AudioSource>();
        dLightAudioSource = GameObject.Find("Directional Light").GetComponent<AudioSource>();
        eventHandlerAudioSource = GameObject.Find("EventSystem").GetComponent<AudioSource>();
        counterText = GameObject.Find("CounterText").GetComponent<TextMeshProUGUI>();
        stopSelection = true;
        GameInitialization();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDistributed)
            Game();

        if (cardCount <= 0)
            WonGame();
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count - 1; i++)
        {
            T temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    void GameInitialization()
    {
        foreach (var child in GameObject.Find("Card Positions").GetComponentsInChildren<Transform>())
        {
            cardPositions.Add(child);
        }

        cardPositions.RemoveAt(0);
        cardCount = cardPositions.Count;
        ShuffleList(cardPositions);

        for (int i = 0; i < cardPositions.Count; i++)
        {
            Card card = Instantiate(Resources.Load("Card"), startingPos.position, Quaternion.identity)
                .GetComponent<Card>();

            if (!isDistributed)
                StartCoroutine(Lerp(i, card));

            card.name += i;

            if (i < 2)
            {
                card.cardType = Card.CardType.A;
                card.cardImage.sprite = Resources.Load("Textures/1", typeof(Sprite)) as Sprite;
            }
            else if (i >= 2 && i < 4)
            {
                card.cardType = Card.CardType.B;
                card.cardImage.sprite = Resources.Load("Textures/2", typeof(Sprite)) as Sprite;
            }
            else if (i >= 4 && i < 6)
            {
                card.cardType = Card.CardType.C;
                card.cardImage.sprite = Resources.Load("Textures/3", typeof(Sprite)) as Sprite;
            }
            else if (i >= 6 && i < 8)
            {
                card.cardType = Card.CardType.D;
                card.cardImage.sprite = Resources.Load("Textures/4", typeof(Sprite)) as Sprite;
            }
            else if (i >= 8 && i < 10)
            {
                card.cardType = Card.CardType.E;
                card.cardImage.sprite = Resources.Load("Textures/5", typeof(Sprite)) as Sprite;
            }
            else if (i >= 10 && i < 12)
            {
                card.cardType = Card.CardType.F;
                card.cardImage.sprite = Resources.Load("Textures/6", typeof(Sprite)) as Sprite;
            }
        }

        StartCoroutine(GameTimer());
    }

    private IEnumerator Lerp(int i, Card card)
    {
        while (true)
        {
            card.transform.position = Vector3.Lerp(card.transform.position, cardPositions[i].position,
                Time.deltaTime * distributionTime);

            if (Vector3.Distance(card.transform.position, cardPositions[i].position) < 0.1)
            {
                isDistributed = true;
                yield break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    void Game()
    {
        lifeText.text = "Lives: " + Lives;
        if (Lives > 0 && !gameWon)
        {
            if (selectedCards.Count == 2)
            {
                Card card1 = selectedCards[0].GetComponent<Card>();
                Card card2 = selectedCards[1].GetComponent<Card>();
                if (card1.cardType == card2.cardType)
                {
                    card1.animator.enabled = true;
                    card2.animator.enabled = true;
                    card1.TriggerAnimation();
                    card2.TriggerAnimation();
                    cardCount -= 2;
                    selectedCards.Clear();
                }
                else
                {
                    Lives--;
                    card1.CardSelected = false;
                    card2.CardSelected = false;
                    card1.WrongCardSelected = true;
                    card2.WrongCardSelected = true;
                    selectedCards.Clear();
                }
            }
        }
        else
        {
            LostGame();
        }

        if (counter <= 0 && cardsShown && !gameWon)
        {
            LostGame();
        }
    }

    private bool audioPlayed = false;

    private void LostGame()
    {
        gameLost = true;
        stopSelection = true;
        playAgainButton.SetActive(true);
        if (!audioPlayed)
        {
            sHandlerAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_Lose"));
            audioPlayed = true;
        }

        animator.SetTrigger("PlayLoseAnim");
    }

    private void WonGame()
    {
        gameWon = true;
        playAgainButton.SetActive(true);

        if (!audioPlayed)
        {
            sHandlerAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_Win"));
            audioPlayed = true;
        }

        animator.SetTrigger("PlayWinAnim");
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public IEnumerator GameTimer()
    {
        bool lastFiveSeconds = false;
        counter = gameDuration;
        while (counter > 0)
        {
            if (gameWon)
            {
                counterText.text = "Game Ended.";
                eventHandlerAudioSource.Stop();
                yield break;
            }

            if (gameLost)
            {
                counterText.text = "Game Ended.";
                eventHandlerAudioSource.Stop();
                yield break;
            }

            yield return new WaitForSeconds(1);
            if (cardsShown)
            {
                counter--;
                if (!eventHandlerAudioSource.isPlaying && !lastFiveSeconds)
                    eventHandlerAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_Timer"));

                if (counter <= gameDuration * 0.666f && counter > gameDuration * 0.333f)
                    eventHandlerAudioSource.pitch = 1.1f;
                else if (counter <= gameDuration * 0.333f && counter > gameDuration * 0.116f)
                    eventHandlerAudioSource.pitch = 1.25f;
                else if (counter <= gameDuration * 0.116f)
                {
                    if (!lastFiveSeconds)
                    {
                        eventHandlerAudioSource.pitch = 1;
                        eventHandlerAudioSource.volume = 0.05f;
                        eventHandlerAudioSource.PlayOneShot((AudioClip)Resources.Load("SFX_TimerEnd"));
                        lastFiveSeconds = true;
                    }
                }

                counterText.text = "Time: " + counter.ToString();
            }
        }
    }
}