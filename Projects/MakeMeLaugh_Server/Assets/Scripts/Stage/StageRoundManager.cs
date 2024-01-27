
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    
    public Joke CurrentJoke { get; private set; }
    
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
            joke.AddPunchlineSegment(new PunchlineSegment(testPlayer){Text = "To get to"});
            joke.AddPunchlineSegment(new PunchlineSegment(testPlayer) { Text = "the other side!" });
            Jokes = new[] { joke };
        }
    }

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

    public Coroutine SpeakComedian(string line)
    {
        return StartCoroutine(PlaybackSubtitleCoroutine(line, 0.05f, 1.0f));
    }

    private IEnumerator TellJokesCoroutine()
    {
        foreach (var joke in Jokes)
        {
            CurrentJoke = joke;
            yield return SpeakComedian(joke.GetSetup());
            yield return SpeakComedian(joke.CompletedPunchline);
            yield return new WaitForSeconds(2.0f);
        }
        
        yield return SpeakComedian("You've been a wonderful audience. Good night!");
    }
}
