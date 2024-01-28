public class WaitingInfo
{
  public WaitingInfo(string title, string text, string percent)
  {
    Title = title;
    Text = text;
    Percent = percent;
  }

  public string Title { get; }


  public string Text { get; }
  public string Percent { get; }

}