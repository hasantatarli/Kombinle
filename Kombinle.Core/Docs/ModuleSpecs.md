# Module Specification

---

# 🧩 Core Modules

---

## 1. Domain & Outfit Language

### Purpose

Sistemin ortak konuşma dilini tanımlar: kıyafet türleri, slot yapısı, occasion ve context.

### Responsibilities

* Garment / Category / Slot kavramlarını tanımlamak
* Occasion ve Context temel yapılarını sağlamak
* Anchor kavramını ayrı domain olarak yönetmek

### Invariants

* Sistem kategori bazlıdır (gender-based değildir)
* Anchor, SlotToItem’dan ayrı bir kavramdır
* Outfit-level değerlendirmelerde Anchor + Slot birlikte ele alınabilir

### Notes

Bu modül tüm sistemin temelidir. Diğer tüm modüller buna dayanır.

---

## 2. Occasion & Rule Configuration

### Purpose

Occasion davranışlarını config üzerinden tanımlamak ve büyütmek.

### Responsibilities

* Occasion JSON/config yüklemek
* Formality, default context, slot yapısı sağlamak
* CombinationModes (multi-path) davranışını tanımlamak

### Invariants

* Occasion’lar mümkün olduğunca config üzerinden tanımlanır
* Sistem config-driven büyür
* Config davranışı runtime’da eksiksiz yansıtılmalıdır

### Notes

Config-driven yaklaşımın merkezi burasıdır.

---

## 3. Combination Generation

### Purpose

Gardırop ve occasion’a göre kombin adayları üretmek.

### Responsibilities

* Candidate kombin üretmek
* Primary ve variant kombinler oluşturmak
* Multi-path kombinleri ayrı ayrı üretmek
* Deterministic davranışı korumak

### Invariants

* Üretim deterministik olmalıdır
* Dress ve TopBottom aynı candidate içinde karışamaz
* Multi-path’ler ayrı işlenir
* Anchor ayrı ele alınır
* * Supporter selection target formality’ye göre yapılır; `requiredFormality` minimum eşik gibi kullanılmaz
* Optional `Outerwear` primary/core kombine otomatik eklenmez
* Aynı garment aynı candidate içinde hem Anchor hem Outerwear olarak kullanılmamalıdır

### Notes

Path isolation hataları bu modülde ortaya çıkar.


Generation may include optional outerwear when context requires it.

Examples:
- Rain
- Snow / Cold
- Winter + Outdoor
- Autumn + Outdoor + Night

Optional outerwear must not be generated for every scenario.

---

## 4. Evaluation & Scoring

### Purpose

Kombinlerin uygunluğunu değerlendirmek ve sıralamak.

### Responsibilities

* Formality uyumu değerlendirmek
* Color harmony / clash kontrolü yapmak
* Context etkisini skora yansıtmak
* Ranking ve tie-break üretmek

### Invariants

* Skor kullanıcıya gösterilmez
* Amaç sadece “iyi mi?” değil, “daha iyi hangisi?” sorusudur
* Pair count alone must not determine outfit quality
* Color compatibility and pair relation importance are separate scoring concepts
* Color and scoring weights should remain config-driven where possible
* `requiredFormality` target formality olarak değerlendirilir
* Formality scoring distance-based çalışır
* Casual occasion’da formal parçalar otomatik olarak daha iyi sayılmaz
* Scoring, uygun kombin yoksa sistemi tamamen susturmak yerine en yakın giyilebilir seçeneği sıralayabilmelidir

### Depends On

* Domain
* Generation
* Context Evaluation

### Notes

Mode-aware scoring bu modülün kritik evrim noktasıdır.


Context suitability now includes layer intensity and layer role evaluation.

Scoring should consider:

- Season
- Setting
- Weather
- TimeOfDay
- Layer role
- Layer intensity

Context scoring should influence ranking but should not blindly dominate compatibility/formality scoring.

---

## 5. Decision & Alternatives

### Purpose

En iyi kombini seçmek ve anlamlı alternatifleri üretmek.

### Responsibilities

* Best kombin seçimi
* Alternatives listesi üretmek
* RecommendedAlternative davranışını yönetmek
* No-best durumunu ele almak
* Wardrobe gap bilgisini decision summary seviyesinde taşıyabilmek

### Invariants

* RecommendedAlternative sadece riskli durumda öne çıkar
* Alternatifler anlamlı ve farklı olmalıdır
* Soft vs hard warning ayrımı korunmalıdır
* Layer çıkarılmış alternatifler generic item swap olarak açıklanmamalıdır
* Jacket/Jacket çıkarılmış alternatiflerde `ALT_LAYER_REMOVED` gibi özel reason kullanılabilir
* Alternative reason, teknik farkı değil kullanıcı açısından anlamlı farkı anlatmalıdır

### Notes

Bu modül sistemi gerçek “decision engine” haline getirir.

---

BestPool is part of the Decision layer.

It represents an internal pool of Best-quality candidates used for deterministic rotation. It is separate from Alternatives.

BestPool candidates must be:
- context-safe
- close to the current Best quality band
- meaningfully different
- not hard-failed

Alternatives remain secondary recommendations and are not promoted to Best in v1.

---

## 6. Explanation & Product Messaging

### Purpose

Kararı kullanıcıya anlaşılır ve güven veren şekilde aktarmak.

### Responsibilities

* Decision mesajlarını üretmek
* Context warning mesajlarını sağlamak
* Alternative reason mesajlarını yönetmek
* Ürün tonunu korumak

### Invariants

* Mesajlar catalog üzerinden gelir (hardcode yok)
* Soft warning ve hard risk farklı tonlarda sunulur
* No-best mesajları ürün diliyle verilir

### Notes

Explainability bu modülde gerçekleşir.

---

## 7. API Contract & Response Surface

### Purpose

Engine sonucunu dış dünyaya tutarlı bir API cevabı olarak sunmak.

### Responsibilities

* Request/response contract yönetmek
* Response mapping yapmak
* Outfit gösterimini üretmek
* Duplicate render’ı engellemek

### Invariants

* API yeni karar üretmez
* Response formatı stabil olmalıdır
* Kullanıcıya teknik detaylar yansıtılmaz
* Wardrobe gap bilgilerini API response’a sade ve commerce-ready formatta taşımak

### Notes

Bu katman tamamen presentation odaklıdır.

---

## 8. Regression Safety

### Purpose

Sistem davranışlarının bozulmadan korunmasını sağlamak.

### Responsibilities

* Ana senaryoları testlerle sabitlemek
* Regression hatalarını yakalamak
* Yeni feature’ları smoke test ile kilitlemek

### Invariants

* Geliştirme küçük ve doğrulanabilir adımlarla yapılır
* Testler davranışın bozulup bozulmadığını kontrol eder

### Notes

Teknik modül değil, ama sistemin stabil kalması için kritiktir.

---

# 🔗 Module Relationships (High-Level Flow)

Generation → Evaluation → Decision → Messaging → API

Context Evaluation → Evaluation’a veri sağlar
Occasion Config → Generation ve Evaluation’ı yönlendirir
Domain → tüm modüllerin temelidir

---

# ⚠️ Known Boundaries

* Context Evaluation ayrı modül değil, Evaluation’ın parçasıdır
* Message Catalogs, Messaging modülünün parçasıdır
* Response Mapping, API katmanının parçasıdır
* Alternative reasoning, Decision modülünün alt sorumluluğudur

---


---

# Demo Layer

## Purpose

Demo layer allows non-technical users to test Kombinle without Postman.

## Responsibilities

- Provide basic UI controls
- Send request to `/api/v1/decision`
- Render recommendation output in readable cards
- Translate technical enum names into user-facing labels
- Show best outfit, layers, reasons and alternatives

## Invariants

- Demo layer must not contain outfit decision logic
- Demo layer only maps and presents API output
- Engine remains the source of truth

## Current Files

- `Kombinle.Api/wwwroot/demo.html`

## Current Scope

- Occasion selection
- Wardrobe profile selection
- Manual context selection
- Recommendation display
- Debug JSON display

## Out of Scope

- Login
- Database persistence
- Image upload
- Weather API
- Payment / subscription


--- 
## Planned Module: Style Semantics

Date: 2026-06-05
Purpose:
Represent style suitability separately from:

- Formality
- Thermal suitability
- Layer semantics

Initial Traits:
- BusinessAppropriate
- SmartCasualAppropriate
- CasualAppropriate

Future Extensions:
- Elegant
- Relaxed
- Minimal
- Statement