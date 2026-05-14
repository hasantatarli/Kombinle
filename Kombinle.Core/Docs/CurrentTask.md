# CURRENT TASK

## Goal
Design and implement BestPool / deterministic rotation to reduce repeated-best perception while keeping the engine deterministic and explainable.

## Completed
- Real-user feedback collected from early demo sharing
- Season added to ContextDto / ContextInput
- Demo UI now sends Season
- Context-aware outerwear generation expanded:
  - Rain
  - Snow / Cold
  - Winter + Outdoor
  - Autumn + Outdoor + Night
- Layer semantic foundation added:
  - Comfort
  - Structure
  - Protection
- Layer intensity scoring added
- Summer + Indoor now prefers lighter outfits
- Winter + Outdoor now can generate and prefer protective outerwear
- HasOuterwear now considers anchor/layer categories, not only Slot.Outerwear

## In Progress
- BestPool / deterministic rotation design  “BestPool stabilized”
- Build a pool of strong, context-safe combinations instead of relying only on a single top-scored Best
- Keep current Best behavior unchanged in the first step; first expose/debug BestPool candidates
- Preserve deterministic behavior; no random selection

## Next Steps
5. Design BestPool / deterministic rotation
6. Reduce repeated-best perception without breaking determinism
7. Continue testing with male_extended_v1 wardrobe

## NOT DOING NOW
- Random outfit selection
- AI image recognition / wardrobe ingestion
- Instagram import
- Shopping / commerce integration
- Full personalization engine
- Modest / covered style implementation
- Accessory companion recommendation
- Large taxonomy/config refactor