## Goal
Improve outfit intelligence using semantic reasoning while keeping
the engine deterministic, explainable and config-driven.

## In Progress

### Layer Compatibility Matrix v1

Goal:

Define compatibility rules between semantic layer roles.

Roles:

- Structure
- Comfort
- Protection

Initial focus:

- Structure + Protection
- Comfort + Protection
- Structure + Comfort

Implementation approach:

- Generation guardrails first
- Scoring adjustments only if necessary

## Next
1. Thermal Coherence v1 (Done)
   - Winter/Cold/Outdoor + light top penalty
   - Warm layer bonuses
   - Outfit thermal consistency


2. Layer Conflict Expansion
   - Hoodie + Formal Shirt penalty
   - Comfort vs Structure conflicts
   - Semantic layer compatibility

3. Future-Date Weather Support
   - Forecast-based context generation



## Recently Completed

### Thermal Coherence v1

- Light top detection
- Warm support detection
- Protection layer scoring
- Thermal consistency bonus
- Summer/Cold handling
- ContextDelta ranking priority
- Semantic slot cleanup
- Regression coverage


## NOT DOING NOW
- AI ranking
- User learning
- Multi-color garments
- Fabric/material semantics
- Expanded color matrix

