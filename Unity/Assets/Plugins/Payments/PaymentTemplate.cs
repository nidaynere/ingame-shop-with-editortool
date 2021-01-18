using Payments.Products;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Payments
{
    /// <summary>
    /// This is the data of the payments.
    /// Apps can have multiple payment templates.
    /// This will be retrieved from the server.
    /// Products & Bundles are purchasable.
    /// Products contains ItemDefinitions,
    /// Bundles contains Products.
    /// </summary>
    [System.Serializable]
    public class PaymentTemplate
    {
        #region collections
        /// <summary>
        /// Definitions of app items. Users can see this, but this items cannot be purchased directly.
        /// A product contains items.
        /// Only products & bundles can be purchased.
        /// </summary>
        public List<Item> ItemDefinitions = new List<Item>();

        public List<Product> Products = new List<Product>();
        public List<Bundle> Bundles = new List<Bundle>();
        #endregion
    }
}
