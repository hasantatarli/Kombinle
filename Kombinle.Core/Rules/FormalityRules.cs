using Kombinle.Core.Domain;
using Kombinle.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Rules
{
    public class FormalityRules : IRule
    {
        public RuleResult Evaluate(Combination combination, Occasion occasion)
        {
            var expected = occasion.RequiredFormality;

            if (combination.Items.Any(x => x.Formality < expected))
            {
                return RuleResult.Fail("Formality seviyesi uygun değil");
            }

            return RuleResult.Pass();
        }
    }

}
