using Plugin.Sample.GenericTaxes.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.GenericTaxes.Pipelines
{
    [PipelineDisplayName("CreateComposerTemplatesPipeline")]
    public interface ICreateComposerTemplatesPipeline : IPipeline<CreateComposerTemplatesArgument, bool, CommercePipelineExecutionContext>
    {
    }
}