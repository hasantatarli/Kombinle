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

Best kombin jacket/blazer içerdiğinde, ceketsiz alternatif generic reason ile açıklanıyordu.

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