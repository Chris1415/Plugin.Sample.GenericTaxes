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
    /// <summary>
    /// CalculateCartGenericTaxBlock
    /// </summary>
    [PipelineDisplayName("GenericTaxes.Block.CalculateCartTaxBlock")]
    public class CalculateCartGenericTaxBlock : PipelineBlock<Cart, Cart, CommercePipelineExecutionContext>
    {
        /// <summary>
        /// En Culture
        /// </summary>
        private readonly CultureInfo CultureEn = CultureInfo.CreateSpecificCulture("en-GB");

        /// <summary>
        /// Run
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns></returns>
        public override Task<Cart> Run(Cart arg, CommercePipelineExecutionContext context)
        {
            if (!ConditionsValid(arg, context))
            {
                Task.FromResult(arg);
            }

            GenericTaxPolicy taxPolicy = context.GetPolicy<GenericTaxPolicy>();

            context.Logger.LogDebug(string.Format("{0} - Policy: {1}", this.Name, taxPolicy.TaxCalculationEnabled));

            IEnumerable<CartLineComponent> source = arg.Lines.Where(line =>
            {
                if (taxPolicy.TaxExemptTagsEnabled && line.HasComponent<CartProductComponent>())
                    return line.GetComponent<CartProductComponent>().Tags
                    .Select((t => t.Name)).Contains(taxPolicy.TaxExemptTag, StringComparer.InvariantCultureIgnoreCase);
                return false;
            });

            Decimal adjustmentLinesTotal = new Decimal();
            Action<CartLineComponent> action = (l => adjustmentLinesTotal += l.Totals.SubTotal.Amount);
            source.ForEach(action);

            string currencyCode = context.CommerceContext.CurrentCurrency();

            Decimal cartLevelAdjustments = arg
               .Adjustments
               .Where(p => p.IsTaxable)
               .Aggregate(Decimal.Zero, ((current, adjustment) => current + adjustment.Adjustment.Amount));

            // Cart Adjustments
            if (cartLevelAdjustments > Decimal.Zero)
            {
                Decimal defaultCartTaxRate = taxPolicy.DefaultCartTaxRate;
                decimal cartLevelTaxRate = cartLevelAdjustments * defaultCartTaxRate;
                string taxName = $"TaxFee-CartAdjustments-{defaultCartTaxRate * 100}";
                arg.Adjustments.Add(new CartLevelAwardedAdjustment
                {
                    Name = taxName,
                    DisplayName = taxName,
                    Adjustment = new Money(currencyCode, cartLevelTaxRate),
                    AdjustmentType = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                    AwardingBlock = this.Name,
                    IsTaxable = false
                });
            }

            IDictionary<decimal, decimal> taxesDictionary = FillTaxesDictionary(taxPolicy, context, arg);

            // Cart Lines
            foreach (decimal key in taxesDictionary.Keys)
            {
                decimal tax = taxesDictionary[key];
                if (tax > Decimal.Zero)
                {
                    string taxName = $"TaxFee-CartLines-{(key * 100)}%";
                    arg.Adjustments.Add(new CartLevelAwardedAdjustment
                    {
                        Name = taxName,
                        DisplayName = taxName,
                        Adjustment = new Money(currencyCode, tax),
                        AdjustmentType = context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax,
                        AwardingBlock = this.Name,
                        IsTaxable = false
                    });
                }
            }
            
            return Task.FromResult(arg);
        }

        /// <summary>
        /// Fills the Taxes Dictionary with the proper Tax Values from cart
        /// </summary>
        /// <param name="taxPolicy">current taxPolicy</param>
        /// <param name="context">context</param>
        /// <param name="arg">arg</param>
        /// <returns></returns>
        private IDictionary<decimal, decimal> FillTaxesDictionary(
            GenericTaxPolicy taxPolicy, 
            CommercePipelineExecutionContext context, 
            Cart arg)
        {
            Decimal defaultItemTaxRate = taxPolicy.DefaultItemTaxRate;
            IDictionary<decimal, decimal> taxesDictionary = InitializeTaxesDictionary(taxPolicy.Whitelist);

            var sellableItems = context.CommerceContext.GetEntities<SellableItem>();
            foreach (var sellableItem in sellableItems)
            {
                var composerTemplateViewsComponent = sellableItem.GetComponent<ComposerTemplateViewsComponent>().Views
                    .FirstOrDefault();
                var composerView = sellableItem.GetComposerView(composerTemplateViewsComponent.Key);

                // Extract the needed tax value from custom view property
                string taxValue = composerView?.Properties
                    .FirstOrDefault(element => element.Name.Equals(taxPolicy.TaxFieldName))
                    ?.Value;

                // Cast the string with correct culture to decimal
                if (!decimal.TryParse(taxValue, NumberStyles.Any, this.CultureEn, out decimal taxValueAsDecimal)
                    || !taxPolicy.Whitelist.Contains(taxValueAsDecimal))
                {
                    context.Logger.LogDebug(string.Format("{0} - Tax Rate: {1} is invalid or not whitelisted", this.Name, taxValue));
                    if (taxPolicy.UseDefaultTaxRateIfNoneIsSet)
                    {
                        taxValueAsDecimal = defaultItemTaxRate;
                    }
                    else
                    {
                        continue;
                    }   
                }

                var cartLine = arg.Lines
                    .FirstOrDefault(element => element.GetComponent<CartProductComponent>().Id.Equals(sellableItem.ProductId));
                Decimal adjustments = cartLine.Adjustments
                    .Where((a => a.IsTaxable))
                    .Aggregate(Decimal.Zero, ((current, adjustment) => current + adjustment.Adjustment.Amount));

                taxesDictionary.TryGetValue(taxValueAsDecimal, out decimal storedTaxRate);
                storedTaxRate += taxValueAsDecimal * (cartLine.Totals.SubTotal.Amount + adjustments);
                taxesDictionary[taxValueAsDecimal] = storedTaxRate;
            }

            return taxesDictionary;
        }

        /// <summary>
        /// Helper to initialize Taxes Dictionary
        /// </summary>
        /// <param name="whiteList">whitelist</param>
        /// <returns>inititalized taxes dictioanry</returns>
        private IDictionary<decimal, decimal> InitializeTaxesDictionary(IEnumerable<decimal> whiteList)
        {
            IDictionary<decimal, decimal> taxesDictionary = new Dictionary<decimal, decimal>();
            foreach (decimal whitelistentry in whiteList)
            {
                taxesDictionary.Add(whitelistentry, 0.0M);
            }

            return taxesDictionary;
        }

        /// <summary>
        /// Check if the conditions are valid to execute the block
        /// </summary>
        /// <param name="arg">arg</param>
        /// <param name="context">context</param>
        /// <returns></returns>
        private bool ConditionsValid(Cart arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(string.Format("{0}: The cart can not be null", this.Name));

            if (!arg.HasComponent<FulfillmentComponent>())
            {
                return false;
            }

            if (!arg.Lines.Any())
            {
                arg.Adjustments
                    .Where(a =>
                    {
                        if (!string.IsNullOrEmpty(a.Name) && a.Name.Equals("TaxFee", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(a.AdjustmentType))
                        {
                            return a.AdjustmentType.Equals(context.GetPolicy<KnownCartAdjustmentTypesPolicy>().Tax, StringComparison.OrdinalIgnoreCase);
                        }
                        return false;
                    })
                   .ToList().ForEach((a => arg.Adjustments.Remove(a)));

                return false;
            }

            if (arg.GetComponent<FulfillmentComponent>() is SplitFulfillmentComponent)
            {
                return false;
            }

            return true;
        }
    }
}
