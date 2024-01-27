using System;
using System.Runtime.InteropServices;
using Unity.Collections;

public struct PlayerMessage <T>
{
    public string PlayerUuid;
    public ClientToServerMessageType MessageType;
    public T PlayerSubmission;

    public PlayerMessage(string playerUuid, ClientToServerMessageType messageType, T playerSubmission)
    {
        this.PlayerUuid = playerUuid;
        this.MessageType = messageType;
        this.PlayerSubmission = playerSubmission;
    }

    public static NativeArray<byte> GetBytes(PlayerMessage<T> playerMessage)
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

        // var byteArr = new NativeArray<byte>(arr, Allocator.Temp);
        // var slice = new NativeSlice<byte>(byteArr).SliceConvert<byte>();

        // UnityEngine.Debug.Assert(arr.Length == slice.Length);
        // slice.CopyTo(nativeArrBytes);

        return nativeArrBytes;
    }

    public static PlayerMessage<string> FromBytes(NativeArray<byte> nativeArray)
   {
        var slice = new NativeSlice<byte>(nativeArray).SliceConvert<byte>();
        var bytes = new byte[slice.Length];
        slice.CopyTo(bytes);
        // PlayerMessage<string> playerMessage = new PlayerMessage<string>();
        
        // return playerMessage;
        PlayerMessage<string> playerMessage = new PlayerMessage<string>();
        
        int size = Marshal.SizeOf(playerMessage);
        IntPtr ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(size);
        
            Marshal.Copy(bytes, 0, ptr, size);
        
            playerMessage = (PlayerMessage<string>)Marshal.PtrToStructure(ptr, playerMessage.GetType());
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        return playerMessage;
    }
}
