# Kombinle Project Brain v1

## 🎯 Project Goal
Kombinle, kullanıcının dolabındaki kıyafetlere göre **koşul ve bağlama göre en doğru kombini öneren ve kararını açıklayan** bir decision engine’dir.

Amaç:
- Sadece kombin üretmek değil
- **Karar vermek + nedenini açıklamak + alternatif sunmak**

---

## 🧱 Architecture Overview

### Core Layer
- Combination generation
- Scoring (Score, TieBreak)
- Context evaluation (ContextDelta, WarningCodes)
- Risk calculation (Safe, Warning, HardFail)
- Alternative selection (AlternativePicker)

### API Layer
- Minimal API (`/api/v1/decision`)
- Request → DecisionSummary → Response mapping

### Mapping Layer
- `ResponseMapper`
    - Decision → DecisionCardDto
    - Alternatives → AlternativeCardDto
    - WardrobeFeedback mapping
    - RecommendedAlternative logic

---

## 🔑 Key Concepts

### 1. Decision
- Best combination
- Headline + Subtext (DecisionMessageCatalog)
- Context-aware messaging

### 2. Alternatives
- Best dışındaki seçenekler
- Explainable (AlternativeReasonCodes)

### 3. RecommendedAlternative ⭐
- En iyi alternatif (highlight)
- Sadece riskli durumlarda gösterilir
- Alternatives listesinden ayrılır

---

## 🧠 Decision Logic

### Decision States
- Safe
- Warning
- HardFail

### Context Health
- Good
- Poor

---

## 🧾 Message System (Catalog-based)

### 1. Decision Messages
- `decision_messages.json`
- Headline + Subtext

### 2. Context Messages
- `context_messages.json`
- Risk açıklamaları (örn: RAIN_SUEDE_SHOES)

### 3. Alternative Messages
- `alternative_messages.json`
- priority + group içerir

---

## 🧠 Alternative Reason System

### Source
`ScoredCombination.AlternativeReasonCodes`

### Processing
1. Distinct
2. Priority (küçük sayı = daha önemli)
3. Group deduplication
4. Max 2 reason

### Goal
- Gürültüyü azaltmak
- En anlamlı 1–2 reason göstermek

---

## 🧠 Current Implementation (IMPORTANT)

### MapAlternativeReasons
- Priority ascending (10 > 999)
- Group-based filtering
- Take AFTER grouping (corrected)

---

## ⭐ RecommendedAlternative Logic

### Conditions
- BestContextHealth == Poor
- Alternatives mevcut

### Behavior
- İlk alternatif seçilir (already ranked)
- Alternatives listesinden çıkarılır

---

## ⚠️ Important Design Decisions

### 1. Small priority = high importance
- 10 = high
- 999 = low

### 2. Max 2 reason göster
- UX için kritik

### 3. Decision vs WardrobeFeedback ayrımı
- Decision → global yorum
- Wardrobe → ek öneri

---

## 🧪 Smoke Test Scenarios

### 1. business_meeting_formal (WARNING)
- Rain + suede shoes
- Expected:
  - Warning headline
  - Context-aware subtext
  - RecommendedAlternative present

### 2. casual_weekend (SAFE)
- Expected:
  - Generic safe message
  - No recommended alternative

### 3. interview_formal (SAFE + alternatives)
- Expected:
  - Safe decision
  - Alternatives list
  - No recommended alternative

---

## 📦 Example Request

```json
{
  "occasionId": "business_meeting_formal",
  "context": {
    "weather": "Rain",
    "setting": "Outdoor",
    "timeOfDay": "Day"
  },
  "items": [
    { "category": "Jacket", "colorFamily": "Navy", "formality": "Formal" },
    { "category": "Shirt", "colorFamily": "White", "formality": "Formal" },
    { "category": "Pants", "colorFamily": "Grey", "formality": "Formal" },
    { "category": "Shoes", "colorFamily": "Black", "formality": "Formal", "shoe": { "material": "Suede" } }
  ]
}

🚀 Current State

System artık:

✅ Decision veriyor
✅ Context-aware açıklıyor
✅ Alternatif üretiyor
✅ En iyi alternatifi seçiyor
✅ Multilanguage çalışıyor
✅ Explainable AI-like davranıyor

🧭 Next Roadmap
1. Context Intelligence (short-term)

Temperature

Indoor vs Outdoor detaylandırma

Layer logic

2. User Preferences (mid-term)

Style preference

Color avoidance

Formality bias

3. UI / Demo (strategic)

Basit frontend

Kombin öner + alternatif seç

⚠️ Rules for Future Development

Hardcode mesaj YOK → her şey JSON’dan

Yeni reason → priority + group zorunlu

API response sade kalmalı

Max 2 reason kuralı korunmalı

📌 Notes

AlternativePicker zaten sıralama yapıyor → tekrar ranking yazma

RecommendedAlternative = Alternatives[0]

DecisionSubtext sadece riskli durumda override edilir

🎯 Summary

Kombinle artık:

❌ Kombin öneren sistem değil
✅ Karar veren + açıklayan sistem

### Engine stabilization updates
- CombinationCandidate içinde Anchor artık SlotToItem[Anchor] olarak tekrar tutulmuyor
- BuildSignature anchor’ı ayrı hesaba katacak şekilde güncellendi
- Context night visibility evaluation anchor-aware hale getirildi
- DecisionSummary meaningful difference kontrolü anchor değişimini doğru algılayacak şekilde düzeltildi
- ResponseMapper outfit item rendering duplicate garment göstermeyecek şekilde temizlendi
- Tests passing, recommended alternative behavior restored

## API Stabilization & Smoke Test Lock-In (Current State)

### What was stabilized
- `CombinationCandidate` içinde `Anchor`, artık `SlotToItem[Anchor]` olarak tekrar tutulmuyor.
- `BuildSignature` anchor’ı ayrı hesaba katacak şekilde güncellendi.
- Context evaluation tarafında anchor-aware davranış eklendi:
  - `ApplyNight(...)` artık `candidate.Anchor` parçasını da görünürlük değerlendirmesine dahil ediyor.
- `DecisionSummaryBuilder.HasMeaningfulDifference(...)` anchor farkını doğru sayacak şekilde düzeltildi.
- `ResponseMapper.MapOutfitItems(...)` duplicate garment render etmeyecek şekilde güncellendi.
- `ResolveAlternativeShortText(...)` fallback olarak teknik signature göstermek yerine kullanıcı dostu kısa kombin metni üretecek şekilde iyileştirildi.
- `CreateNoBestResponse(...)` fallback metinleri ürün diline uygun hale getirildi.
- Hard warning ve soft warning karar dili ayrıştırıldı:
  - `ResolveDecisionMessageCode(...)` soft warning (`SOFT_*`) kodlarını ayrı decision code’lara yönlendiriyor.
  - soft warning decision message catalog entry’leri eklendi.
- `ResolveDecisionSubtextTr(...)` soft warning tonuna uygun hale getirildi.

### Behavior now locked
Aşağıdaki davranışlar artık doğru çalışıyor ve regresyon riski yüksek alanlar olarak kabul edilmeli:
- Safe case:
  - risk yoksa `recommendedAlternative = null`
  - alternatif varsa normal `alternatives` içinde listelenir
- Hard warning + recommended alternative:
  - riskli best kombin korunur
  - daha güvenli alternatif `recommendedAlternative` olarak öne çıkar
- No-best:
  - boş outfit döner
  - fallback headline/subtext ürün diliyle gelir
- Soft warning:
  - sert risk dili yerine yumuşak karar tonu kullanılır
- Night visibility:
  - anchor’daki bright item artık görünürlük hesabına dahil edilir

### API smoke tests added
Yeni test projesi eklendi:
- `Kombinle.Api.Tests`

Eklenen smoke test kapsamı:
- safe scenario
- hard warning + recommended alternative scenario
- no-best scenario
- soft warning scenario

Amaç:
- response behavior’ı endpoint seviyesinde sabitlemek
- mapper / summary / message tone regresyonlarını erken yakalamak

### Important implementation note
Bu aşamadan sonra:
- yeni feature eklemeden önce mevcut davranış testle kilitlenmeli
- response tone / recommendation behavior değişirse önce smoke test güncellenmeli
- message catalog değişiklikleri product behavior değişikliği olarak değerlendirilmeli

### Current phase
Project is in:
- **Engine stabilization + API response polish phase**

Core decision engine büyük ölçüde stabil.
Öncelik artık yeni feature eklemek değil, response quality, contract consistency ve regression safety.

## Multi-Path Combination Support (Dress + TopBottom)

### Summary
Engine artık tek kombin yolu yerine aynı occasion içinde birden fazla kombin üretim yolunu destekler.

### New Capability
Occasion config içine `combinationModes` eklendi.

Örnek:
- "Dress"
- "TopBottom"

### Behavior
- Dress mode:
  - Dress + Shoes (+ optional outerwear)
- TopBottom mode:
  - Top + Bottom + Shoes (+ optional anchor)

Generator bu modlara göre ayrı candidate set üretir ve sonuçları birleştirir.

### Implementation
- `Occasion.CombinationModes` eklendi
- `OccasionCatalogLoader` JSON’dan map ediyor
- `CombinationGenerator` içinde branching eklendi:
  - `GenerateDressMode`
  - `GenerateTopBottomMode`

### Important Rule
- Generate = entry point
- Mode methods = leaf (recursive call yok)

### Slot Behavior
- Anchor "Soft" yapılarak TopBottom path enable edildi
- Dress path Anchor üzerinden çalışmaya devam eder

### Tests Added
- WeddingFlexible_DressPath_ShouldReturnDressOutfit
- WeddingFlexible_TopBottomPath_ShouldReturnSeparatedOutfit

### Result
- Aynı occasion içinde farklı kombin stratejileri mümkün
- Kadın / erkek ayrımı yapılmadan daha gerçekçi kombin üretimi sağlandı