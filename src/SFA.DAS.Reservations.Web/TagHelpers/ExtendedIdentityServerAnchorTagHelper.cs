using System.Net;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Infrastructure.Configuration;

namespace SFA.DAS.Reservations.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes= "asp-identity-server-chg-email")]
    public class ExtendedIdentityServerAnchorTagHelper : AnchorTagHelper
    {
        private readonly IHtmlGenerator _generator;
        private readonly ReservationsWebConfiguration _webOptions;
        private readonly IdentityServerConfiguration _options;

        [HtmlAttributeName("asp-identity-server-chg-email")]
        public string ChangeEmailLink { get; set; }
        [HtmlAttributeName("asp-identity-server-chg-pwd")]
        public string ChangePwdLink { get; set; }
        [HtmlAttributeName("asp-external-id")]
        public string ExternalId { get; set; }

        public ExtendedIdentityServerAnchorTagHelper(IHtmlGenerator generator, IOptions<IdentityServerConfiguration> options, IOptions<ReservationsWebConfiguration> webOptions) : base(generator)
        {
            _generator = generator;
            _webOptions = webOptions.Value;
            _options = options.Value;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (!string.IsNullOrEmpty(ChangeEmailLink))
            {
                var url = WebUtility.UrlEncode($"{ViewContext.HttpContext.Request.Scheme}://{ViewContext.HttpContext.Request.Host}{ViewContext.HttpContext.Request.PathBase}/{ExternalId}/service/email/change");
                
                output.Attributes.SetAttribute("href", _options.ChangeEmailLinkFormatted() + url);
            }
            else if (!string.IsNullOrEmpty(ChangePwdLink))
            {
                var url = WebUtility.UrlEncode($"{ViewContext.HttpContext.Request.Scheme}://{ViewContext.HttpContext.Request.Host}{ViewContext.HttpContext.Request.PathBase}/{ExternalId}/service/password/change");
                output.Attributes.SetAttribute("href", _options.ChangePasswordLinkFormatted() + url);
            }
            
        }
    }
}