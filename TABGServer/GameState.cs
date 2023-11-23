using System;

// Token: 0x02000179 RID: 377
public enum GameState : byte
{
    // Token: 0x040009B3 RID: 2483
    WaitingForPlayers,
    // Token: 0x040009B4 RID: 2484
    CountDown,
    // Token: 0x040009B5 RID: 2485
    Flying,
    // Token: 0x040009B6 RID: 2486
    Started,
    // Token: 0x040009B7 RID: 2487
    Ended,
    // Token: 0x040009B8 RID: 2488
    OpenDoors,
    // Token: 0x040009B9 RID: 2489
    RoundOver,
    // Token: 0x040009BA RID: 2490
    Intermission,
    // Token: 0x040009BB RID: 2491
    Voting,
    // Token: 0x040009BC RID: 2492
    VotingOver
}
