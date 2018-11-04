using System;
using System.Threading.Tasks;
using Plugin.Sample.GenericTaxes.Pipelines;
using Plugin.Sample.GenericTaxes.Pipelines.Arguments;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;

namespace Plugin.Sample.GenericTaxes.Commands
{
    public class CreateComposerTemplatesCommand : CommerceCommand
    {
        private readonly ICreateComposerTemplatesPipeline _pipeline;


        public CreateComposerTemplatesCommand(ICreateComposerTemplatesPipeline pipeline, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this._pipeline = pipeline;
        }

        public async Task<bool> Process(CommerceContext commerceContext, object parameter)
        {
            using (var activity = CommandActivity.Start(commerceContext, this))
            {
                var arg = new CreateComposerTemplatesArgument(parameter);
                var result = await this._pipeline.Run(arg, new CommercePipelineExecutionContextOptions(commerceContext));

                return result;
            }
        }
    }
}
