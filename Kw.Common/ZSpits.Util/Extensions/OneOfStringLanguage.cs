﻿using Kw.Common.OneOf;
using static Kw.Common.ZSpitz.Util.Language;

namespace Kw.Common.ZSpitz.Util {
    public static class OneOfStringLanguageExtensions {
        public static Language? ResolveLanguage(this OneOf<string, Language?> languageArg) {
            if (languageArg.IsT1) { return languageArg.AsT1; }
            return languageArg.AsT0 switch {
                LanguageNames.CSharp => CSharp,
                LanguageNames.VisualBasic => VisualBasic,
                _ => null,
            };
        }
    }
}
