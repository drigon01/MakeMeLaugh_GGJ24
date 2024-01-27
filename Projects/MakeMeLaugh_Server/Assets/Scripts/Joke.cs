using System;
using System.Collections.Generic;

public class PunchlineSegment
{
    public PunchlineSegment(Player author)
    {
        m_author = author;
    }
    
    private string m_punchline ="";
    private Player m_author;
    private bool m_isDone = false;

    public bool IsDone()
    {
        return m_isDone;
    }
    
    public void SetIsDone(bool done)
    {
        m_isDone = done;
    }
    
    public Player GetAuthor()
    {
        return m_author;
    }
}


public class Joke
{
    public static int MAX_SEGMENTS = 4;
    private string m_setup;
    private List<PunchlineSegment> m_punchlineSegments;
    
    private Player m_author;
    private int m_points;
    
    public Joke(Player author, List<Player> coauthors)
    {
        m_author = author;
        m_punchlineSegments = new List<PunchlineSegment>();
        foreach (var coauthor in coauthors)
        {
            m_punchlineSegments.Add(new PunchlineSegment(coauthor));   
        }
        
        m_setup = GetSetupTemplate();
    }
    
    void AddPunchlineSegment(PunchlineSegment segment)
    {
        m_punchlineSegments.Add(segment);
    }

    public bool IsDone()
    {
        foreach (var segment in m_punchlineSegments)
        {
            if (!segment.IsDone())
            {
                return false;
            }
        }

        return true;
    }

    // TODO: Implement
    string GetSetupTemplate()
    {
        return "Why did the _BLANK_ cross the road?";
    }

    public string GetSetup()
    {
        return m_setup;
    }

    public void RunNextPunchlineSegment()
    {
        foreach(var segment in m_punchlineSegments)
        {
            if (!segment.IsDone())
            {
                segment.GetAuthor().SendPunchline(m_setup, m_punchlineSegments);
                // Pain
                segment.SetIsDone(true);
                return;
            }
        }
    }
}