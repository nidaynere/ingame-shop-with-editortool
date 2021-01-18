using System.Linq;

namespace Payments.Products
{
    [System.Serializable]
    public class Price
    {
        public enum Currency
        {
            USDollars,
            Euros,
            Diamond, // In game item.
            TotalCurrenciesCount
        }

        public Currency CurrencyType;
        /// <summary>
        /// This is the price. But as string to be parsed to multiple things.
        /// </summary>
        public float Value;
    }

    /// <summary>
    /// Base Item available for user purchase.
    /// It could be single item, bundle, anything.
    /// </summary>
    public class Base
    {
        public string Id;

        /// <summary>
        /// Name of the base
        /// </summary>
        public string Name;

        /// <summary>
        /// Icon URL, will be shown in RealTime & PaymentsEditor.
        /// </summary>
        public string IconURL;

        /// <summary>
        /// Main description
        /// </summary>
        public string Desc;
    }

    public class Purchasable : Base
    {
        public Price[] Prices;

        /// <summary>
        /// Returns the price of the item in specific currency.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public float GetPrice(Price.Currency currency)
        {
            var price = Prices.ToList().Find(x => x.CurrencyType == currency);
            if (price == null)
            {
                return 0;
            }

            return price.Value;
        }

        /// <summary>
        /// If this purchasable contains the target item Id;
        /// </summary>
        /// <param name="ItemId"></param>
        /// <returns></returns>
        public virtual bool ContainsItem(string ItemId) { return false; }
    }
}

