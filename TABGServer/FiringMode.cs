using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    [Flags]
    public enum FiringMode : byte
    {
        // Token: 0x0400098B RID: 2443
        None = 0,
        // Token: 0x0400098C RID: 2444
        Semi = 1,
        // Token: 0x0400098D RID: 2445
        Burst = 2,
        // Token: 0x0400098E RID: 2446
        FullAutoStart = 4,
        // Token: 0x0400098F RID: 2447
        FullAutoStop = 8,
        // Token: 0x04000990 RID: 2448
        ContainsDirection = 16,
        // Token: 0x04000991 RID: 2449
        RightGun = 32,
        // Token: 0x04000992 RID: 2450
        WantsToBeSynced = 64,
        // Token: 0x04000993 RID: 2451
        UseBulletEffect = 128
    }
}
