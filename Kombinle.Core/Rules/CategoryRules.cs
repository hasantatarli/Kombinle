using Kombinle.Core.Config;
using Kombinle.Core.Domain;
using Kombinle.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kombinle.Core.Rules
{
    public class CategoryRules : IRule
    {
        //public RuleResult Evaluate(Combination combination, Occasion occasion)
        //{
        //    var categories = combination.Items
        //        .Select(x => x.Category)
        //        .ToList();

        //    if (categories.Count != categories.Distinct().Count())
        //        return RuleResult.Fail("Aynı kategoriden birden fazla ürün var");

        //    // K5 – Zorunlu kategori eksik mi?
        //    foreach (var required in occasion.AnchorCategories)
        //    {
        //        if (!categories.Contains(required))
        //        {
        //            return RuleResult.Fail(
        //                $"Zorunlu kategori eksik: {required}"
        //            );
        //        }
        //    }


        //    return RuleResult.Pass();
        //}

        public RuleResult Evaluate(Combination combination, Occasion occasion)
        {
            //    var anchor = combination.Anchor;

            //    if (anchor == null)
            //    {
            //        return RuleResult.Fail("Anchor parça bulunamadı");
            //    }

            //    var rule = AnchorCompositionConfig.Rules
            //        .FirstOrDefault(r => r.Anchor == anchor.Category);

            //    if (rule == null)
            //    {
            //        // Anchor için kural tanımı yoksa serbest kabul et
            //        return RuleResult.Pass();
            //    }

            //    foreach (var requiredCategory in rule.RequiredCategories)
            //    {
            //        if (!combination.Items.Any(i => i.Category == requiredCategory))
            //        {
            //            return RuleResult.Fail(
            //                $"{anchor.Category} için {requiredCategory} gerekli");
            //        }
            //    }

            //    return RuleResult.Pass();
            //}
            return RuleResult.Pass();
        }
    }


}
