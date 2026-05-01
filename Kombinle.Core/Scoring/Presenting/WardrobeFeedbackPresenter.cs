using Kombinle.Core.Domain;
using Kombinle.Core.Scoring.Context;
using System.Collections.Generic;
using System.Linq;

namespace Kombinle.Core.Scoring.Presenting
{
    public record WardrobeFeedbackUx(string LineTr, string? DetailTr, string LineEn, string? DetailEn);

    public static class WardrobeFeedbackPresenter
    {
        // V1: max 1 feedback göster (spam yok)
        // V1: max 1 feedback göster (spam yok)
        public static WardrobeFeedbackUx? BuildUx(IReadOnlyList<WardrobeFeedback> feedback)
        {
            if (feedback == null || feedback.Count == 0) return null;

            var f = feedback[0];

            // Prefer catalog
            if (ContextMessageCatalog.TryGet(f.ContextWarningCode, out var msg))
            {
                // No viable combination: wardrobe gap tonu kullanma
                if (f.ContextWarningCode == "NO_VIABLE_COMBINATION")
                {
                    return new WardrobeFeedbackUx(
                        LineTr: msg.TitleTr,
                        DetailTr: msg.DetailTr,
                        LineEn: msg.TitleEn,
                        DetailEn: msg.DetailEn
                    );
                }

                // Default wardrobe gap UX
                return new WardrobeFeedbackUx(
                    LineTr: $"Bu koşullarda dolabında bir boşluk var: {msg.TitleTr}.",
                    DetailTr: msg.DetailTr,
                    LineEn: $"There’s a wardrobe gap for these conditions: {msg.TitleEn}.",
                    DetailEn: msg.DetailEn
                );
            }

            // Fallback
            if (f.ContextWarningCode == "NO_VIABLE_COMBINATION")
            {
                return new WardrobeFeedbackUx(
                    LineTr: "Bu parçalarla uyumlu bir kombin çıkaramadım.",
                    DetailTr: null,
                    LineEn: "I couldn’t build a compatible outfit from these pieces.",
                    DetailEn: null
                );
            }

            return new WardrobeFeedbackUx(
                LineTr: $"Bu koşullarda dolabında bir boşluk var: {f.ContextWarningCode}.",
                DetailTr: null,
                LineEn: $"There’s a wardrobe gap for these conditions: {f.ContextWarningCode}.",
                DetailEn: null
            );
        }
    }
}
