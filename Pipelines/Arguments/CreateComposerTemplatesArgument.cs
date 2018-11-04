using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;

namespace Plugin.Sample.GenericTaxes.Pipelines.Arguments
{
    public class CreateComposerTemplatesArgument : PipelineArgument
    {
        public CreateComposerTemplatesArgument(object parameter)
        {
            Condition.Requires(parameter).IsNotNull("The parameter can not be null");
            this.Parameter = parameter;
        }

        public object Parameter { get; set; }
    }
}