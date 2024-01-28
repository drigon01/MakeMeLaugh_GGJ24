using System;
using System.Collections.Generic;

public class PunchlineSegment
{
    public PunchlineSegment(Player author)
    {
        Author = author;
        IsDone = false;
        Text = "";
    }
    
    public string Text { get; set; }
    public Player Author { get; }
    public bool IsDone { get; set; }
    
    public override string ToString()
    {
        return Text;
    }
}


public class Joke
{
    public static string[] jokeTemplates = { 
        "I told my _BLANK_ I needed a break.", 
        "Why did the _BLANK_ cross the road?",
        "My _BLANK_ used to chase people on a bike.",
        "I'm reading a book on _BLANK_.",
        "I tried to catch some _BLANK_ yesterday.",
    };
    
    public static int MAX_SEGMENTS = 3;
    public List<PunchlineSegment> PunchlineSegments { get; }
    public string Setup { get; set; }
    public string JokeId { get; }
    
    private Player m_author;
    private int m_points;
    
    public Joke(Player author, List<Player> coauthors)
    {
        JokeId = Guid.NewGuid().ToString();
        m_author = author;
        PunchlineSegments = new List<PunchlineSegment>();
        foreach (var coauthor in coauthors)
        {
            PunchlineSegments.Add(new PunchlineSegment(coauthor));   
        }
        
        Setup = GetSetupTemplate();
    }
    
    public void AddPunchlineSegment(PunchlineSegment segment)
    {
        PunchlineSegments.Add(segment);
    }
    
    public void AddPunchlineSegmentText(string segmentText)
    {
        // find the first segment that is not done
        foreach (var s in PunchlineSegments)
        {
            if (!s.IsDone)
            {
                s.Text = segmentText;
                return;
            }
        }
    }

    public bool IsPunchlineComplete()
    {
        foreach (var segment in PunchlineSegments)
        {
            if (!segment.IsDone)
            {
                return false;
            }
        }

        return true;
    }

    // TODO: Implement
    string GetSetupTemplate()
    {
        // get random joke setup
        return jokeTemplates[UnityEngine.Random.Range(0, jokeTemplates.Length)];
        
    }

    public string CompletedPunchline => string.Join(" ", PunchlineSegments);
    
    public string CompletedJoke => Setup + " " + CompletedPunchline;

    public void RunNextPunchlineSegment()
    {
        foreach(var segment in PunchlineSegments)
        {
            if (!segment.IsDone)
            {
                segment.Author.SendPunchline(this);
                // Pain
                segment.IsDone = true;
                return;
            }
        }
    }

    public string CurrentPunchlineTemplate()
    {
        string template = "";
        foreach(var segment in PunchlineSegments)
        {
            if(segment.IsDone)
            {
                template += segment.ToString();
            }
            else
            {
                template += "_BLANK_";
                return template;
            }
        }
        return template;
    }
}