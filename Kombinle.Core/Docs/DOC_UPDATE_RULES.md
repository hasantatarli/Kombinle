1. ProjectBrain.md
Ne zaman güncellenir?

Sadece şu durumlarda:

* sistem amacı değişirse
* çekirdek mimari yaklaşım değişirse
* yeni bir çekirdek akış eklenirse
* projenin fazı değişirse

Ne zaman güncellenmez?
* küçük bug fix
* tek method değişikliği
* test ekleme
* küçük refactor

Nasıl güncellenir?

“rewrite” değil, diff tabanlı güncellenir.

Sorulacak prompt
Elimde mevcut `ProjectBrain.md` var. Son değişikliklere göre bunu tamamen yeniden yazma; sadece hangi bölümler güncellenmeli onları çıkar ve ardından güncellenmiş final sürümü üret.

Kurallar:

* Sadece çekirdek sistem amacı, prensipler, core flow, core modules, key decisions, constraints ve risks alanlarını değerlendir.
* Küçük implementation detaylarını ekleme.
* Tahmin yapma.
* Eğer güncelleme gerekmiyorsa bunu açıkça söyle.

Format:

1. What changed?
2. What stays valid?
3. Updated ProjectBrain.md


2. DecisionLog.md
Ne zaman güncellenir?

Şu an en sık güncellenecek dosya bu.

Her seferinde:

* mimari karar aldıysan
* davranış kuralı netleştiyse
* bir yaklaşımı reddettiysen
* “bu böyle kalmalı” dediysen
Ne zaman güncellenmez?
* sıradan kod değişikliği
* önemsiz naming/refactor
* geçici deneme

Nasıl güncellenir?

Yeni entry eklenir. Eski entry rewrite edilmez, sadece gerekirse not düşülür.

Sorulacak prompt
Bu çalışma içinde alınan yeni kararları `DecisionLog.md` için çıkar.

Kurallar:

* Sadece açıkça netleşmiş kararları yaz.
* Tahmin yapma.
* “Ne yaptık” değil, “neden böyle yapıyoruz” odaklı yaz.
* Aynı konudaki tekrarları birleştir.
* Eğer yeni karar yoksa açıkça söyle.

Format:

## <Short Title>

### Context

### Decision

### Reason

### Confidence


3. ModuleSpec.md
Ne zaman güncellenir?
* modül sorumluluğu değişirse
* modül ikiye bölünürse
* iki modül birleşirse
* input/output contract değişirse
* invariant değişirse
Ne zaman güncellenmez?
* iç implementation değiştiyse ama davranış aynıysa

* Nasıl güncellenir?

Sadece ilgili modül bölümü güncellenir. Tüm dosya baştan yazılmaz.

Sorulacak prompt
Mevcut `ModuleSpec.md` içindeki modülleri son değişikliklere göre gözden geçir.

Kurallar:

* Sadece modül seviyesi değişiklikleri çıkar.
* Implementation detayı verme.
* Sorumluluk, invariants, inputs/outputs veya boundaries değiştiyse bunu belirt.
* Değişmeyen modülleri tekrar yazma.
* Eğer değişiklik yoksa açıkça söyle.

Format:

1. Modules that changed
2. What changed in each
3. Updated module sections only


4. CODE_MAP.md
Ne zaman güncellenir?
* yeni klasör/modül geldiyse
* entry flow değiştiyse
* eski dosyalar legacy olduysa
* yeni API surface geldiyse
* aktif akış başka dosyalara taşındıysa

Ne zaman güncellenmez?
* method içi değişiklik
* aynı akış içinde küçük kod düzeltmesi

Nasıl güncellenir?

Harita güncellenir; “kesin unused” iddiası eklenmez.

Sorulacak prompt
Mevcut `CODE_MAP.md` dosyasını son yapıya göre güncelle.

Kurallar:

* Amaç dosya bazlı detay değil, aktif akış ve modül görünürlüğü.
* Sadece akışı, modül sınırlarını, entry point’leri ve status değişikliklerini güncelle.
* “Unused” diye kesin hüküm verme; sadece ACTIVE / LEGACY / INACTIVE / HISTORICAL / UNKNOWN kullan.
* Eğer aktif omurga değişmediyse bunu açıkça söyle.

Format:

1. Flow changes
2. Module/status changes
3. Updated CODE_MAP sections


5. CurrentTask.md
Ne zaman güncellenir?

En sık güncellenen dosya budur.

* her yeni aktif işe başlarken
* iş değiştiğinde
* scope değiştiğinde
* o task bittiğinde yeni task açılır

Ne zaman güncellenmez?
* aynı task devam ederken küçük ilerlemelerde

Nasıl güncellenir?
Eski task overwrite edilebilir. Bu yaşayan dosya.

Sorulacak prompt

Mevcut bağlama göre `CurrentTask.md` oluştur veya güncelle.

Kurallar:

* Sadece tek aktif task yaz.
* Genel roadmap çıkarma.
* Kapsamı dar tut.
* In scope / out of scope net olsun.
* Eğer aktif task değişmediyse mevcut task’ın hâlâ geçerli olup olmadığını söyle.

Format:

## Task Name

## Goal

## Why This Matters

## In Scope

## Out of Scope

## Relevant Modules

## Relevant Files

## Constraints

## Success Criteria

## Notes



6. Handoff.md veya oturum sonu notu
Ne zaman güncellenir?

Her çalışma oturumu sonunda.

Nasıl güncellenir?

Kısa olur. 5 maddeyi geçmez.

Prompt

Bu çalışma oturumu için kısa bir handoff özeti çıkar.

Kurallar:

* Kısa yaz.
* Ne yaptık / ne değişmedi / sıradaki net adım ne şeklinde olsun.
* Tahmin yapma.
* Genel özet değil, bu oturuma özel yaz.

Format:

## What We Did

* ...

## What Did Not Change

* ...

## Next Step

* ...

## Open Risks / Notes

* ...


En pratik güncelleme ritmi

Ben olsam şöyle yürütürüm:

Her task başında
* CurrentTask.md

Task sırasında karar çıktıysa
* DecisionLog.md

Task bittiğinde
* Handoff.md

Haftalık veya önemli milestone sonrası
* CODE_MAP.md
* ModuleSpec.md

Büyük yön değişiminde
* ProjectBrain.md

Çok kısa karar tablosu
* Bug fix / küçük davranış değişimi → CurrentTask + gerekirse DecisionLog
* Yeni invariant / kural → DecisionLog + ModuleSpec
* Yeni modül / flow değişimi → CODE_MAP + ModuleSpec
* Mimari yön değişimi → ProjectBrain
* Oturum kapatma → Handoff


