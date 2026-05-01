using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Presenting
{
    public sealed record DecisionMessage(
    string HeadlineTr,
    string HeadlineEn,
    string? SubtextTr,
    string? SubtextEn,
    int Priority
);
}
