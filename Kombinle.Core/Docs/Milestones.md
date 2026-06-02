# Milestones

## Completed
- Real weather integration added through Open-Meteo
- Demo now supports city-based context loading
- Lightweight context summary bar added
- Advanced context override panel added
- Weather override flow stabilized
- Dynamic wardrobe profile loading added
- Wardrobe preview summary added
- Category-grouped wardrobe inspection drawer added
 
---

Date: 22/05/2026
- JSON-based wardrobe persistence pipeline implemented
- Wardrobe profiles migrated from hardcoded loader to JSON resources
- WardrobeProfileService added
- API wardrobe endpoints now use JSON-backed persistence
- Decision pipeline now consumes wardrobe JSON profiles
- BestPool deterministic rotation stabilized
- RotationAttempt behavior validated through integration tests
- RequestKey-based rotation reset behavior added
- Category catalog system introduced
- Category metadata moved to JSON catalog
- CategoryCatalogService added
- Wardrobe API responses enriched with category metadata
- Wardrobe drawer grouping now uses taxonomy groups
- Legacy occasion fallback factories removed
- Occasion catalog became the single source of truth
- CategorySemantics centralized:
  - Layer semantics
  - Top/Bottom semantics
  - Footwear semantics
  - Core/Support pair semantics
	
---

Date: 2026-05-25
## Taxonomy-Driven Generation Migration

### Completed
- Added provider-backed semantic taxonomy model
- Added slot-aware semantic provider access
- Introduced `allowedTraits` support
- Migrated Shoes generation to taxonomy-driven matching
- Migrated Top generation to taxonomy-driven matching
- Migrated Bottom generation to taxonomy-driven matching
- Added regression tests for trait-driven slot matching

### Semantic Separation
The engine now separates:
- Traits → semantic behavior
- Slots → outfit composition eligibility
- Groups → taxonomy/UI grouping

### Naming Cleanup
- `IsTopCategory` → `CanFillTopSlot`
- `IsBottomCategory` → `CanFillBottomSlot`
- `IsFootwearCategory` → `CanFillShoesSlot`

### OnePiece Migration
- Removed `DressPath` semantic trait
- Introduced `IsOnePiece()`
- Migrated generation filters away from direct `Category.Dress` checks

### UX Improvements
- Added distinction between:
  - missing soft anchor
  - weak-formality soft anchor
- Introduced `SOFT_ANCHOR_FORMALITY_WEAK`


---
Date: 2026-06-02

## Semantic Taxonomy Foundation Complete

Completed:
- allowedSlots matching
- semantic slot eligibility
- semantic provider architecture
- category catalog validation
- occasion semantic validation
- SemanticTraits registry
- SemanticSlotNames registry
- protection layer guardrails

Outcome:
Engine evolved from category-driven matching
towards semantic composition.