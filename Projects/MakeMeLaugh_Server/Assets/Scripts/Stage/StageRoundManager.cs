
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[RequireComponent(typeof(UIDocument))]
public class StageRoundManager : MonoBehaviour
{
    public static IEnumerable<Joke> Jokes { get; private set; }

    private UIDocument _uiHost;
    private Label _subtitle;
    public Animator director;
    public DialogOptionsTable closingLines;
    public ComedianController comedian;

    public event Action OnFinishedSet;
    
    public Joke CurrentJoke { get; private set; }
    public bool AcceptingLaughs { get; private set; }
    
    public static void BeginSet(IEnumerable<Joke> jokes)
    {
        Jokes = jokes.ToArray();
    }

    void OnEnable()
    {
        _uiHost = GetComponent<UIDocument>();
        _subtitle = _uiHost.rootVisualElement.Q<Label>("Subtitle");
        _subtitle.text = String.Empty;
        CurrentJoke = null;
        
        if (TransportServer.Instance != null)
            TransportServer.Instance.OnPlayerMessageReceived += OnPlayerMessageReceived;

        if (Jokes == null)
        {
            // Install some default jokes for now, for testing
            var testPlayer = new Player("0", "Nobody");

            var joke = new Joke(testPlayer, new List<Player> { testPlayer });
            joke.AddPunchlineSegment(new PunchlineSegment(testPlayer) {Text = "To get to"});
            joke.AddPunchlineSegment(new PunchlineSegment(testPlayer) { Text = "the other side!" });
            var joke2 = new Joke(testPlayer, new List<Player> { testPlayer });
            joke2.AddPunchlineSegment(new PunchlineSegment(testPlayer) { Text = "Because it was stapled to" });
            joke2.AddPunchlineSegment(new PunchlineSegment(testPlayer) { Text = "the tortoise!" });
            var joke3 = new Joke(testPlayer, new List<Player> { testPlayer });
            joke3.AddPunchlineSegment(new PunchlineSegment(testPlayer)
                { Text = "It's really none of our goddamned business." });
            Jokes = new[] { joke, joke2, joke3 };
        }
    }

    void OnDisable()
    {
        if (TransportServer.Instance != null)
            TransportServer.Instance.OnPlayerMessageReceived -= OnPlayerMessageReceived;
    }

    private void OnPlayerMessageReceived(object sender, PlayerMessageEventArgs e)
    {
        switch (e.EventPlayerMessage.MessageType)
        {
            case MessageType.PLAYER_DEPLOY_TOMATO:
            {
                ThrowTomato();
                break;
            }

            case MessageType.PLAYER_DEPLOY_ROSE:
            {
                ThrowRose();
                break;
            }

            case MessageType.PLAYER_DEPLOY_RIMSHOT:
            {
                DoRimshot();
                break;
            }

            case MessageType.PLAYER_DEPLOY_TRUMPET:
            {
                DoTrumpet();
                break;
            }

            case MessageType.PLAYER_LAUGHED:
            {
                if (CurrentJoke != null)
                {
                    CurrentJoke.Points++;
                }

                break;
            }

            default:
            {
                break;
            }
        }
    }

    public AudioClip[] segueClips;
    public AudioSource seguePlayer;

    public float PlaySegueClip()
    {
        seguePlayer.clip = segueClips[Random.Range(0, segueClips.Length)];
        seguePlayer.Play();
        return seguePlayer.clip.length;
    }

    public AudioSource trumpetPlayer;
    
    [ContextMenu("Do trumpet")]
    private void DoTrumpet()
    {
        trumpetPlayer.Play();
    }

    public AudioSource rimshotPlayer;

    [ContextMenu("Do rimshot")]
    private void DoRimshot()
    {
        rimshotPlayer.Play();
    }

    [ContextMenu("Throw rose")]
    private void ThrowRose()
    {
        ThrowInstanceOfPrefab(RosePrefab);
    }

    public GameObject TomatoPrefab;
    public GameObject RosePrefab;
    public float MinThrowForce = 8f;
    public float MaxThrowForce = 12f;

    private void ThrowInstanceOfPrefab(GameObject prefab)
    {
        // We want an x offset which is off the screen, so between (-1,0) or (1, 2)
        var xOffset = UnityEngine.Random.Range(-1, 1);
        if (xOffset >= 0)
            xOffset += 1;

        var yOffset = 0.25f;

        var spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(xOffset, yOffset, -1));
        var instance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        var aimPort = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 2f));
        var throwDirection = (aimPort - spawnPosition).normalized;
        if (throwDirection.y < 0.2f)
        {
            throwDirection.y = 0.2f;
            throwDirection.Normalize();
        }
        
        // Throw the tomato upwards and towards the center of the screen
        instance.GetComponent<Rigidbody>().AddForce((aimPort - spawnPosition).normalized * UnityEngine.Random.Range(MinThrowForce, MaxThrowForce), ForceMode.Impulse);

    }

    [ContextMenu("Throw tomato")]
    private void ThrowTomato()
    {
        ThrowInstanceOfPrefab(TomatoPrefab);
    }

    [UsedImplicitly]
    public void SpeakAnnouncer(DialogOptionsTable dialog)
    {
        PlaybackSubtitle(dialog.GetRandomLine());
    }

    private void PlaybackSubtitle(string text)
    {
        StartCoroutine(PlaybackSubtitleCoroutine(text, 0.05f, 2.0f));
    }

    private IEnumerator PlaybackSubtitleCoroutine(string text, float delay, float postDelay)
    {
        int revealedIndex = 0;
        string prefix = String.Empty;
        string postfix = text;
        while (revealedIndex < text.Length)
        {
            prefix += postfix[0];
            postfix = postfix.Remove(0, 1);
            revealedIndex++;
            
            _subtitle.text = prefix + "<color=#00000000>" + postfix + "</color>";
            yield return new WaitForSeconds(delay);
        }

        yield return new WaitForSeconds(postDelay);
        _subtitle.text = string.Empty;
    }

    public void BeginTellingJokes()
    {
        StartCoroutine(TellJokesCoroutine());
    }

    private IEnumerator SpeakComedianCoroutine(string line)
    {
        comedian.IsTalking = true;
        yield return StartCoroutine(PlaybackSubtitleCoroutine(line, 0.05f, 0));
        _subtitle.text = line;
        comedian.IsTalking = false;
        yield return new WaitForSeconds(2.0f);
        _subtitle.text = string.Empty;
    }

    public Coroutine SpeakComedian(string line)
    {
        return StartCoroutine(SpeakComedianCoroutine(line));
    }

    public void SetComedianWalking()
    {
        comedian.BodyWalk();
    }

    public void SetComedianIdle()
    {
        comedian.BodyIdle();
    }

    private IEnumerator TellJokesCoroutine()
    {
        foreach (var joke in Jokes)
        {
            CurrentJoke = joke;
            AcceptingLaughs = false; // Don't accept laughs until we've at least started the punchline
            yield return SpeakComedian(joke.Setup);

            AcceptingLaughs = true;
            yield return SpeakComedian(joke.CompletedPunchline);

            float roll = Random.value;
            if (roll < 0.1f)
            {
                director.SetBool("Wideshot", true);
                yield return new WaitForSeconds(PlaySegueClip());
                director.SetBool("Wideshot", false);
                yield return new WaitForSeconds(0.2f);
            }
            else if(roll < 0.3f)
            {
                comedian.BodyTurn();
                yield return new WaitForSeconds(2.0f);
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            joke.AwardPlayerPoints();
        }
        
        yield return SpeakComedian(closingLines.GetRandomLine());

        SceneManager.LoadScene("Afterparty");
    }
}
