using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    [Flags]
    public enum DrivingState : byte
    {
        // Token: 0x040009C7 RID: 2503
        None = 0,
        // Token: 0x040009C8 RID: 2504
        InsideCar = 1,
        // Token: 0x040009C9 RID: 2505
        Driving = 2,
        // Token: 0x040009CA RID: 2506
        Slow = 4
    }

}
