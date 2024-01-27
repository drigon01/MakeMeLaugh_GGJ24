using System;
using System.Runtime.InteropServices;
using Unity.Collections;

[Serializable]
public struct PlayerMessage
{
    public string PlayerUuid;
    public MessageType MessageType;
    public string MessageContent;

    public PlayerMessage(string playerUuid, MessageType messageType, string playerSubmission)
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
