using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace NewsSearch.Core
{
    public enum EnumSources
    {
        [Description("The Guardian")]
        TheGuardian = 100,
        [Description("Reddit")]
        Reddit = 101,
        [Description("Social Mention")]
        SocialMention = 102,
        [Description("Wikipedia")]
        Wikipedia = 103,
        [Description("YouTube")]
        YouTube = 104
    }
}