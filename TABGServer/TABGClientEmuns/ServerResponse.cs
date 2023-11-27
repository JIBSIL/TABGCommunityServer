// ServerResponse gotten from the TABG Client, on 11/23/2023, using ILspy
[Flags]
public enum ServerResponse : byte
{
    Error = 0,
    Accepted = 1,
    SquadDontFit = 2,
    SquadIsBiggerThanTeamSize = 4,
    DontHaveBooking = 8,
    MatchAlreadyStarted = 0x10,
    WrongPassword = 0x20
}