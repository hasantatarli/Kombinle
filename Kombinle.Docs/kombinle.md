Kombinle – Project Brain v2
1. Project Overview

Kombinle kişisel bir outfit decision engine’dir.

Amaç:

Kullanıcının dolabındaki kıyafetleri ve bulunduğu koşulları dikkate alarak:

hızlı

güvenli

açıklanabilir

kombin önerileri üretmek.

Bu proje:

AI-first değil

rule-based decision engine üzerine kuruludur

ve gerektiğinde AI bileşenleri eklenebilir.

Temel prensip:

Kombinle bir moda danışmanı değil,
bir giyilebilir kombin karar motorudur.

2. Product Vision

Kombinle şu sorunu çözer:

Sabah insanlar şu problemi yaşar:

Ne giysem?

Hava nasıl?

Mekan uygun mu?

Kombinle bu kararı otomatikleştirir.

3. 60 Second User Experience

Kullanıcı 60 saniyede değer görmelidir.

Akış:

App açılır

Occasion seçilir

Context otomatik gelir

Kullanıcı 1-2 parça ekler

Sistem kombin üretir

Amaç:

Kullanıcıyı yormadan çalıştığını göstermek.

4. Product Strategy

Önce ürün çalışmalı.

Sonra:

dolap genişletme

öneri kalitesi

AI classifier

gelir.

Bu yüzden ilk aşama:

Decision Engine.

5. Core Design Philosophy

Engine şu özelliklere sahiptir:

Deterministic
Explainable
Testable
Context aware

Amaç:

Kullanıcıya neden o kombin önerildiğini anlatabilmek.

6. System Architecture
User Input
   │
   ▼
Combination Generator
   │
   ▼
Combination Scorer
   │
   ▼
Ranking
   │
   ▼
Decision Summary
   │
   ▼
API Response
7. Project Structure
Kombinle
│
├── Kombinle.Api
│
├── Kombinle.Api.Contracts
│
├── Kombinle.Core
│
└── Kombinle.Core.Tests
8. Core Project Structure
Core
│
├── Domain
├── Context
├── Generation
├── Engine
├── Scoring
├── Rules
└── Resources
9. Important Domain Models
Garment

Represents a clothing item.

Properties:

Category
ColorFamily
Formality

Optional:

ShoeProperties

Slot

Outfit roles.

Example:

Anchor
Top
Bottom
Shoes
Outerwear
SlotRequirement

Slot requirement types:

Hard
Soft
Optional

Hard → kombin üretilemez
Soft → kombin üretilir ama kalite düşer
Optional → tercih edilir

10. Occasion System

Occasion tanımlar:

formalite

slot yapısı

default context

Example:

BusinessMeeting_Formal
CasualWeekend
Interview_Formal

Occasion artık JSON’dan yüklenebilir.

11. Context System

Context üç ana faktörden oluşur:

Weather
Setting
TimeOfDay

Context engine şu çıktıları üretir:

ContextDelta
ContextWarnings
ContextReasons

12. Context Message System

Warning kodları kullanıcı diline çevrilir.

Example:

RAIN_SUEDE_SHOES

↓

Yağmurda süet ayakkabı riskli

Mesajlar JSON’dan yüklenebilir.

13. Wardrobe Feedback

Engine sadece kombin üretmez.

Ayrıca dolap hakkında öneri verir.

Example:

MissingItemForContext
IncompleteOutfit
14. Combination Generator

Generator şu şekilde çalışır:

1 Anchor seçilir
2 Supporter pool oluşturulur
3 Primary kombin üretilir
4 Controlled variants oluşturulur

Guardrails:

MaxVariantsPerAnchor = 2
MaxAltsPerSlot = 1
15. Combination Scoring

Score şu faktörlerden oluşur:

Color harmony
Formality match
Context penalties
Preferred anchor colors

16. Decision Summary

Engine sonucu şu verileri üretir:

Best combination
Alternatives
Wardrobe feedback
Context health
Debug metrics

17. Fallback Strategy

Kombin üretilemezse sistem kullanıcıyı boş döndürmez.

Fallback outfit üretir.

Öncelik:

Anchor
Shoes
Top

Amaç:

Kullanıcıyı terk etmemek.

18. API Layer

Endpoint:

POST /api/v1/decision

Input:

occasionId
context
items

Output:

decision
alternatives
wardrobeFeedback
debug

19. Testing Philosophy

Her değişiklik sonrası:

dotnet test

çalıştırılır.

Amaç:

regresyonu önlemek

engine stabil kalması

20. Development Methodology

Bu projede ChatGPT şu rolleri üstlenir:

AI Co-Founder
Software Architect
Code Reviewer
Product Mentor

21. Interaction Pattern

Developer:

Hasan

Profile:

15 yıl yazılım deneyimi
DB ve backend güçlü
product geliştirmeye odaklı

ChatGPT’den beklenen:

açık teknik yönlendirme

mimari öneriler

ürün eleştirisi

hızlı debug

22. Current Project Status

Engine çalışıyor.

Tamamlananlar:

Combination generator
Scoring engine
Context system
Alternative reasoning
Wardrobe feedback
JSON configuration
API integration

Tests:

All tests passing.

23. Known Improvements

Response mapper cleanup
Duplicate anchor fix
Alternative text formatting

24. Next Phases

Phase 1
API stabilization

Phase 2
Photo classifier

Phase 3
Wardrobe persistence

Phase 4
Mobile UI

25. Long Term Vision

Kombinle şu noktaya ulaşabilir:

personal wardrobe assistant
style intelligence engine
shopping advisor

26. Key Engineering Principle

Engine şu prensiple tasarlanır:

Make it reliable first
Make it smart later

27. How to Resume Development

Yeni chat başlatıldığında:

1 Bu doküman paylaşılır
2 ChatGPT bağlamı yükler
3 Development kaldığı yerden devam eder