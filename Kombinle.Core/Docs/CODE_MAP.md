# CODE_MAP

## Purpose

Bu dosya, Kombinle projesinin mevcut kod haritasını yüksek seviyede gösterir.
Amaç, tüm kodu açıklamak değil; aktif akışı, çekirdek modülleri, giriş noktalarını ve legacy/inactive alanları görünür kılmaktır.

---

## Active Product Flow

API Request
→ `Kombinle.Api/Program.cs`
→ `IDecisionService / DecisionService`
→ Occasion resolve + request mapping
→ `CombinationGenerator`
→ `CombinationScorer`
→ `CombinationRanker`
→ `DecisionSummaryBuilder`
→ `ResponseMapper`
→ API Response

Demo UI
→ `Kombinle.Api/wwwroot/demo.html`
→ POST `/api/v1/decision`
→ renders API response as user-readable cards

---

## Core Entry Points

### Active Entry Point

* `Kombinle.Api/Program.cs`

### Active Orchestration

* `Kombinle.Api/Services/DecisionService.cs`

### Historical Entry Point

* `Kombinle.Core/Program.cs`

**Notes**

* Aktif ürün giriş noktası API tarafındadır.
* `Kombinle.Core/Program.cs`, API katmanı yazılmadan önce kullanılan console test harness / legacy runner olarak değerlendirilmelidir.


* Kombinle.Core.Tests contains older core-level tests and may include outdated assumptions.
* *Current active regression validation is primarily under Kombinle.Api.Tests.

---

## Core Modules

### 1. Occasion & Input Interpretation

**Purpose**
Occasion, context ve kullanıcı girdisini engine’in anlayacağı hale getirir.

**Likely Files**

* `Kombinle.Api.Contracts/DecisionContracts.cs`
* `Kombinle.Api/Services/DecisionService.cs`
* `Kombinle.Api/Mapping/OccasionResolver.cs`
* `Kombinle.Api/Mapping/MappingHelpers.cs`
* `Kombinle.Core/Domain/Occasion/*`
* `Kombinle.Core/Resources/occasions.json`

**Status**
ACTIVE

**Notes**

* Occasion resolving ve request → domain dönüşümü burada başlar.
* Occasion yapısı config-driven görünmektedir.

---

### 2. Domain & Outfit Model

**Purpose**
Sistemin ortak veri dilini sağlar: garment, category, slot, occasion, context ve kombin yapısı.

**Likely Files**

* `Kombinle.Core/Domain/*.cs`
* `Kombinle.Core/Domain/Context/*`
* `Kombinle.Core/Domain/Traits/*`

**Status**
ACTIVE

**Notes**

* Tüm diğer modüller bu alanın üstüne kurulur.
* `Anchor`, `SlotSet`, `Garment`, `Occasion` gibi temel kavramlar burada yer alır.

---

### 3. Combination Generation

**Purpose**
Gardırop ve occasion bilgisine göre kombin adayları üretir.

**Likely Files**

* `Kombinle.Core/Generation/CombinationGenerator.cs`
* `Kombinle.Core/Generation/AnchorSelector.cs`
* `Kombinle.Core/Generation/SupporterSelector.cs`

**Status**
ACTIVE

**Notes**

* Multi-path generation burada çalışır.
* Dress ve TopBottom yollarının ayrımı burada enforce edilir.
* Deterministic üretim ve guardrail mantığı bu modülde bulunur.
* SupporterSelector artık target formality’ye yakın parçaları önce sıralamalıdır.
* Optional outerwear primary candidate’a otomatik dahil edilmez; layer/presentation konusu olarak ele alınır.
* Anchor ile aynı garment supporter pool’da tekrar kullanılmamalıdır.

Recent changes:
* Optional outerwear generation is now context-aware
* `ShouldIncludeOuterwear` considers rain, snow/cold, winter outdoor and autumn outdoor night

---

### 4. Evaluation, Scoring & Ranking

**Purpose**
Üretilen kombinleri formality, color, context ve ranking açısından değerlendirir.

**Likely Files**

* `Kombinle.Core/Scoring/*`
* `Kombinle.Core/Scoring/Context/*`
* `Kombinle.Core/Rules/*`
* `Kombinle.Core/Config/ScoringConfig.cs`
* `Kombinle.Core/Resources/color_rules.json`
* `Kombinle.Core/Resources/scoring_config.json`
* `Kombinle.Core/Config/ColorRulesConfig.cs`
* `Kombinle.Core/Config/ColorRulesConfigLoader.cs`
* `Kombinle.Core/Config/ScoringConfigLoader.cs`

**Status**
ACTIVE

**Notes**

* `CombinationScorer` ve `CombinationRanker` aktif akışta kullanılır.
* Context evaluation ayrı çekirdek modül değil, bu modülün parçası olarak düşünülmelidir.
* Rules burada aktif destek rolündedir.
* Color scoring now uses both pair relation weight and color compatibility level.
* Color rules and scoring parameters are loaded from JSON resources.
* Formality scoring minimum-threshold modelinden target-distance modeline evrildi.
* Casual occasion’da formal parçalar otomatik olarak daha iyi kabul edilmez.

* Recent changes:
* `ContextScoringService` now includes season-aware layer suitability
* Layer roles and layer intensity are evaluated during context scoring
* `HasOuterwear` should consider garment category/role, not only Slot.Outerwear

---

### 5. Decision, Alternatives & Feedback

**Purpose**
En iyi kombini, alternatifleri ve kullanıcıya dönülecek karar semantiğini oluşturur.

**Likely Files**

* `Kombinle.Core/Scoring/DecisionSummaryBuilder.cs`
* `Kombinle.Core/Scoring/DecisionSummary.cs`
* `Kombinle.Core/Scoring/Alternatives/*`
* `Kombinle.Core/Scoring/WardrobeFeedbackRules/*`
* `Kombinle.Core/Scoring/Presenting/WardrobeFeedbackPresenter.cs`
* `Kombinle.Core/Scoring/WardrobeGapEngine.cs`
* `Kombinle.Core/Domain/WardrobeGap.cs`

**Status**
ACTIVE

**Notes**

* `DecisionSummaryBuilder`, ranking sonucunu yorumlayıp ürün karar yapısına dönüştürür.
* `AlternativePicker` bu alanın alt sorumluluğudur.
* Wardrobe feedback bu modülün parçası olarak çalışır.
* WardrobeGapEngine, ileride commerce bridge için kullanılacak eksik/fırsat sinyallerini üretir.
* AlternativePicker artık layer removed gibi kullanıcı açısından anlamlı farkları reason code’a çevirebilir.
* 
---

### 6. Product Messaging & Explanation

**Purpose**
Kararın kullanıcıya ürün diliyle ve açıklanabilir şekilde aktarılmasını sağlar.

**Likely Files**

* `Kombinle.Core/Resources/alternative_messages.json`
* `Kombinle.Core/Resources/context_messages.json`
* `Kombinle.Core/Resources/decision_messages.json`
* `Kombinle.Core/Scoring/Presenting/DecisionMessage*.cs`
* `Kombinle.Core/Scoring/Alternatives/AlternativeMessage*.cs`
* `Kombinle.Core/Domain/Context/ContextMessage*.cs`

**Status**
ACTIVE

**Notes**

* Catalog tabanlı mesaj sistemi burada konumlanır.
* Hardcoded ürün dili yerine message catalog yaklaşımı kullanılır.

---

### 7. API Response Surface

**Purpose**
Engine kararını dış dünyaya tutarlı bir API response olarak sunar.

**Likely Files**

* `Kombinle.Api/Program.cs`
* `Kombinle.Api/Mapping/ResponseMapper.cs`
* `Kombinle.Api.Contracts/DecisionContracts.cs`

**Status**
ACTIVE

**Notes**

* `ResponseMapper`, `DecisionSummary` → `DecisionResponse` dönüşümünü yapar.
* Bu modül yeni karar üretmez; mevcut kararı sunar.
* Duplicate garment render önleme burada çözülür.

---

### 8. Regression Safety

**Purpose**
Çekirdek davranışların bozulmadan korunmasını sağlar.

**Likely Files**

* `Kombinle.Core.Tests/*`
* `Kombinle.Api.Tests/*`
* `Kombinle.Api/smoke-tests/*`

**Status**
ACTIVE

**Notes**

* İş kuralı modülü değildir.
* Sistem güvenliği açısından çekirdek destek modülüdür.
* `smoke-tests/` runtime modülü değil, test artifact olarak değerlendirilmelidir.

### 9. Demo UI Layer

**Purpose**
Postman kullanmadan engine çıktısını teknik olmayan kullanıcıya göstermek.

**Likely Files**

* `Kombinle.Api/wwwroot/demo.html`

**Status**
ACTIVE / DEMO

**Notes**

* Bu katman karar üretmez.
* API response’u kullanıcı dostu kartlara çevirir.
* Enum/display mapping burada yapılabilir.
* Demo validation için kullanılır.

---

## Supporting / Nested Areas

Bunlar ayrı çekirdek modül olarak değil, üst modüllerin parçası olarak düşünülmelidir:

* Context Evaluation → `Evaluation, Scoring & Ranking` parçası
* Message Catalogs → `Product Messaging & Explanation` parçası
* Response Mapping → `API Response Surface` parçası
* Alternative Reasoning → `Decision, Alternatives & Feedback` parçası
* Wardrobe Feedback → `Decision, Alternatives & Feedback` parçası

---

## Confirmed Active Flow

### API Layer

* `Kombinle.Api/Program.cs`
* `Kombinle.Api/Services/DecisionService.cs`

### Core Flow

* `OccasionResolver`
* `MappingHelpers`
* `CombinationGenerator`
* `CombinationScorer`
* `CombinationRanker`
* `DecisionSummaryBuilder`

### Presentation Layer

* `ResponseMapper`

---

## Legacy / Historical / Inactive

### Historical

* `Kombinle.Core/Program.cs`

**Reason**
API öncesi dönemde senaryoları console üzerinden çalıştırmak için kullanılmıştır.
Aktif ürün giriş noktası değildir.

---

### Inactive / Empty

* `Kombinle.Core/Engine/RuleEngine.cs`
* `Kombinle.Core/Engine/CombinationBuilder.cs`

**Reason**
Mevcut bilgiye göre aktif akışta kullanılmıyor ve içerikleri boş/eski durumda.

---

## Resolved False Alarms

* `Kombinle.Core/Config/ResponseMapper.cs` → böyle bir dosya yok
* `Kombinle.Core/Config/OccasionResolver.cs` → böyle bir dosya yok
* Core tarafında `ResponseMapper` duplicate şüphesi kapatıldı
* Occasion resolver aktif akışta API mapping alanında konumlanıyor

---

## Working Rule

Bu dosya kesin “unused code” kararı vermez.
Bu dosyanın amacı:

* aktif akışı görünür kılmak
* modül sınırlarını netleştirmek
* legacy / inactive alanları işaretlemek
* sonraki teknik temizlik için sağlam bir harita oluşturmaktır

---

## Suggested Status Tags

İleride dosya bazlı sınıflama yapılırken şu etiketler kullanılmalıdır:

* `ACTIVE`
* `LEGACY`
* `INACTIVE`
* `HISTORICAL`
* `UNKNOWN`
* `DEMO`

Bu etiketler, silme kararı vermeden önce görünürlüğü artırmak için kullanılmalıdır.
