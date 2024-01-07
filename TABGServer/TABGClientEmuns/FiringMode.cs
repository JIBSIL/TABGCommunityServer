// FiringMode gotten from the TABG Client, on 11/26/2023, using ILspy
[Flags]
public enum FiringMode : byte
{
    None = 0,
    Semi = 1,
    Burst = 2,
    FullAutoStart = 4,
    FullAutoStop = 8,
    ContainsDirection = 0x10,
    RightGun = 0x20,
    WantsToBeSynced = 0x40,
    UseBulletEffect = 0x80
}
