// PacketContainerFlags gotten from the TABG Client, on 11/26/2023, using ILspy
[Flags]
public enum PacketContainerFlags : byte
{
    Nothing = 0,
    PlayerPosition = 1,
    PlayerRotation = 2,
    PlayerDirection = 4,
    CarPosition = 8,
    CarRotation = 0x10,
    CarInput = 0x20,
    All = 0x40
}
