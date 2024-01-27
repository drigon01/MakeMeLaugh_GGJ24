
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

    public GameObject wideCamera;
    public GameObject entryCamera;
    public GameObject closeupCamera;

    private UIDocument _uiHost;
    
    public static void BeginSet(IEnumerable<Joke> jokes)
    {
        Jokes = jokes.ToArray();
        SceneManager.LoadScene("StageScene", LoadSceneMode.Additive);
    }

    void OnEnable()
    {
        SceneManager.LoadScene("stage", LoadSceneMode.Additive);

        _uiHost = GetComponent<UIDocument>();

        PlaybackSubtitle("Ladies and gentlemen, please welcome to the stage, the star of tonight's entertainment, ComedyBot 5000!");
    }

    private void PlaybackSubtitle(string text)
    {
        _uiHost.rootVisualElement.Q<Label>("Subtitle").text = "<color=#00000000>" + text + "</color>";
        StartCoroutine(PlaybackSubtitleCoroutine(text, 0.05f));
    }

    private IEnumerator PlaybackSubtitleCoroutine(string text, float delay)
    {
        int revealedIndex = 0;
        string prefix = String.Empty;
        string postfix = text;
        while (revealedIndex < text.Length)
        {
            prefix += postfix[0];
            postfix = postfix.Remove(0, 1);
            revealedIndex++;
            
            _uiHost.rootVisualElement.Q<Label>("Subtitle").text = prefix + "<color=#00000000>" + postfix + "</color>";
            yield return new WaitForSeconds(delay);
        }
    }

    void OnDisable()
    {
        SceneManager.UnloadSceneAsync("stage");
    }
}
