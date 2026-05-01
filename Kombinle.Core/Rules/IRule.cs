using Kombinle.Core.Domain;
using Kombinle.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Rules
{
    public interface IRule
    {
        RuleResult Evaluate(Combination combination, Occasion occasion);
    }

}
