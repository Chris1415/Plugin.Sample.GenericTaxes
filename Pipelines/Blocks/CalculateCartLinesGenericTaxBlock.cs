using Microsoft.Extensions.Logging;
using Plugin.Sample.GenericTaxes.Policies;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Commerce.Plugin.Fulfillment;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sample.GenericTaxes.Pipelines.Blocks
{
    [PipelineDisplayName("GenericTaxes.Block.CalculateCartLinesGenericTaxBlock")]
    public class CalculateCartLinesGenericTaxBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(string.Format("{0}: The cart can not be null", this.Name));
            Condition.Requires(arg.Lines).IsNotNull(string.Format("{0}: The cart lines can not be null", this.Name));

            if (!arg.Lines.Any())
            {
                Task.FromResult(arg);
            }

            List<CartLineComponent> list = arg.Lines
                .Where((line =>
                    {
                        return line != null
                         ? line.HasComponent<FulfillmentComponent>()
                         : false;
                    }))
                .Select((l => l))
                .ToList();

            if (!list.Any())
            {
                context.Logger.LogDebug(string.Format("{0} - No lines to calculate tax on", this.Name));
                return Task.FromResult(arg);
            }

            string currencyCode = context.CommerceContext.CurrentCurrency();
            GenericTaxPolicy policy1 = context.GetPolicy<GenericTaxPolicy>();
            GlobalPricingPolicy policy2 = context.GetPolicy<GlobalPricingPolicy>();

            context.Logger.LogDebug(string.Format("{0} - Policy:{1}", this.Name, policy1.TaxCalculationEnabled));

            Decimal defaultItemTaxRate = policy1.DefaultItemTaxRate;

            context.Logger.LogDebug(string.Format("{0} - Item Tax Rate:{1}", this.Name, defaultItemTaxRate));

            foreach (CartLineComponent cartLineComponent in list)
            {
                if (policy1.TaxExemptTagsEnabled && cartLineComponent.HasComponent<CartProductComponent>())
                {
                    IList<Tag> tags = cartLineComponent.GetComponent<CartProductComponent>().Tags;
                    Func<Tag, string> func = (t => t.Name);
                    Func<Tag, string> selector = null;
                    if (tags.Select(selector).Contains(policy1.TaxExemptTag, StringComparer.InvariantCultureIgnoreCase))
                    {
                        context.Logger.LogDebug(string.Format("{0} - Skipping Tax Calculation for product {1} due to exempt tag", (object)this.Name, (object)cartLineComponent.ItemId), Array.Empty<object>());
                        continue;
                    }
                }

                Decimal num = cartLineComponent.Adjustments.Where((a => a.IsTaxable)).Aggregate(Decimal.Zero, ((current, adjustment) => current + adjustment.Adjustment.Amount));

                context.Logger.LogDebug(string.Format("{0} - SubTotal:{1}", this.Name, cartLineComponent.Totals.SubTotal.Amount));
                context.Logger.LogDebug(string.Format("{0} - Adjustment Total:{1}", this.Name, num));

                //** Custom Implementation
                // Retrieve the sellable item from commerce context
                var sellableItem = context.CommerceContext.GetEntity<SellableItem>();
                var composerTemplateViewsComponent = sellableItem.GetComponent<ComposerTemplateViewsComponent>().Views.FirstOrDefault();
                var composerView = sellableItem.GetComposerView(composerTemplateViewsComponent.Key);

                // Extract the needed tax value from custom view property
                string taxValue = composerView.Properties.FirstOrDefault(element => element.Name.Equals(policy1.TaxFieldName)).Value;

                // Cast the string with correct culture to decimal
                if (!decimal.TryParse(taxValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-GB"), out decimal taxValueAsDecimal))
                {
                    taxValueAsDecimal = defaultItemTaxRate;
                }

                // Check if the result decimal is whitelisted
                if (!policy1.Whitelist.Contains(taxValueAsDecimal))
                {
                    context.Logger.LogDebug(string.Format("{0} - Tax Rate: {1} is not whitelisted", this.Name, taxValue));
                    taxValueAsDecimal = defaultItemTaxRate;
                }
                //**

                Money money = new Money(currencyCode, (cartLineComponent.Totals.SubTotal.Amount + num) * defaultItemTaxRate);
                if (policy2.ShouldRoundPriceCalc)
                {
                    money.Amount = Decimal.Round(money.Amount, policy2.RoundDigits, policy2.MidPointRoundUp ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                }

                IList<AwardedAdjustment> adjustments = cartLineComponent.Adjustments;
                CartLineLevelAwardedAdjustment awardedAdjustment = new CartLineLevelAwardedAdjustment
                {
                    Name = "TaxFee",
                    DisplayName = "TaxFee",
                    Adjustment = money,
                    AdjustmentType = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                    AwardingBlock = this.Name,
                    IsTaxable = false,
                    IncludeInGrandTotal = false
                };
                adjustments.Add(awardedAdjustment);
            }
            return Task.FromResult(arg);
        }
    }
}
