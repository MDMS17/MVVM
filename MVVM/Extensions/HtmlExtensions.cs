using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mcpdandpcpa.Extensions
{
    public static class HtmlExtensions
    {
        public static IHtmlContent DisabledFor(this IHtmlHelper htmlHelper,
                                              bool booleanLinkName)
        => new HtmlString(booleanLinkName ? "disabled='disabled'" : "");
    }
}

