using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Context
{
    public class ContextResult
    {
        public int DeltaScore { get; set; }
        public List<string> WarningCodes { get; } = new();
        public List<string> Reasons { get; } = new();
        public List<ContextUserNote> UserNotes { get; } = new();
    }

}
