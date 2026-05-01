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

    public static class ColorRules
    {
        private const string ColorRulesRelativePath = "Resources/color_rules.json";

        private static readonly Lazy<ColorRulesConfig> _config = new(() =>
        {
            var path = Path.Combine(AppContext.BaseDirectory, ColorRulesRelativePath);
            return ColorRulesConfigLoader.LoadFromJsonFile(path);
        });

        public static bool IsClashing(ColorFamily a, ColorFamily b) =>   GetCompatibility(a, b) == ColorCompatibility.Clash;

        public static bool IsNeutral(ColorFamily c) => _config.Value.NeutralColors.Contains(c);

        public static bool IsBright(ColorFamily c) => _config.Value.BrightColors.Contains(c);

        public static ColorCompatibility GetCompatibility(ColorFamily a, ColorFamily b)
        {
            if (IsPairIn(_config.Value.ClashPairs, a, b))
                return ColorCompatibility.Clash;

            if (IsPairIn(_config.Value.WeakPairs, a, b))
                return ColorCompatibility.WeakMatch;

            if (IsPairIn(_config.Value.StrongPairs, a, b))
                return ColorCompatibility.StrongMatch;

            if (IsNeutral(a) && IsNeutral(b))
                return ColorCompatibility.StrongMatch;

            if (IsNeutral(a) || IsNeutral(b))
                return ColorCompatibility.Acceptable;

            return ColorCompatibility.Acceptable;
        }

        private static bool IsPairIn(
            IEnumerable<ColorPairRule> pairs,
            ColorFamily a,
            ColorFamily b)
        {
            return pairs.Any(p =>
                (p.A == a && p.B == b) ||
                (p.A == b && p.B == a));
        }
    }

}
