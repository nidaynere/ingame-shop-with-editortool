using UnityEngine;

public class PaymentsAppSettings : ScriptableObject
{
    [System.Serializable]
    public class SortOrder
    {
        public string Name;
        public string[] SortingOrderItemIds;
    }

    public SortOrder[] SortingOrders;

    public string ServiceURL = "http://localhost:9001";

    /// <summary>
    /// This can be different for different apps.
    /// </summary>
    public string[] GameItemList = new string[]
        {
            "Gems",
            "Tickets",
            "Coins"
        };
}
