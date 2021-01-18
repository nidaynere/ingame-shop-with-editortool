using System;
using System.Linq;
using System.Collections.Generic;

namespace Payments.Products
{
    [Serializable]
    public class Bundle : Purchasable, ICloneable
    {
        [Serializable]
        public class BundleItem
        {
            public string ItemId;
            public int Amount;
        }

        /// <summary>
        /// List of products included in the bundle.
        /// </summary>
        public List<BundleItem> BundleItems = new List<BundleItem>();

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool ContainsItem(string ItemId)
        {
            return BundleItems.Find(x => x.ItemId.Equals(ItemId)) != null;
        }
    }
}
