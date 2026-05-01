using Kombinle.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring
{
    public interface IScoreComponent
    {
        int Calculate(Combination combination, Occasion occasion);
    }

}
