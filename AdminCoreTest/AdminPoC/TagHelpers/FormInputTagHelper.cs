#nullable enable
namespace AdminPoC.TagHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.AspNetCore.Razor.TagHelpers;

    [HtmlTargetElement("formInput", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class FormInputTagHelper : TagHelper
    {
        [HtmlAttributeName("for-name")] public string Name { get; set; }

        [HtmlAttributeName("for-type")] public Type Type { get; set; }

        [HtmlAttributeName("with-label")] public string LabelText { get; set; }

        [HtmlAttributeName("with-value")] public object Value { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("name", this.Name);
            if (this.Type.IsEnum)
            {
                this.PrepareEnum(output);
                return;
            }

            output.TagName = "input";
            output.Attributes.SetAttribute("value", this.Value);

            if (this.Type == typeof(string))
            {
                output.Attributes.SetAttribute("type", "text");
            }
            else if (this.Type == typeof(DateTime))
            {
                output.Attributes.SetAttribute("type", "datetime");
            }
            else if (this.Type == typeof(int))
            {
                output.Attributes.SetAttribute("type", "number");
            }
        }

        private void PrepareEnum(TagHelperOutput output)
        {
            output.TagName = "select";

            var values = Enum.GetValues(this.Type);
            var names = Enum.GetNames(this.Type);
            var options =
                values
                    .Cast<object>()
                    .Select((t, i) => new
                    {
                        Text = names[i],
                        Value = values.GetValue(i)?.ToString(),
                    })
                    .Select(x =>
                        x.Value == this.Value.ToString()
                            ? $"<option value='{x.Value}' selected>{x.Text}</option>"
                            : $"<option value='{x.Value}'>{x.Text}</option>")
                    .ToList();
            output.Content.SetHtmlContent(
                string.Join("", options));
        }
    }
}