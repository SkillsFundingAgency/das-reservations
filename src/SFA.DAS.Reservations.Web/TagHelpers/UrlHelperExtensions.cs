using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Web.TagHelpers
{
    [HtmlTargetElement("a", Attributes = "asp-external-controller")]
    public class ExtendedAnchorTagHelper : AnchorTagHelper
    {
        private readonly IExternalUrlHelper _helper;

        [HtmlAttributeName("asp-external-action")]
        public string ExternalAction { get; set; }
        [HtmlAttributeName("asp-external-id")]
        public string ExternalId { get; set; }
        [HtmlAttributeName("asp-external-controller")]
        public string ExternalController { get; set; }
        [HtmlAttributeName("asp-external-subdomain")]
        public string ExternalSubDomain { get; set; }
        [HtmlAttributeName("asp-external-folder")]
        public string ExternalFolder { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context,output);

            output.Attributes.SetAttribute("href",_helper.GenerateUrl(ExternalId, ExternalController, ExternalAction, ExternalSubDomain, ExternalFolder));
        }

        public ExtendedAnchorTagHelper(IHtmlGenerator generator,IExternalUrlHelper helper) : base(generator)
        {
            _helper = helper;
        }
    }
}
