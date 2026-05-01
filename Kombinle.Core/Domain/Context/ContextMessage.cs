using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Context
{
    public enum MessageKind
    {
        ContextWarning = 0,
        WardrobeFeedback = 1
    }

    public record ContextMessage(
        MessageKind Kind,
        string TitleTr,
        string TitleEn,
        string DetailTr,
        string DetailEn
    );
}
