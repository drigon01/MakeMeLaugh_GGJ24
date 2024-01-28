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
        "What do you call a _BLANK_ with no legs?",
        "I went to the _BLANK_ the other day.",
        "I'm thinking of opening a _BLANK_.",
        "_BLANK_ is my favorite subject.",
        "Why did the _BLANK_ cross the road?",
        "Have you heard about the _BLANK_ who walked into a bar?",
        "What did one _BLANK_ say to the other?",
        "Why don't _BLANK_ ever play hide and seek?",
        "Did you hear about the _BLANK_ who tried to become a comedian?",
        "What's _BLANK_'s favorite type of music?",
        "Why did the _BLANK_ break up with the calendar?",
        "How does a _BLANK_ answer the phone?",
        "Why did the _BLANK_ bring a suitcase to the zoo?",
        "What do you call a _BLANK_ riding a bicycle?",
        "Why did the _BLANK_ bring a suitcase to the beach?",
        "Have you heard about the _BLANK_ who learned to juggle?",
        "What did one _BLANK_ say to the other _BLANK_?",
        "Why don't _BLANK_ ever go to school?",
        "Did you hear about the _BLANK_ who opened a spa?",
        "What's _BLANK_'s favorite kind of sandwich?",
        "Why did the _BLANK_ break up with the dictionary?",
        "How does a _BLANK_ prepare for a marathon?",
        "Why did the _BLANK_ bring a ladder to the library?",
        "What do you call a _BLANK_ with a magic wand?",
        "Why did the _BLANK_ take a nap in the refrigerator?",
        "Have you heard about the _BLANK_ who became a detective?",
        "What did one _BLANK_ say to the other _BLANK_ at the concert?",
        "Why don't _BLANK_ ever go on roller coasters?",
        "Did you hear about the _BLANK_ who won the lottery?",
        "What's _BLANK_'s favorite board game?",
        "Why did the _BLANK_ bring a pencil to the art class?",
        "How does a _BLANK_ start a campfire?",
        "Why did the _BLANK_ wear sunglasses to the computer?",
        "What do you call a _BLANK_ with a superpower?",
        "Why did the _BLANK_ challenge a tornado to a dance-off?",
        "Have you heard about the _BLANK_ who invented a language for talking to squirrels?",
        "What did one _BLANK_ say to the other _BLANK_ during a pillow fight in outer space?",
        "Why don't _BLANK_ ever participate in thumb-wrestling tournaments with robots?",
        "Did you hear about the _BLANK_ who built a roller coaster on the moon for intergalactic tourists?",
        "What's _BLANK_'s preferred method of communication with dolphins?",
        "Why did the _BLANK_ break up with the parallel universe?",
        "How does a _BLANK_ train for competitive synchronized swimming with mermaids?",
        "Why did the _BLANK_ bring a trampoline to the comedy club?",
        "What do you call a _BLANK_ with a secret lair under a rainbow?",
        "Why did the _BLANK_ challenge a black hole to a staring contest?",
        "Have you heard about the _BLANK_ who taught quantum physics to rubber chickens?",
        "What did one time-traveling _BLANK_ say to another during a synchronized interpretative dance marathon?",
        "Why don't interdimensional _BLANK_ ever enter thumb-wrestling matches against invisible unicorns?",
        "Did you hear about the _BLANK_ who organized a spaghetti-eating contest for parallel universe spaghetti monsters?",
        "What's the preferred karaoke song of sentient _BLANK_ from the Andromeda Galaxy?",
        "Why did the _BLANK_ break up with the fifth dimension? It was just too one-dimensional.",
        "How does a hyper-intelligent _BLANK_ solve a Rubik's Cube underwater with a swarm of glow-in-the-dark jellyfish?",
        "Why did the _BLANK_ bring a fog machine to the silent meditation retreat?",
        "What do you call a _BLANK_ with a Ph.D. in interpretive dance and a minor in telekinesis?",
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