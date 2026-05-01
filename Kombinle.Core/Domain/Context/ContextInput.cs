using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Context
{
    public record ContextInput(
        Weather? Weather = null,
        Setting? Setting = null,
        TimeOfDay? Time = null
    );

    public enum Weather { Clear, Rain, Snow, Hot, Cold }
    public enum Setting { Indoor, Outdoor }
    public enum TimeOfDay { Day, Night }
}
