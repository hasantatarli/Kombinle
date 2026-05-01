using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Domain.Context
{
    public enum TagSource { Default, User, Catalog, Inferred }

    public record TagValue<T>(
        T Value,
        TagSource Source,
        double Confidence = 1.0,
        string? Evidence = null
    );
}
