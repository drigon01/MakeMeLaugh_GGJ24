using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ScoreScreenController : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _container;
    public VisualTreeAsset scoreListEntryAsset;

    public void Awake()
    {
        _document = GetComponent<UIDocument>();
        _container = _document.rootVisualElement.Q("ScoresContainer");
    }

    public void SetScores(IDictionary<string, int> scores)
    {
        var kvps = scores.OrderByDescending(kvp => kvp.Value).ToArray();

        for (int i = 0; i < kvps.Length; ++i)
        {
            var row = scoreListEntryAsset.Instantiate();
            row.Q<Label>(className:"player-name").text = kvps[i].Key;
            row.Q<Label>(className:"player-score").text = kvps[i].Value.ToString();
            row.AddToClassList($"score-{i}");
            _container.Add(row);
        }
    }

    void Start()
    {
        SetScores(GameManager.Instance.GetPlayers().ToDictionary(p => p.Name, p => p.Points));
    }
}
