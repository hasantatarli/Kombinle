using Kombinle.Api.Contracts;

namespace Kombinle.Api.Services;

public interface IDecisionService
{
    DecisionResponse Decide(DecisionRequest req);
}
