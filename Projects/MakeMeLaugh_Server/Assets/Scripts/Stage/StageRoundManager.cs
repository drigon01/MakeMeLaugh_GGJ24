
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(UIDocument))]
public class StageRoundManager : MonoBehaviour
{
    public static IEnumerable<Joke> Jokes { get; private set; }

    private UIDocument _uiHost;
    private Label _subtitle;
    public DialogOptionsTable closingLines;
    public ComedianController comedian;
    
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

        if (Jokes == null)
        {
            // Install some default jokes for now, for testing
            var testPlayer = new Player(0, "Nobody");

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
        comedian.HeadTalking();
        yield return StartCoroutine(PlaybackSubtitleCoroutine(line, 0.05f, 0));
        _subtitle.text = line;
        comedian.HeadIdle();
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
            yield return SpeakComedian(joke.GetSetup());

            AcceptingLaughs = true;
            yield return SpeakComedian(joke.CompletedPunchline);
            comedian.BodyTurn();
            
            yield return new WaitForSeconds(2.0f);
        }
        
        yield return SpeakComedian(closingLines.GetRandomLine());
    }
}
