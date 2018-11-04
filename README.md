# Plugin.Sample.GenericTaxes
Sitecore XC Engine Generic Taxes Plugin Implementation

With this Plugin you are able not only to apply one single tax rate, but as many tax rates as you want.

For detailed information just read https://hachweb.wordpress.com/2018/08/17/sitecore-xc-9-0-2-walkthrough-of-extending-the-standard-tax-plugin-to-fulfill-multiple-tax-rates/

- Note:
This code is an Extension of existing standart sitecore tax plugin functionality and is not fully tested.
The purpose of that repository is to be used for learning and inspiration.

# XP Side
Just add a new Field with Composer, which holds the tax rates for the specific sellable items.
In this example the field is called "Taxes" and holds german tax rates 0.07 and 0.19 as values for 7% and 19% tax rates.

# XC Side
Integrate the Demo Policy below into your Environment file and bootstrap() the environment
Sitecore XC will then automatically extract the values within the newly created "Taxes" field uses them for tax calculation and creates for every tax rate a specific adjustment within the cart. In this example it creates an adjustment for 7 and one for 19% taxes
If a product does not containt any tax information it falls back to the "DefaultItemTaxRate" if "UseDefaultTaxRateIfNoneIsSet" is set to true, otherweise it does not include that sellable item into taxcalculation
For cart level adjustments which are taxable the tax rate from "DefaultCartTaxRate" is used

# Sample JSON Configuration for integration within an Environment file

 {
   "$type": "Plugin.Sample.GenericTaxes.Policies.GenericTaxPolicy, Plugin.Sample.GenericTaxes",
	"DefaultCartTaxRate": 0.07,
	"DefaultItemTaxRate": 0.07,
	"TaxCalculationEnabled": true,
	"PriceIncudesTax": false,
	"CalculateTaxBasedOn": "ShippingAddress",
	"ShippingTaxClass": "CartItems",
	"RoundAtSubTotal": false,
	"TaxFieldName": "Taxes",
	"UseDefaultTaxRateIfNoneIsSet": true,
	"Whitelist": {
	  "$type": "System.Collections.Generic.List`1[[System.Decimal, mscorlib]], mscorlib",
	  "$values": [
		0.07,
		0.19
	  ]
	}
  },
