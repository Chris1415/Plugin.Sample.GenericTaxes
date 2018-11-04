using System;
using System.Threading.Tasks;
using Sitecore.Commerce.Core;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.ManagedLists;
using Sitecore.Commerce.Plugin.Views;
using Sitecore.Commerce.EntityViews;
using System.Collections.Generic;
using Plugin.Sample.GenericTaxes.Pipelines.Arguments;
using Plugin.Sample.GenericTaxes.Policies;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    [PipelineDisplayName("CreateComposerTemplatesBlock")]
    public class CreateGenericTaxesComposerTemplatesBlock : PipelineBlock<CreateComposerTemplatesArgument, bool, CommercePipelineExecutionContext>
    {
        private readonly CommerceCommander _commerceCommander;
        public CreateGenericTaxesComposerTemplatesBlock(CommerceCommander commerceCommander)
        {
            _commerceCommander = commerceCommander;
        }

        public override async Task<bool> Run(CreateComposerTemplatesArgument arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull($"{this.Name}: The argument can not be null");

            //Create the template, add the view and the properties
            string itemId = $"Composer-{Guid.NewGuid()}";

            var composerTemplate = new ComposerTemplate(GenericTaxesConstants.ComposerViewValue);
            composerTemplate.GetComponent<ListMembershipsComponent>().Memberships.Add(CommerceEntity.ListName<ComposerTemplate>());

            composerTemplate.LinkedEntities = new List<string>() { "Sitecore.Commerce.Plugin.Catalog.SellableItem" };

            composerTemplate.Name = "GenericTaxes";
            composerTemplate.DisplayName = "Generic Taxes";

            var composerTemplateViewComponent = composerTemplate.GetComponent<EntityViewComponent>();
            var composerTemplateView = new EntityView
            {
                Name = "Generic Taxes",
                DisplayName = "GenericTaxes",
                DisplayRank = 0,
                ItemId = itemId,
                EntityId = composerTemplate.Id
            };

            GenericTaxPolicy taxPolicy = context.GetPolicy<GenericTaxPolicy>();
            AvailableSelectionsPolicy availableSelectionsPolicy = new AvailableSelectionsPolicy();

            foreach (decimal whiteListEntry in taxPolicy.Whitelist)
            {
                availableSelectionsPolicy.List.Add(new Selection()
                {
                    Name = whiteListEntry.ToString(),
                    DisplayName = whiteListEntry.ToString(),
                });
            }

            composerTemplateView.Properties.Add(new ViewProperty()
            {
                DisplayName = taxPolicy.TaxFieldName,
                Name = taxPolicy.TaxFieldName,
                OriginalType = "System.String",
                RawValue = string.Empty,
                Value = string.Empty,
                Policies = new List<Policy>()
                {
                    availableSelectionsPolicy
                }
            });

            composerTemplateViewComponent.View.ChildViews.Add(composerTemplateView);
            var persistResult = await this._commerceCommander.PersistEntity(context.CommerceContext, composerTemplate);

            return await Task.FromResult(true);
        }
    }
}