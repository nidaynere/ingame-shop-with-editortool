using System;
using System.Collections.Generic;

namespace Payments.Products
{
    [Serializable]
    public class Product : Purchasable, ICloneable
    {
        public string ItemId;
        public int Amount;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public override bool ContainsItem(string itemId)
        {
            return ItemId.Equals(itemId);
        }
    }
}
