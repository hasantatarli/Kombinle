# Kombinle — Feedback Driven Architecture Direction

## Neden Bu Doküman?

İlk gerçek kullanıcı feedback’leri sonrasında ürünün:

* hangi problemlere gerçekten dokunduğu,
* hangi alanların güçlendirilmesi gerektiği,
* hangi fikirlerin backlog’da kalması gerektiği,
* hangi mimari kararların korunması gerektiği

netleşmeye başladı.

Bu doküman:

* ürün yönünü kaybetmemek,
* yanlış feature expansion’a gitmemek,
* deterministic / explainable yaklaşımı korumak

amacıyla oluşturuldu.

---

# 1. Korunacak Çekirdek Ürün Fikri

Kombinle:

> “Kullanıcının kendi dolabı için çalışan deterministic outfit decision engine”

olmaya devam etmeli.

Ürün:

* generic outfit inspiration app’e,
* Pinterest benzeri moodboard ürününe,
* AI stylist chatbot’una

dönmemeli.

Ana değer:

> “Benim dolabımdan ne giyeceğime hızlı karar vermek.”

---

# 2. İlk Feedbacklerden Çıkan Ana Problemler

## 2.1 Onboarding Friction

Kullanıcılar:

* kıyafetleri tek tek eklemek istemiyor
* fotoğraf çekmekle uğraşmak istemiyor
* dolap oluşturmayı yorucu buluyor

Ancak:

* ürün fikrini değerli buluyorlar
* hızlı karar verme kısmını faydalı görüyorlar

Bu:

> “Product value var, onboarding ağır.”

anlamına geliyor.

---

## 2.2 Variety / Intelligence Perception

Feedback:

* “Hep aynı şeyi öneriyor gibi.”
* “Business Meeting ve Interview aynı.”
* “Restart edince yine aynı.”

Bu:

* engine bug’ı değil,
* deterministic davranışın kullanıcı tarafından “akıllı değil” gibi algılanması.

---

## 2.3 Context Depth Eksikliği

Feedback:

* Mevsim yok
* Yağmur sadece kaban ekliyor
* Güneşli kış gününde t-shirt önermemeli

Bu:

> Context model yüzeysel kalıyor.

anlamına geliyor.

---

## 2.4 Outfit Completeness

Feedback:

* Çanta eksik
* Bazı kombinler tamamlanmamış hissettiriyor

Bu:

> Outfit completeness perception

problemi.

---

## 2.5 Persona Coverage

Feedback:

* Modest / tesettür kullanıcıları düşünülmemiş
* Eşarp bazı kullanıcılar için accessory değil mandatory item

Bu:

> Persona-specific slot rules

ihtiyacını gösteriyor.

---

# 3. Kritik Mimari Kararlar

## 3.1 Deterministic Yapı Korunacak

Kombinle:

* explainable,
* deterministic,
* rule-driven

kalmaya devam edecek.

AI:

* karar verici olmayacak,
* yardımcı katman olarak kullanılacak.

---

## 3.2 AI Sadece Input Assistant Olacak

AI:

* kıyafeti tanır
* kategori önerir
* renk önerir
* formality önerir

Ama:

* kullanıcı confirm eder
* engine deterministic karar verir

Doğru model:

```text
AI assists
User confirms
Engine decides
```

---

## 3.3 “Benim Dolabım” Fikri Korunacak

Preset wardrobe:

* sadece demo/onboarding için kullanılabilir
* ürünün ana çalışma mantığı olmayacak

Çünkü ürünün ana değeri:

> “Kendi dolabımı yönetiyor.”

---

# 4. Yakın Vadeli Yol Haritası

## Faz 1 — Stabilization & UX

Odak:

* Variety perception
* Better explanation
* Better context understanding

### Yapılacaklar

#### A. Primary Selection Rotation

Amaç:

* deterministic yapıyı bozmadan
* sürekli aynı kombin hissini azaltmak

Yaklaşım:

* Top N güçlü kombin arasında
* session/request bazlı hafif rotation
* score farkı büyükse rotation yapılmamalı

---

#### B. Alternative-first UX

Alternatifler:

* gerçek değer taşımalı
* sadece “alternatif 1” olmamalı

Örnek:

* Daha güvenli seçenek
* Daha sade kullanım
* Daha şık görünüm

---

#### C. Better Explainability

Kullanıcı:

> “Neden bunu seçti?”

sorusunun cevabını anlamalı.

Özellikle:

* renk uyumu
* context etkisi
* formality

konuları daha görünür hale getirilmeli.

---

## Faz 2 — Context Expansion

### A. Season Support

Yeni context:

```text
Season:
- Winter
- Summer
- Spring
- Autumn
```

Weather’dan ayrı olmalı.

Çünkü:

```text
Clear + Winter != Clear + Summer
```

---

### B. Deeper Context Effects

Rain:

* sadece outerwear değil
* shoes
* fabrics
* bottoms

üzerinde de etkili olmalı.

---

## Faz 3 — Assisted Wardrobe Creation

En büyük onboarding problemi burada çözülecek.

### Sources

* Camera
* Gallery
* Instagram (future)

---

### Flow

1. User uploads photo
2. AI detects garments
3. AI suggests attributes
4. User confirms
5. Items saved to wardrobe

---

### Rule

AI assists wardrobe input,
not outfit decisions.

---

## Faz 4 — Persona & Completeness Expansion

### A. Modest / Covered Style Support

Yeni slot örneği:

```json
{
  "slot": "Headwear",
  "level": "Hard",
  "allowedCategories": ["Scarf"]
}
```

Amaç:

* eşarp bazı kullanıcılar için mandatory item olsun
* accessory gibi davranmasın

---

### B. Outfit Completeness & Companion Accessories

Kullanıcı feedback’i:

* “Elbise yanında çanta da kullanırım.”
* “Bazı kombinler çantasız eksik hissettiriyor.”

Bu nedenle bazı parçalar:

* core outfit item değil,
* ama companion recommendation olarak önerilebilir.

Örnek:

```text
Bu kombinle şunlar da iyi eşleşebilir:
- Siyah küçük çanta
- Minimal takı
- İnce kemer
```

Buradaki amaç:

* kullanıcıyı accessory overload’a boğmadan
* kombinin “tamamlanmış” hissettirmesi.

Önemli ayrım:

* Core outfit engine deterministic kalmalı
* Companion accessories secondary recommendation layer olmalı

Opsiyonel accessory kategorileri:

* Bag
* Watch
* Jewelry
* Belt
* Scarf

Not:
Bazı persona’larda accessory optional değil mandatory olabilir.
Örneğin modest / covered style kullanıcılarında headwear accessory değil zorunlu kombin parçası olabilir.

---

# 5. Şimdilik Yapılmayacaklar

## Avoid Premature Expansion

Henüz yapılmaması gerekenler:

* Full AI stylist
* Shopping-first model
* Marketplace
* Autonomous AI decisions
* Over-randomized outfit selection
* Massive taxonomy expansion
* Admin panel
* Social feed

---

# 6. En Kritik Ürün İçgörüsü

Kullanıcılar:

> “Bu kötü fikir.”

demiyor.

Şunu söylüyor:

> “Bu işe yarar ama kullanımı kolaylaşmalı ve daha akıllı hissettirmeli.”

Bu çok önemli bir sinyal.

Ürün problemi:

* value eksikliği değil,
* friction ve perception.
