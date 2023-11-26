using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    // Token: 0x0200017F RID: 383
    [Flags]
    public enum PacketContainerFlags : byte
    {
        // Token: 0x040009D4 RID: 2516
        Nothing = 0,
        // Token: 0x040009D5 RID: 2517
        PlayerPosition = 1,
        // Token: 0x040009D6 RID: 2518
        PlayerRotation = 2,
        // Token: 0x040009D7 RID: 2519
        PlayerDirection = 4,
        // Token: 0x040009D8 RID: 2520
        CarPosition = 8,
        // Token: 0x040009D9 RID: 2521
        CarRotation = 16,
        // Token: 0x040009DA RID: 2522
        CarInput = 32,
        // Token: 0x040009DB RID: 2523
        All = 64
    }

}
