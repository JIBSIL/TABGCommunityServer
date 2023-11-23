using System;

// Token: 0x02000173 RID: 371
[Flags]
public enum ServerResponse : byte
{
    // Token: 0x04000995 RID: 2453
    Error = 0,
    // Token: 0x04000996 RID: 2454
    Accepted = 1,
    // Token: 0x04000997 RID: 2455
    SquadDontFit = 2,
    // Token: 0x04000998 RID: 2456
    SquadIsBiggerThanTeamSize = 4,
    // Token: 0x04000999 RID: 2457
    DontHaveBooking = 8,
    // Token: 0x0400099A RID: 2458
    MatchAlreadyStarted = 16
}
