using Sitecore.Commerce.Plugin.Tax;
using System;
using System.Collections.Generic;

namespace Plugin.Sample.GenericTaxes.Policies
{
    /// <summary>
    /// Generic Taxes Policy
    /// </summary>
    public class GenericTaxPolicy : GlobalTaxPolicy
    {
        /// <summary>
        /// c'tor
        /// </summary>
        public GenericTaxPolicy() : base()
        {
            this.TaxFieldName = "Taxes";
            this.Whitelist = new List<Decimal>();
            this.UseDefaultTaxRateIfNoneIsSet = true;
        } 

        /// <summary>
        /// Whitelist of allowed tax rates
        /// </summary>
        public IList<Decimal> Whitelist { get; set; }

        /// <summary>
        /// TaxFieldName on the sellable Item
        /// </summary>
        public string TaxFieldName { get; set; }

        /// <summary>
        /// Flag to determine if the default Tax rate should be applied if no one is present
        /// </summary>
        public bool UseDefaultTaxRateIfNoneIsSet { get; set; }
    }
}
