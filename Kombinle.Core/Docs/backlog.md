🧩 Backlog: Proper Localization & Display Mapping

Problem

Garment isimleri backend’de raw enum/value olarak kullanılıyor (Blazer, Shirt vs.)
shortTr alanı tam lokalize değil (mix language)
UI’da string replace ile geçici çözüm uygulanıyor

Goal

Tüm user-facing text’ler gerçek localization pipeline üzerinden gelsin
UI’da string manipulation olmasın

Scope

Garment display names:
Category → display name mapping (catalog/json)
Color → display name mapping
Outfit text generation:
shortTr yerine:
shortCode + param model
veya fully localized builder
Multi-language support:
tr / en desteklenmeli
default language config üzerinden belirlenmeli
Context / labels:
ContextHealth
Occasion names
UI labels

Non-goals (şimdilik)

Dynamic language switching
External i18n service


---

Backlog: Catalog-driven garment taxonomy

Problem

Category, color ve garment type değerleri şu anda enum gibi sabit yapılarla temsil ediliyor.
Ürün gamı genişlediğinde yeni kategori eklemek kod değişikliği gerektirebilir.
Localization/display mapping de bu enum değerlerine bağımlı hale geliyor.

Goal

Garment taxonomy config/catalog üzerinden yönetilebilir hale gelsin.
Engine deterministic kalsın.
UI ve API display text üretimini enum string’lerine bağımlı yapmayalım.

Scope

Category catalog
Color catalog
Category role mapping: top, bottom, shoes, outerwear, dress, accessory
Display names: tr, en
Engine-safe internal ids
Backward compatibility for current static profiles

Non-goal now

Admin panel
Dynamic runtime category creation
Marketplace/product inventory model

Benim önerim: MVP’de enum bırak, ama category isimlerini ürün gerçeği sanma. Onlar şu an engine’in iç dili. Gerçek ürün dili ileride catalog’dan gelmeli.

---

Backlog: Garment traits model

Problem:
Aynı category içindeki parçalar çok farklı davranabilir. Özellikle dress, shoes, jacket gibi kategorilerde model/fit/fabric farkı karar kalitesini etkiler.

Goal:
Category’yi büyütmek yerine garment traits eklemek.

Examples:
Length, sleeve, fit, fabric, pattern, seasonality, styleTag, heelHeight, waterResistance.

Rule:
Category karar yapısını belirler, traits kalite/context değerlendirmesini iyileştirir.

Net kararım: Model isimlerini category’ye gömmeyelim; trait olarak büyütelim.

---

Backlog: Context health product messaging

Problem:
bestContextHealth şu anda teknik durum sinyali olarak geliyor: Good / Okay / Poor. Demo bunu UI’da çeviriyor, fakat kullanıcıya gösterilecek açıklama backend/config tarafından yönetilmeli.

Goal:
Context health için kullanıcı dostu mesajlar catalog’dan gelsin.

Scope:

Good / Okay / Poor display label
TR/EN açıklama metni
Decision response içinde user-facing context summary
Demo’nun string üretmemesi, sadece gelen metni basması

Example:

{
  "OKAY": {
    "labelTr": "Uygun",
    "hintTr": "Kombin kullanılabilir, ancak bazı koşullarda daha iyi seçenekler olabilir.",
    "labelEn": "Suitable",
    "hintEn": "This outfit works, but there may be stronger options for the current conditions."
  }
}

Net kararım: Şu an demo’ya gömme. Backlog’a yaz. Ürünleştirme aşamasında backend/config’e taşı.