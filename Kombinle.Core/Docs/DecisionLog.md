# Decision Log

---

# 🔑 High Confidence Decisions (Core)

* Config-Driven Growth
* Occasion Definitions Should Be JSON-Based
* Messages Must Come From Catalogs (No Hardcode)
* Anchor Is a Separate Domain Concept
* Outfit-Level Rules Must Consider Anchor + SlotToItem
* Multi-Path Occasion Support (CombinationModes)
* Dress and TopBottom Paths Must Not Mix
* RecommendedAlternative Only for Risky Results
* User-Facing Raw Scores Should Not Be Shown
* System Must Be Category-Based (Not Gender-Based)
* Color Harmony Scoring Should Be Capped

---

# 📌 Architectural Tensions

## Config vs Code Authority

### Description

Config-driven growth hedeflenirken bazı kritik kurallar kod içinde enforce ediliyor.

### Risk

Yeni config eklendiğinde engine bu kuralları enforce etmezse tekrar hibrit veya invalid kombinler üretilebilir.

### Open Question

Kuralların ana kaynağı ne olmalı?

* Config mi?
* Engine mi?
* Hybrid model mi?

### Status

Open

## Category Model vs Path-Based Behavior

### Description

Model kategori bazlı ilerliyor ancak bazı davranışlar (özellikle Dress) özel path olarak ele alınıyor.

### Risk

Yeni kategoriler (Jumpsuit, Set vb.) geldiğinde sistemin nasıl genişleyeceği belirsiz.

### Open Question

Sistem tamamen generic mi olmalı yoksa belirli "first-class paths" mı korunmalı?

### Status

Open

---

# 📘 Full Decision Log

---


## Context Must Affect Generation and Scoring

### Context

User feedback showed that weather/season/setting changes were not meaningfully changing recommendations.

Examples:
- Summer + Indoor still recommended layered outfits
- Winter + Outdoor did not generate coat candidates
- Outdoor feedback incorrectly claimed missing outerwear even when Jacket existed as Anchor

### Decision

Context must affect both:

- candidate generation
- scoring/ranking

Not only warnings or final messages.

### Implementation

- Season added to context
- Outerwear generation expanded beyond Rain
- Winter + Outdoor can include protective outerwear
- Summer + Indoor penalizes layer intensity
- HasOuterwear now evaluates garment layer behavior, not only Slot.Outerwear

### Reason

A recommendation engine must react to the actual situation, not only occasion.

### Confidence

High

---
## Layer Semantics Introduced

### Context

Categories like Hoodie, Jacket, Jacket and Coat were all treated too similarly.

This caused weak or confusing recommendations such as:
- Hoodie being treated as rain-appropriate
- Jacket not being counted as outerwear if used as Anchor
- Coat not being generated for Winter + Outdoor

### Decision

Layer categories now have semantic roles:

- Comfort
- Structure
- Protection

Layer intensity is used for context suitability.

### Reason

Layer behavior depends on purpose, not only category name.

### Confidence

High

---

## Demo-First Product Validation

### Context

Engine v1 behavior became stable enough to be tested by a non-technical user.

### Decision

Before adding database, weather API, image recognition or commerce integrations, a simple demo UI will be used for validation.

### Reason

The product should first prove that:

- The recommendation is understandable
- The output feels useful
- The user can judge the result without Postman or technical JSON
- Feedback can be collected before expanding architecture

### Impact

- Static wardrobe profiles will be used temporarily
- Demo UI will focus on clarity rather than full functionality
- Advanced features are deferred

### Confidence

High

---

## Static Wardrobe Profiles for Demo

### Context

Manual Postman item entry was slowing testing and creating inconsistent scenarios.

### Decision

Demo uses four static wardrobe profiles:

- `female_basic_v1`
- `female_balanced_v1`
- `male_basic_v1`
- `male_balanced_v1`

### Reason

This allows controlled testing across occasions and contexts without introducing database complexity too early.

### Confidence

High

---

## Weather API Deferred

### Context

Weather affects outfit decisions, but integrating a weather provider would introduce location, API, caching and fallback complexity.

### Decision

Weather API is deferred.

Demo uses manual context selection:

- Weather
- Setting
- TimeOfDay

### Reason

The current validation goal is product usefulness, not external weather automation.

### Confidence

High

---

## Formality Should Be Target-Based, Not Minimum-Based

### Context

Önceki modelde `requiredFormality` minimum eşik gibi kullanılıyordu.

Bu yaklaşımda:

* Casual occasion’da Formal parçalar avantajlı kalabiliyordu
* Smart Casual / Casual senaryolarda sistem fazla formal kombinler öneriyordu
* Gardıropta casual parçalar olsa bile seçim formal tarafa kayabiliyordu

### Decision

`requiredFormality` artık minimum eşik değil, hedef formality olarak ele alınacak.

### Reason

Occasion şunu ifade eder:

> Bu ortam için hedeflenen stil seviyesi nedir?

Bu nedenle:

* Casual occasion → Casual parçalar en yakın eşleşme
* Smart occasion → Smart parçalar en yakın eşleşme
* Formal occasion → Formal parçalar en yakın eşleşme

Formal parça, casual occasion’da otomatik olarak “daha iyi” sayılmamalıdır.

### Implementation

Scoring tarafında formality distance hesaplanır:

* exact match → olumlu sinyal
* 1 seviye sapma → küçük ceza
* 2 seviye sapma → daha büyük ceza

Supporter selection tarafında:

* `g.Formality >= requiredFormality` yaklaşımı kaldırıldı
* Target formality’ye en yakın parçalar önce sıralanır

### Impact

* Smart Casual Dinner senaryosunda casual parçalar candidate pool’a girebilir
* Formal bias kırılır
* Occasion davranışı ürün gerçekliğine yaklaşır

### Confidence

High

---

## Optional Outerwear Should Not Automatically Become Core Outfit

### Context

Optional outerwear primary kombine otomatik ekleniyordu.

Bu durum özellikle Indoor / Clear senaryolarda:

* Coat/Jacket’ın gereksiz şekilde kombine girmesine
* Skorun renk uyumu üzerinden şişmesine
* Best outfit’in fazla katmanlı görünmesine

neden oldu.

### Decision

Optional `Outerwear`, primary/core kombin içine otomatik eklenmemeli.

### Reason

Outerwear her zaman core outfit değildir.

Bazı senaryolarda:

* dışarı çıkarken giyilebilir
* mekânda çıkarılabilir
* layer olarak sunulabilir

Bu yüzden core outfit scoring’ini doğrudan şişirmemelidir.

### Implementation

`BuildPrimary(...)` içinde optional `Outerwear` primary candidate’a otomatik eklenmez.

### Impact

* Indoor / Clear senaryolarda gereksiz coat/jacket kullanımı azaldı
* Smart Casual Dinner çıktısı daha sade ve gerçekçi hale geldi
* Layer-aware presentation için zemin hazırlandı

### Confidence

High

---

## Same Garment Must Not Be Used as Both Anchor and Outerwear

### Context

Bazı candidate’larda aynı garment hem Anchor hem Outerwear olarak görünebiliyordu.

Örnek:

* Anchor: Jacket-Navy-Formal
* Outerwear: Jacket-Navy-Formal

### Decision

Aynı garment aynı candidate içinde iki farklı rolde kullanılmamalıdır.

### Reason

Bu durum:

* duplicate semantic role yaratır
* color scoring’i şişirir
* outfit açıklamasını bozar
* kullanıcıya aynı parçayı iki kez gösterme riskini artırır

### Implementation

Supporter pool oluşturulurken anchor ile aynı garment pool’dan çıkarılır.

### Impact

* Anchor + Outerwear duplicate problemi engellendi
* Scoring daha dengeli hale geldi

### Confidence

High

---

## Layer-Removed Alternative Should Have Its Own Reason

### Context

Best kombin jacket/Jacket içerdiğinde, ceketsiz alternatif generic reason ile açıklanıyordu.

Örnek:

* Best: Jacket + Blouse + Pants + Shoes
* Alternative: Blouse + Pants + Shoes

Önceki reason:

* `ALT_ITEM_SWAP_VARIATION`

### Decision

Anchor/layer kaldırılmış alternatif için özel reason kullanılmalı:

* `ALT_LAYER_REMOVED`

### Reason

Bu alternatif sıradan bir item swap değildir.

Kullanıcı açısından anlamı:

> Aynı kombini ceketsiz daha sade ve rahat kullanabilirsin.

### Impact

* Alternative explanation daha anlamlı hale geldi
* Smart Casual / Casual senaryolarda UX iyileşti

### Confidence

High

---

## Wardrobe Gap Detection Should Be Separate From Commerce

### Context

Ürün vizyonunda, kullanıcı dolabında eksik parça varsa ileride partner firmalara yönlendirme yapılması planlanıyor.

Örnek:

* casual shoes eksik
* casual bottom eksik
* smart casual için uygun top eksik

### Decision

Engine doğrudan ürün/marka önermemeli.

Ayrım:

* Engine → wardrobe gap tespit eder
* Commerce layer → bu gap’i ürün önerisine çevirir

### Reason

Bu ayrım engine’i temiz tutar.

Engine’in görevi:

> Bu occasion için dolapta hangi fırsat/eksik var?

Commerce katmanının görevi:

> Bu eksik için hangi partner ürünleri önerilebilir?

### Implementation

Yeni kavramlar:

* `WardrobeGap`
* `WardrobeGapEngine`
* API response içinde `wardrobeGaps`

### Impact

* Commerce bridge için mimari temel atıldı
* Engine marka/ürün bağımlılığı kazanmadan ürün değerini artırabilir

### Confidence

High

---

## Config-Driven Growth

### Context

Yeni feature ve occasion eklerken sistemin hard-coded mı yoksa config-driven mı büyüyeceği netleştirildi.

### Decision

Sistem mümkün olduğunca config-driven büyütülecek.

### Reason

Kod değişmeden büyüme ve esneklik sağlamak.

### Confidence

Yüksek

---

## Occasion Definitions Should Be JSON-Based

### Context

Yeni occasion eklerken kod değişikliği gerekip gerekmediği sorgulandı.

### Decision

Yeni occasion’lar JSON/config üzerinden tanımlanmalı.

### Reason

Sistem zaten config-driven yapıya uygun.

### Confidence

Yüksek

---

## Messages Must Come From Catalogs

### Context

Mesaj yönetiminde tutarlılık ihtiyacı.

### Decision

Tüm kullanıcı mesajları catalog (JSON) üzerinden yönetilmeli.

### Reason

Ürün dili ve maintainability.

### Confidence

Yüksek

---

## Anchor Is a Separate Domain Concept

### Context

Anchor’ın SlotToItem içinde tutulması duplicate sorunlara yol açtı.

### Decision

Anchor ayrı bir domain alanı olarak tutulmalı.

### Reason

Daha temiz ve doğru modelleme.

### Confidence

Yüksek

---

## Outfit-Level Rules Must Consider Anchor + SlotToItem

### Context

Anchor ayrıldıktan sonra bazı kurallar anchor’ı görmemeye başladı.

### Decision

Outfit-level kurallar Anchor + SlotToItem birlikte değerlendirmeli.

### Reason

Eksik değerlendirme hatalarını önlemek.

### Confidence

Yüksek

---

## RecommendedAlternative Only for Risky Results

### Context

Alternatiflerin ne zaman highlight edileceği belirsizdi.

### Decision

Sadece riskli (Poor) durumlarda highlight edilmeli.

### Reason

Kullanıcıyı doğru yönlendirmek.

### Confidence

Yüksek

---

## Alternative Reasons Must Be Short and De-Duplicated

### Context

Alternative explanation’lar gürültülüydü.

### Decision

* Max 2 reason
* Dedup uygulanmalı
* Öncelik sırası olmalı

### Reason

Daha temiz UX.

### Confidence

Yüksek

---

## Soft Warnings Must Not Be Treated as Hard Risks

### Context

Soft warning’ler fazla sert görünüyordu.

### Decision

Soft ve hard warning dili ayrılmalı.

### Reason

Doğru ürün tonu.

### Confidence

Yüksek

---

## No-Best Response Should Use Product Language

### Context

Fallback mesajlar teknik kalıyordu.

### Decision

No-best response ürün diliyle verilmeli.

### Reason

Kullanıcı deneyimi.

### Confidence

Yüksek

---

## User-Facing Raw Scores Should Not Be Shown

### Context

Score gösterimi tartışıldı.

### Decision

Score kullanıcıya gösterilmemeli.

### Reason

Yanıltıcı kesinlik hissi yaratır.

### Confidence

Yüksek

---

## System Must Be Category-Based, Not Gender-Based

### Context

Modelin cinsiyet bazlı olmaması gerektiği vurgulandı.

### Decision

Kategori bazlı sistem tercih edildi.

### Reason

Daha esnek ve genişletilebilir.

### Confidence

Yüksek

---

## Dress Support Should Be a First-Class Path

### Context

Dress capability kaybolmuştu.

### Decision

Dress ayrı bir path olarak desteklenmeli.

### Reason

Önceden var olan güçlü bir domain davranışı.

### Confidence

Yüksek

---

## Multi-Path Occasion Support

### Context

Tek slot set yetersiz kaldı.

### Decision

Occasion’lar multi-path desteklemeli (CombinationModes).

### Reason

Gerçek hayattaki kombin çeşitliliği.

### Confidence

Yüksek

---

## Dress and TopBottom Paths Must Not Mix

### Context

Hibrit kombinler üretildi.

### Decision

Bu path’ler aynı candidate içinde karışamaz.

### Reason

Anlamsız kombinleri engellemek.

### Confidence

Yüksek

---

## Wedding Flexible Should Support Multiple Paths

### Context

Gerçekçi wedding senaryosu ihtiyacı.

### Decision

Hem Dress hem TopBottom desteklenmeli.

### Reason

Gerçek kullanım senaryosu.

### Confidence

Yüksek

---

## Large Wardrobe Handling Is Deferred

### Context

50+ item senaryosu gündeme geldi.

### Decision

Şimdilik ertelendi.

### Reason

Öncelik core engine stabilizasyonu.

### Confidence

Yüksek

---

## Development Should Be Iterative and Safe

### Context

Değişiklikler başka yerleri bozdu.

### Decision

Küçük ve testli adımlarla ilerlenmeli.

### Reason

Regresyonları önlemek.

### Confidence

Yüksek

---

## Color Harmony Scoring Should Be Capped

### Context

Color harmony scoring was previously based on pairwise relationships between all items in an outfit.  
This caused combinations with more items (e.g., Top+Bottom+Shoes) to accumulate disproportionately high scores compared to structurally simpler outfits (e.g., Dress-based combinations).

### Problem

- Pair-based scoring scaled linearly (or worse) with item count
- Extra compatible items inflated scores ("pair explosion")
- Dress-based outfits were systematically disadvantaged due to fewer possible pairs
- The system favored “more combinations” over “better outfits”

### Decision

- Positive color harmony contribution is **capped / diminished after a threshold**
- Only the first few meaningful pair relations contribute full value
- Additional compatible pairs contribute **reduced value**
- Color clash penalties remain **fully applied and uncapped**

Additionally:

- Dress-based combinations receive a small **structure-level bonus**
- This reflects that Dress inherently represents a complete Top+Bottom structure

### Reason

To ensure:

- Fair comparison between different combination paths (Dress vs TopBottom)
- Scoring reflects **outfit quality**, not **pair count**
- Extra items improve completeness, but do not dominate ranking

### Impact

- Prevents score inflation from additional items (e.g., shoes, accessories)
- Produces more balanced ranking across paths
- Improves explainability of scoring
- Aligns system behavior with product goal: decision engine, not pair counter

### Confidence

High

## Color Compatibility and Pair Weighting Must Be Domain-Aware

### Context

Initial color scoring relied heavily on pair counts. This caused outfits with more item pairs to gain disproportionate score advantages.

### Decision

Color scoring now separates two concepts:

* Pair relation importance
* Color compatibility

Pair relation examples:

* Core pair: Top + Bottom
* Support pair: Bottom + Shoes, Dress + Shoes
* Weak pair: Top + Shoes

Color compatibility levels:

* StrongMatch
* Acceptable
* WeakMatch
* Clash

### Reason

Outfit quality should not be determined only by the number of possible item pairs.  
A pair should contribute meaningfully only when both its relationship and color compatibility support the outfit.

### Impact

* Prevents pair-count score inflation
* Makes Dress and TopBottom comparison fairer
* Allows color behavior to be tuned from config
* Keeps scoring deterministic and explainable

### Confidence

High

---
## 2026-04-XX — Layer Model & Context-Aware Generation

### Decision 1 — Layer Separation

Outfit yapısı ikiye ayrıldı:

- CoreItems
- Layers

Reason:
- Aynı kombin farklı kullanım senaryolarına sahip olabilir
- UX daha doğal hale gelir

---

### Decision 2 — Alternative Filtering Rule

Sadece layer kaldırılmış kombinler alternatif sayılmaz.

Reason:
- Bu durum aslında aynı kombinin varyasyonu
- Alternatif listesi daha anlamlı hale gelir

---

### Decision 3 — Context-Aware Outerwear

Outerwear sadece context gerektiriyorsa primary'ye dahil edilir.

Initial rule:
- Rain → include outerwear

Future:
- Temperature / feelsLike ile genişletilecek

---

### Decision 4 — Wardrobe Feedback Priority

Feedback seçiminde sadece frequency değil, semantic priority kullanılır.

Priority order:
- RAIN_* > SOFT_* > OUTDOOR_*

Reason:
- Kullanıcıya en kritik problem gösterilmeli


---
## BestPool and Deterministic Rotation

### Context
Real-user feedback showed that the demo could feel repetitive because the same top-ranked outfit was repeatedly recommended.

### Decision
A BestPool concept was introduced.

BestPool is an internal elite-quality candidate pool, not a user-visible list.

Rules:
- Built from ranked combinations
- Candidate score must be at least 75% of Best score
- Candidate must not have worse ContextDelta than Best
- Candidate must not be hard-failed
- Candidate must have meaningful difference
- Existing ranking order is preserved
- Max pool size is capped
- Random selection is not used

Rotation behavior:
- API accepts `rotationAttempt`
- `rotationAttempt = 0` returns the first BestPool candidate
- Repeated “Kombin Öner” clicks increase `rotationAttempt`
- Selected Best is `BestPool[rotationAttempt % BestPool.Count]`
- Alternatives are not promoted to Best
- BestPool is not shown directly to the user

### Reason
This reduces repeated-best perception while preserving deterministic and explainable behavior.

### Non-goals
- No personalization yet
- No persistence/history
- No randomization
- No user choice learning yet
- Alternatives are not used as rotation candidates

### Confidence
High

---

## Occasion Config Audit and Category Boundaries

### Context
Smart/Casual recommendations were being distorted because some occasion configs were too restrictive or had misleading target formality.

### Decision
Occasion `allowedCategories` should act as domain guardrails, not overly strict styling definitions.

Changes:
- `office_business_casual` target formality set to `Smart`
- `smart_casual_dinner` target formality set to `Smart`
- Smart/Casual occasions allow broader valid categories such as `Tshirt`, `Jeans`, `Sneakers`
- Formal work occasions allow `Jacket` as an anchor
- `wedding_formal_flexible` supports optional `Outerwear`

### Reason
The generator should not block valid wardrobe items too early. Generation defines acceptable boundaries; scoring decides which candidate is best.

### Non-goals
- No category catalog refactor yet
- No database-backed taxonomy yet
- No dynamic category creation

### Confidence
High

---

## 2026-05-15 — Context Notes & Category Semantics Cleanup

### Context Notes Pipeline
- Added first-class `ContextUserNotes` flow from `ContextScoringService`
  through `ScoredCombination` → `ResponseMapper` → API response.
- Context guidance is now separated from internal scoring reasons.
- Demo UI now renders contextual notes independently from generic “Why?” messaging.

### Category Semantics Refactor
- Removed ambiguous `Blazer` semantic separation.
- Standardized:
  - `Jacket` → structured/smart jacket family
  - `LightOuterwear` → seasonal/light protective outerwear
  - `Coat` → heavy outerwear
- Prevented confusing “double jacket” combinations in UI semantics.

### Context Behavior Improvements
- Rain + suede scenarios now prefer safer alternatives automatically
  instead of selecting risky combinations and warning afterwards.
- `RecommendedAlternative` promotion remains reserved for stronger
  context failures (`Poor` health).

### Rotation & Alternative Stability
- Deterministic rotation behavior stabilized using BestPool cycling.
- Alternative outputs now remain semantically cleaner across repeated requests.

### Product Direction
- Engine phase considered mostly stabilized.
- Focus shifts toward MVP productization:
  wardrobe management, persistent profiles, session flow, and outfit history.

  ---
  ## 2026-05-15 — Weather Context Integration (MVP v1)

### Weather API Integration
- Added lightweight weather context integration using Open-Meteo.
- Weather provider remains isolated in API layer; engine still consumes deterministic `ContextInput`.
- Added `/api/v1/weather/context` endpoint.

### Context Mapping
- Current weather is mapped into existing engine semantics:
  - Rain
  - Snow
  - Hot
  - Cold
  - Clear
- Season is inferred from month for MVP simplicity.

### Demo UX Direction
- Demo now supports automatic weather detection via browser geolocation.
- Long-term UX direction:
  - compact context summary card
  - manual override support
  - selectable destination city
- Initial MVP city list:
  - Istanbul
  - Ankara
  - Antalya
  - Bursa
  - Konya
  - Samsun
	
---

## Demo UI Evolved Into Product Validation Surface

### Context

Initial demo UI behaved mostly like a technical testing screen.

As real-user testing increased, the need emerged for:

- lightweight weather visibility
- contextual awareness
- wardrobe inspection
- cleaner product-oriented interaction

### Decision

Demo UI now acts as a lightweight product validation surface rather than only a raw API tester.

### Implementations

- Real weather integration through Open-Meteo
- City-based context loading
- Lightweight top-bar context summary
- Advanced weather override testing panel
- Dynamic wardrobe profile loading
- Wardrobe preview summary
- Side-drawer wardrobe inspection UI
- Category-grouped wardrobe visualization

### Reason

Users evaluate recommendations more naturally when:
- wardrobe is visible
- context feels real
- outfit generation appears connected to an actual environment

### Confidence

High

---

## Transition Toward Taxonomy-Driven Architecture

### Context

As category count increased, semantic behavior became fragmented across the engine.

Adding categories such as:
- LightOuterwear
- Hoodie
- Sweater
- Skirt

started to create scaling and maintenance risks.

The same category knowledge existed in:
- scoring
- context logic
- pairing
- wardrobe analysis
- UI grouping

### Decision

A taxonomy-driven architecture transition was started.

### Implementations

- Category metadata moved to JSON catalog
- CategoryCatalogService introduced
- Wardrobe API responses enriched with taxonomy metadata
- UI grouping migrated from raw category grouping to semantic group grouping
- CategorySemantics centralized semantic category logic:
  - top categories
  - bottom categories
  - footwear categories
  - layer roles
  - core/support pair semantics

### Additional Cleanup

- Legacy occasion fallback factories removed
- Occasion JSON catalog became the single source of truth
- Static SlotSet preset dependency reduced

### Reason

The system needs to support future category growth without:
- widespread enum checks
- multi-file semantic duplication
- unstable engine behavior

### Confidence

High

---

## Taxonomy Migration – Remaining Dress Special Cases

Date: 2026-05-25

- `Category.Dress` hardcoded generation filters migrated to `CategorySemantics.IsOnePiece()`
- `DressPath` semantic trait removed
- `OnePiece` became the canonical semantic for dress-style outfit paths

Remaining intentional special-cases:
- CombinationScorer → dress-mode scoring behavior
- AlternativePicker → dress vs top-bottom alternative reasoning
- CategorySemantics → dress-footwear core pair logic

These remain intentionally explicit because they represent outfit structure behavior,
not simple taxonomy membership.

---

## Taxonomy-Driven Generation Migration

Date: 2026-05-25

### Completed
- `allowedTraits` support added to slot requirements
- `SlotRequirementMatcher` updated to support trait-based matching
- Shoes slot migrated to taxonomy-driven selection
- Top slot migrated to taxonomy-driven selection
- Bottom slot migrated to taxonomy-driven selection

### Semantic Model Separation
The engine now distinguishes between:

- Traits → semantic meaning / behavior
- Slots → outfit composition eligibility
- Groups → taxonomy / UI grouping

Examples:
- `Layer`, `Protection`, `Comfort` → traits
- `Top`, `Bottom`, `Shoes`, `Anchor` → slots
- `Top`, `Layer`, `Shoes` → groups

### Naming Cleanup
- `IsTopCategory` → `CanFillTopSlot`
- `IsBottomCategory` → `CanFillBottomSlot`
- `IsFootwearCategory` → `CanFillShoesSlot`

### OnePiece Migration
- `DressPath` semantic trait removed
- `OnePiece` became the canonical semantic for dress-style outfit structures
- `CombinationGenerator` migrated from direct `Category.Dress` checks to `CategorySemantics.IsOnePiece()`

### Remaining Intentional Special-Cases
The following areas still intentionally contain explicit dress-mode logic:
- `CombinationScorer`
- `AlternativePicker`
- `CategorySemantics` dress-footwear core pair handling

These represent outfit structure semantics rather than simple taxonomy membership.

---
## Anchor Slot Migration Boundary

### Decision

Top, Bottom, Shoes and Outerwear generation can use taxonomy-driven matching.

Anchor generation remains mixed:

- `casual_weekend` uses `allowedSlots: ["Anchor"]`
- formal/smart occasions keep explicit `allowedCategories`

### Reason

Anchor defines outfit structure and occasion character.

Using `allowedSlots: ["Anchor"]` for formal/smart occasions could allow casual anchor categories such as Hoodie or Cardigan to enter contexts where Jacket or Dress should remain explicit.

### Current Rule

- Casual flexible anchor → slot-driven
- Formal/smart anchor → category-explicit

---
Date: 2026-05-25

### Decision: Protection Layer Guardrail

#### Problem:
Protection-type garments could appear together
(e.g. LightOuterwear + Coat).

#### Decision:
Protection layer duplication is prevented at generation output level.

#### Reason:
This is a structural outfit invalidity rather than a scoring preference.

Examples:
- LightOuterwear + Coat
- Raincoat + Coat
- Coat + Coat

Status:
Implemented.
Regression test added.


---
Date: 2026-06-02
#### Decision: When final scores are equal, context suitability must outrank tie-break signals.

Ranking priority:

HardFail
→ Score
→ ContextDelta
→ TieBreak
→ WarningCount
→ Signature

#### Reason:

Thermal / environmental suitability
is a stronger signal than aesthetic tie-breaks.

Example:

Winter + Outdoor

Tshirt outfit
Score: -5
ContextDelta: -22

Shirt outfit
Score: -5
ContextDelta: -12

Shirt should win despite lower tie-break.

--- 

Decision:
Context suitability outranks tie-break signals.

Final ranking order:

1. HardFailCount
2. Score
3. ContextDelta
4. TieBreakScore
5. WarningCount
6. Signature

Reason:
Environmental suitability is more important than aesthetic tie-breaks when total scores are equal.

--- 
Decision:
Protection layers and structure layers use separate semantic roles.

Jacket:
- Anchor only
- Structure role

LightOuterwear:
- Outerwear only
- Protection role

Coat:
- Outerwear only
- Heavy protection role