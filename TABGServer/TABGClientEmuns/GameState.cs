// GameState gotten from the TABG Client, on 11/23/2023, using ILspy
public enum GameState : byte
{
    WaitingForPlayers,
    CountDown,
    Flying,
    Started,
    Ended,
    OpenDoors,
    RoundOver,
    Intermission,
    Voting,
    VotingOver
}