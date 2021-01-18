using System;

namespace Payments.Products
{
    [Serializable]
    /// <summary>
    /// Item is the main item that can be rewarded to user.
    /// Products can be purchased from bundles or/and from directly.
    /// </summary>
    public class Item : Base, ICloneable
    {
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
