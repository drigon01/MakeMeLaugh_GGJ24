/// <summary>
/// Utility struct to store the waiting control's backing data
/// </summary>
public struct WaitingInfo
{
  public WaitingInfo(string title, string text, string percent = null)
  {
    Title = title;
    SubText = text;
    Percent = percent;
  }

  /// <summary>
  /// Title of the waiting action
  /// </summary>
  public string Title { get; }

  /// <summary>
  /// Sub text / details of the waiting action
  /// </summary>
  public string SubText { get; }

  /// <summary>
  /// Percentage is used to track waiting status
  /// </summary>
  public string Percent { get; }
}