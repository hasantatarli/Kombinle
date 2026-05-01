using Kombinle.Core.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Scoring.Context
{
    //public record ContextMessage(string TitleTr, string TitleEn, string DetailTr, string DetailEn);

    public static class ContextMessageCatalog
    {
        private static readonly Dictionary<string, ContextMessage> _map = BuildMap();

        //{
        //    ["RAIN_SUEDE_SHOES"] = new ContextMessage(
        //        Kind: MessageKind.ContextWarning,
        //        TitleTr: "Yağmurda süet ayakkabı riskli",
        //        TitleEn: "Suede shoes are risky in rain",
        //        DetailTr: "Süet malzeme suyu çeker ve hızlı yıpranabilir. Yağmurda daha dayanıklı bir ayakkabı daha güvenli olur.",
        //        DetailEn: "Suede absorbs water and can wear quickly. In rain, a more durable shoe is safer."
        //    ),

        //            ["SOFT_ANCHOR_MISSING"] = new ContextMessage(
        //        Kind: MessageKind.WardrobeFeedback,
        //        TitleTr: "Ceket kombini güçlendirir",
        //        TitleEn: "A jacket strengthens the outfit",
        //        DetailTr: "Bu plan için ceket şart değil ama eklersen daha derli toplu ve resmi görünür.",
        //        DetailEn: "A jacket isn’t required, but it makes the outfit look more structured and formal."
        //    ),
        //};

        private static Dictionary<string, ContextMessage> BuildMap()
        {
            // 1) Önce JSON'dan dene
            try
            {
                var baseDir = AppContext.BaseDirectory;
                var path = Path.Combine(baseDir, "Resources", "context_messages.json");

                if (File.Exists(path))
                {
                    var loaded = ContextMessageCatalogLoader.LoadFromJsonFile(path);
                    if (loaded.Count > 0)
                        return loaded;
                }
            }
            catch
            {
                // Bilinçli olarak yutuyoruz
                // Fallback çalışmalı
            }


            // 2) Fallback (hardcoded, güvenli)
            return new Dictionary<string, ContextMessage>(StringComparer.OrdinalIgnoreCase)
            {
                ["RAIN_SUEDE_SHOES"] = new ContextMessage(
                    Kind: MessageKind.ContextWarning,
                    TitleTr: "Yağmurda süet ayakkabı riskli",
                    TitleEn: "Suede shoes are risky in rain",
                    DetailTr: "Süet malzeme suyu çeker ve hızlı yıpranabilir. Yağmurda daha dayanıklı bir ayakkabı daha güvenli olur.",
                    DetailEn: "Suede absorbs water and can wear quickly. In rain, a more durable shoe is safer."
                ),

                ["RAIN_LOW_WATER_SHOES"] = new ContextMessage(
                    Kind: MessageKind.ContextWarning,
                    TitleTr: "Düşük su dayanımı",
                    TitleEn: "Low water resistance",
                    DetailTr: "Yağmurda su dayanımı düşük ayakkabılar rahatsız edebilir.",
                    DetailEn: "In rain, low water-resistance shoes can be uncomfortable."
                ),

                ["OUTDOOR_NO_OUTERWEAR"] = new ContextMessage(
                    Kind: MessageKind.ContextWarning,
                    TitleTr: "Dışarı için dış katman eksik",
                    TitleEn: "Missing outerwear for outdoors",
                    DetailTr: "Dışarı çıkışta dış katman (ceket/coat) konfor ve koruma sağlar.",
                    DetailEn: "For outdoors, an outer layer (jacket/coat) improves comfort and protection."
                ),

                ["SOFT_ANCHOR_MISSING"] = new ContextMessage(
                    Kind: MessageKind.WardrobeFeedback,
                    TitleTr: "Ceket kombini güçlendirir",
                    TitleEn: "A jacket strengthens the outfit",
                    DetailTr: "Bu plan için ceket şart değil ama eklersen daha derli toplu ve resmi görünür.",
                    DetailEn: "A jacket isn’t required, but it makes the outfit look more structured and formal."
                )
            };

        }
        public static bool TryGet(string code, out ContextMessage msg) => _map.TryGetValue(code, out msg!);
    }
}