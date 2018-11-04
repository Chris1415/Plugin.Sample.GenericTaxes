using Microsoft.Extensions.Logging;
using Plugin.Sample.GenericTaxes.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Pipelines;

namespace Plugin.Sample.GenericTaxes.Pipelines
{
    public class CreateComposerTemplatesPipeline : CommercePipeline<CreateComposerTemplatesArgument, bool>, ICreateComposerTemplatesPipeline
    {
        public CreateComposerTemplatesPipeline(IPipelineConfiguration<ICreateComposerTemplatesPipeline> configuration, ILoggerFactory loggerFactory)
            : base(configuration, loggerFactory)
        {
        }
    }
}