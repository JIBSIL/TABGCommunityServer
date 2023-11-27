using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    // Token: 0x0200017D RID: 381
    [Flags]
    public enum TABGPlayerState : byte
    {
        // Token: 0x040009CC RID: 2508
        None = 0,
        // Token: 0x040009CD RID: 2509
        Buff = 1,
        // Token: 0x040009CE RID: 2510
        Transcended = 2
    }

}
