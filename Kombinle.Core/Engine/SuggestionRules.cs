using Kombinle.Core.Domain;
using Kombinle.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Engine
{
    public class SuggestionRules
    {
        public static Dictionary<Garment, List<Garment>> GetSuggestions(
            Combination combination,
            List<Garment> wardrobe)
        {
            var suggestions = new Dictionary<Garment, List<Garment>>();

            // 1️⃣ Renk çakışan parçaları bul
            var items = combination.Items;
            for (int i = 0; i < items.Count; i++)
            {
                for (int j = i + 1; j < items.Count; j++)
                {
                    if (ColorRules.IsClashing(items[i].ColorFamily, items[j].ColorFamily))
                    {
                        // her ikisi için alternatif öner
                        if (!suggestions.ContainsKey(items[i]))
                            suggestions[items[i]] = items[i].GetAlternatives(wardrobe);

                        if (!suggestions.ContainsKey(items[j]))
                            suggestions[items[j]] = items[j].GetAlternatives(wardrobe);
                    }
                }
            }

            // 2️⃣ Zorunlu kategori eksik ise
            // Bu adım basit: Kullanıcının dolabında o kategori var mı kontrol et
            // yoksa boş list, kullanıcıya alması gerektiğini gösterebilir
            return suggestions;
        }
    }
}
