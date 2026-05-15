# Project Brain

## System Purpose

Kombinle, kullanıcının dolabındaki kıyafetler ve bulunduğu koşullara göre:

* giyilebilir
* hızlı karar verilebilir
* açıklanabilir

kombinler üreten bir **outfit decision engine**’dir.

Amaç:
Sadece kombin üretmek değil,
**hangi kombinin giyilmesi gerektiğine karar vermek ve bunu açıklamak**.

---

## Core Principles

* Deterministic (aynı input → aynı output)
* Explainable (nedenini açıklar)
* Config-driven (mümkün olduğunca JSON üzerinden büyür)
* Context-aware (hava, ortam, zaman etkili)
* Category-based (gender-based değildir)

---

## Core Flow

1. API request alınır
2. Occasion ve context çözülür
3. Kombin adayları üretilir (multi-path)
4. Adaylar scoring + context ile değerlendirilir
5. Ranking yapılır (best belirlenir)
6. Decision summary oluşturulur
7. Alternatifler belirlenir
8. Karar açıklanır (message catalogs)
9. API response oluşturulur

---

## Core Modules

* Domain & Outfit Model
* Occasion & Configuration
* Combination Generation (multi-path)
* Evaluation & Scoring
* Decision & Alternatives
* Product Messaging
* API Surface

---

## Decision Model

### Decision States

* Safe
* Warning
* HardFail

### Context Health

* Good
* Poor


## Engine Behavior v1.1 (Context-Aware Generation & Layer Model)

Engine artık sadece Occasion bazlı değil, aşağıdaki üçlüye göre karar verir:

- Occasion (formality & structure)
- Context (weather, setting, time)
- Wardrobe (available items)

### Layer Model

Outfit iki parçaya ayrılır:

- CoreItems → temel kombin (top, bottom, shoes)
- Layers → dış katmanlar (jacket, Jacket, coat)

Bu sayede:

- Aynı kombin ceketsiz de kullanılabilir
- Alternatif listesinde gereksiz tekrarlar engellenir

Layer model evolved from slot-only logic to semantic layer behavior.

Layer roles:

- Comfort: Hoodie, Cardigan
- Structure: Jacket, Jacket
- Protection: Coat

Layer intensity affects context suitability.

Examples:

- Summer + Indoor penalizes unnecessary layers
- Winter + Outdoor rewards protective outerwear
- Jacket used as Anchor still counts as outerwear for context purposes

### Alternative Filtering

Aşağıdaki kombinler alternatif olarak **gösterilmez**:

- Sadece layer kaldırılmış kombinler  
  (örn: Jacket çıkarılmış hali)

Alternatif sayılabilmesi için:

- Core değişmeli **veya**
- Layer değişmeli

### Context-Aware Outerwear

Outerwear artık her zaman otomatik eklenmez.

Kurallar:

- Rain → outerwear dahil edilir
- Clear / Indoor → outerwear optional kalır

### Wardrobe Feedback Priority

Feedback seçimi aşağıdaki önceliğe göre yapılır:

1. RAIN_* (kritik risk)
2. SOFT_* (yumuşak öneri)
3. OUTDOOR_* (context eksikliği)

Amaç:

- Kullanıcıya en önemli problemi göstermek
---

## Explainability Model

* Decision mesajları (headline + subtext)
* Context warning mesajları
* Alternative reason codes
* Max 2 reason gösterimi
* Priority ve dedup mekanizması

---

## Key Decisions

* System category-based çalışır
* Anchor ayrı bir domain kavramıdır
* Outfit-level değerlendirme Anchor + Slot birlikte yapılır
* Multi-path desteklenir (Dress / TopBottom)
* Path’ler aynı kombin içinde karışmaz
* RecommendedAlternative sadece riskli durumda gösterilir
* Mesajlar catalog üzerinden yönetilir
* Score kullanıcıya ham olarak gösterilmez

---

## Known Constraints

* Sistem deterministic olmalıdır
* Hibrit kombin üretilemez (Dress + TopBottom)
* API response sade kalmalıdır
* Config runtime’da doğru yüklenmelidir

---

## Known Risks / Tensions

### Config vs Code

Davranışın bir kısmı config’te, kritik kurallar kodda.

### Category vs Path

Model generic, bazı davranışlar path-specific.

---

## Stabilization Notes (Summary)

* Anchor artık ayrı bir domain alanıdır
* Duplicate garment render engellenmiştir
* Meaningful alternative filtering düzeltilmiştir
* RecommendedAlternative davranışı stabilize edilmiştir
* Color scoring is now domain-aware: pair relation weight and color compatibility are evaluated separately.
* Color rules and scoring weights are now config-driven through JSON resources.
* Formality model target-based hale getirildi; `requiredFormality` minimum eşik değil hedef seviye olarak yorumlanır.
* Supporter selection target formality’ye göre sıralanır; casual occasion’larda casual parçalar candidate pool’a girebilir.
* Optional outerwear primary/core kombine otomatik eklenmez; layer concept için temel oluşturuldu.
* Aynı garment’in hem Anchor hem Outerwear rolünde kullanılması engellendi.
* Alternative reasoning layer-aware hale getirildi; `ALT_LAYER_REMOVED` eklendi.
* Wardrobe gap detection altyapısı başlatıldı; commerce bridge için `WardrobeGap` modeli ve `wardrobeGaps` response alanı eklendi.

---

## Current Phase

* Engine stabilization: büyük ölçüde tamam
* API response: stabilize
* Smart Casual / Casual occasion behavior stabilize edilmeye başlandı
* Formality scoring target-based hale getirildi
* Generation tarafında casual item inclusion düzeltildi
* Layer-aware presentation bir sonraki önemli ürün adımı
* Sonraki adım: realistic wardrobe regression scenarios + layer-aware response design


## Engine v1.0 — Stabil State (Post-Refinement)

### Core Capabilities

- Deterministic decision engine
- Explainable scoring (formality, color, context)
- Multi-path generation (Dress vs Top/Bottom)
- Layer-aware outfit model (CoreItems + Layers)
- Context-aware adjustments (weather, setting, time)
- Alternative generation with meaningful differences
- Wardrobe feedback (soft / hard signals)

---

### Formality Handling

- Occasion defines required formality
- Items scored against required formality
- Casual occasion:
  - Formal anchors (e.g., Jacket) receive penalty
  - Casual anchors (cardigan, hoodie) preferred
- Formal occasion:
  - Non-formal items strongly penalized

---

### Color System (v1)

- Neutral colors: White, Black, Grey, Beige, Navy
- Rules:
  - Neutral + Neutral → Strong match
  - Neutral + Any → Acceptable
  - Explicit clash overrides everything (e.g., Black + Brown)
- StrongPairs extended with real-life safe combinations (Blue, Brown interactions)

---

### Layer Model

Outfit is split into:

- CoreItems → essential wearable combination
- Layers → optional or context-driven pieces (jacket, coat)

Behavior:

- Same outfit can be presented with or without layers
- Layer removal is NOT considered a true alternative
- Layer addition is treated as improvement (context/formality)

---

### Context Awareness

Engine considers:

- Weather (Rain, Clear, etc.)
- Setting (Indoor / Outdoor)
- TimeOfDay

Examples:

- Rain → outerwear required or strongly preferred
- Missing outerwear → wardrobe feedback
- Context penalties affect ranking and messaging

Context now includes:

- Weather
- Setting
- TimeOfDay
- Season

Season is not a replacement for weather.
It gives environmental style/comfort context.

Example:

Clear + Summer + Indoor
→ light outfit preferred

Clear + Winter + Outdoor
→ protective outerwear can be generated and preferred

---

### Alternative Logic

Alternatives must be:

- Structurally different OR
- Visually meaningfully different

NOT allowed:

- Same outfit minus a layer (filtered out)

Examples:

- Top/Bottom swap
- Color improvement
- Structure shift (Dress ↔ TopBottom)

---

### Wardrobe Feedback

Generated when:

- Majority of combinations fail same constraint

Types:

- SOFT_* → suggestion (e.g., missing jacket)
- HARD_* → real gap (e.g., no rain-safe shoes)

Priority system:

- Context risks > Soft suggestions

---

### Current Status

Engine is:

- Behaviorally stable
- Test-covered
- Ready for UX layer / productization

Next phases:

- Color v2 (semantic groups)
- Dominant/accent balance
- Personalization (user style)

---

## Demo & Product Validation Phase

Kombinle şu anda engine geliştirme aşamasından demo doğrulama aşamasına geçmiştir.

### Current State

- Engine behavior is stable
- Demo UI is available under `wwwroot/demo.html`
- Static wardrobe profiles are used for validation
- Output is rendered in a user-readable card layout

### Demo Purpose

The demo is not a full product.

Its purpose is to answer:

> Can a non-technical person understand and evaluate the outfit recommendation?

### Current Demo Focus

- Turkish display names for garments
- Cleaner visual layout
- Clear best outfit / alternatives separation
- Readable explanation of why the outfit works

### Not In Scope Yet

- User accounts
- Persistent wardrobe database
- Image recognition
- Real weather API
- Commerce / purchase recommendations