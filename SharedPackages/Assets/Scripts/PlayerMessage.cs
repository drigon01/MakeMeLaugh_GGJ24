using System;
using System.Runtime.InteropServices;
using Unity.Collections;

[Serializable]
public struct PlayerMessage
{
  public string PlayerUuid;
  public MessageType MessageType;
  public string MessageContent;

  public PlayerMessage(string playerUuid, MessageType messageType, string playerSubmission = null)
  {
    this.PlayerUuid = playerUuid;
    this.MessageType = messageType;
    this.MessageContent = playerSubmission;
  }

  public static NativeArray<byte> GetBytes(PlayerMessage playerMessage)
  {
    int size = Marshal.SizeOf(playerMessage);
    byte[] arr = new byte[size];
    NativeArray<byte> nativeArrBytes;
    IntPtr ptr = IntPtr.Zero;
    try
    {
      ptr = Marshal.AllocHGlobal(size);
      Marshal.StructureToPtr(playerMessage, ptr, true);
      Marshal.Copy(ptr, arr, 0, size);
      nativeArrBytes = new NativeArray<byte>(arr, Allocator.Temp);
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }

    return nativeArrBytes;
  }

  public static PlayerMessage FromBytes(NativeArray<byte> nativeArray)
  {
    var slice = new NativeSlice<byte>(nativeArray).SliceConvert<byte>();
    var bytes = new byte[slice.Length];
    slice.CopyTo(bytes);
    PlayerMessage playerMessage = new PlayerMessage();

    int size = Marshal.SizeOf(playerMessage);
    IntPtr ptr = IntPtr.Zero;
    try
    {
      ptr = Marshal.AllocHGlobal(size);

      Marshal.Copy(bytes, 0, ptr, size);

      playerMessage = (PlayerMessage)Marshal.PtrToStructure(ptr, playerMessage.GetType());
    }
    finally
    {
      Marshal.FreeHGlobal(ptr);
    }
    return playerMessage;
  }
}


public enum JokeState
{
  Setup,
  Punchline,
  Done,
}

// Round Messages
public struct PlayerSetupResponse
{
  public PlayerSetupResponse(string setup, string jokeId)
  {
    Setup = setup;
    JokeId = jokeId;
  }
  public string Setup;
  public string JokeId;
}

public struct PlayerPunchlineResponse
{
  public PlayerPunchlineResponse(string punchlineSegment, string jokeId)
  {
    PunchlineSegment = punchlineSegment;
    JokeId = jokeId;
  }
  public string PunchlineSegment;
  public string JokeId;
}

public struct PlayerPunchlineRequest
{
  public PlayerPunchlineRequest(string setup, string punchlineTemplate, string jokeId)
  {
    Setup = setup;
    PunchlineTemplate = punchlineTemplate;
    JokeId = jokeId;
  }
  public string Setup;
  public string PunchlineTemplate;
  public string JokeId;
}

public struct PlayerSetupRequest
{
  public PlayerSetupRequest(string setupTemplate, string jokeId)
  {
    SetupTemplate = setupTemplate;
    JokeId = jokeId;
  }
  public string SetupTemplate;
  public string JokeId;
}

