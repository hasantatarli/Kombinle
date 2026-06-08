# Kombinle Engine Principles

## 1. Deterministic First

Engine deterministic çalışır. Aynı input → aynı output.

## 2. Explainability

Her karar açıklanabilir olmalı:

- Score breakdown
- Alternative reasons
- Context warnings

## 3. No Fake Alternatives

Gerçek alternatifler:

- Stil değiştirir
- Renk dengesi değiştirir
- Yapı değiştirir

Fake alternatifler:

- Sadece layer kaldırma
- Aynı kombin varyasyonu

## 4. Context Matters

Karar sadece event'e göre verilmez:

Occasion + Context + Wardrobe

Context must not be limited to warning messages.

Context can affect:

1. Generation
2. Scoring
3. Ranking
4. Feedback visibility

Example:
Winter + Outdoor should allow protective outerwear candidates.
Summer + Indoor should prefer lighter layer intensity.

## 5. Guardrail over Creativity

Amaç:

- “en yaratıcı kombin” değil
- “giyilebilir güvenli kombin”

## 6. Progressive Intelligence

Engine şu sırayla gelişir:

1. Structure (done)
2. Scoring (done)
3. Context awareness (in progress)
4. Wardrobe intelligence (next)
5. Recommendation engine (future)


---

## Demo Phase Principles

### 1. Validate Before Expanding

Do not add advanced features before the core product experience is validated.

### 2. UI Translates, Engine Decides

The engine uses structured enums and deterministic logic.

The UI is responsible for:
- Turkish display names
- Layout
- Readability

The UI must not contain decision logic.

### 3. Static Data Before Persistence

Use static wardrobe profiles before designing database persistence.

### 4. Manual Context Before Weather API

Use manual weather/context selection before integrating external weather APIs.

### 5. No Feature Explosion

Ideas such as:
- image recognition
- 3D avatar
- purchase recommendations
- premium tiers

are valid future ideas, but not part of current demo validation.

## 6. Layer Semantics Over Slot Names

A garment’s role in context should not depend only on its slot.

Example:
A Jacket used as Anchor can still behave as outerwear.

Layer reasoning should consider garment purpose:

- Comfort
- Structure
- Protection

---

Style suitability and thermal suitability are separate concerns.

Date: 2026-06-05

A garment may be:
- thermally appropriate
- semantically appropriate

without being the most stylistically suitable choice.

Future style scoring should operate independently from
thermal coherence scoring.


---
Date: 2026-06-08

Style suitability is an independent signal.

Style scoring should complement:
- Formality
- Color compatibility
- Thermal suitability

and should not dominate overall ranking.