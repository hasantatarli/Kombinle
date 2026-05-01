using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Alternatives
{
    public sealed record AlternativeMessage(
              string TitleTr,
              string TitleEn,
              string DetailTr,
              string DetailEn,
              int Priority,
              string? Group
          );
}
