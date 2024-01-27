using System;
using UnityEngine;

// Custom EventArgs class with additional information
public class PlayerMessageEventArgs : EventArgs
{
    public PlayerMessage EventPlayerMessage { get; }

    public PlayerMessageEventArgs(PlayerMessage playerMessage)
    {
        EventPlayerMessage = playerMessage;
    }
}