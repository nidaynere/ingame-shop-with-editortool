#pragma warning disable CS0649

using UnityEngine;
using Payments;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System;
/// <summary>
/// Its the main shop manager.
/// It will pull the PaymentsTemplate from payments service at start.
/// </summary>
public class ShopManager : MonoBehaviour
{
    /// <summary>
    /// Item purchased, you can register your system here.
    /// </summary>
    public static Action<ShopItem> OnPurchaseItem;

    private static ShopManager Instance;

    #region serialized variables
    [SerializeField] private UnityEvent onShopLoaded;
    /// <summary>
    /// Main shop panel.
    /// </summary>
    [SerializeField] private GameObject mainPanel;

    [SerializeField] private Transform itemPrefab;
    [SerializeField] private Transform holder;
    [SerializeField] private UIMultipleSelection filterSelection;
    [SerializeField] private Dropdown sortingDropdown;

    [SerializeField] private Transform showcase;
    [SerializeField] private Transform showcaseItem;

    [SerializeField] private Button purchaseButton;
    #endregion

    #region private variables
    /// <summary>
    /// Collection of all items. will be filled by payment template.
    /// </summary>
    private List<ShopItem> Items = new List<ShopItem>();

    private PaymentTemplate currentTemplate;
    private int currentShopFilter;
    private string[] currentItemFilter;
    private ShopItem activeItem;
    #endregion

    #region Loaders
    private void Start()
    {
        Instance = this;

        DontDestroyOnLoad(this);
        DontDestroyOnLoad(gameObject);

        // Load Payments.
        PaymentsClient.Load(TemplateLoaded, 
            () => 
        { Debug.LogError("[SHOP] Template is not retrieved."); });
    }

    private void TemplateLoaded(PaymentTemplate template, PaymentsAppSettings appSettings)
    {
        Debug.Log("[SHOP] Template loaded.");

        currentTemplate = template;

        // Initialize UI.
        foreach (var product in template.Products)
        {
            AddProduct(product);
        }

        foreach (var bundle in template.Bundles)
        {
            AddBundle(bundle);
        }

        // Load filters by items.
        Transform filterHolder = filterSelection.transform;
        var filterPrefab = filterHolder.GetChild(0);

        foreach (var item in template.ItemDefinitions)
        {
            var filter = Instantiate(filterPrefab, filterHolder);
            filter.gameObject.SetActive(true);

            filter.GetComponentInChildren<Text>().text = item.Name;
            filter.gameObject.name = item.Id;
        }

        filterSelection.Initialize(); // first child is the template. use the first generated one.
        // filter event
        filterSelection.OnValueChanged.AddListener((List<int> values) => {
            int count = values.Count;
            string[] itemIds = new string[count];
            for (int i = 0; i < count; i++)
            {
                // get item id.
                itemIds[i] = filterHolder.GetChild(values[i]).name;
            }

            FilterShopByItemId(itemIds);
        });

        filterSelection.SelectAll();

        #region Create sorting orders.
        /// 
        var options = new List<Dropdown.OptionData>();

        //  DEFAULT SORTING ORDERS.
        string[] defaultSortings = new string[] {
            "by name (ascending)",
            "by name (descending)",
            "by price (high to low)",
            "by price (low to high)"
        };

        foreach (var sorting in defaultSortings)
            options.Add(new Dropdown.OptionData() { text = sorting });

        /// Item sortings from the app settings.
        int sortingOrdersCount = appSettings.SortingOrders.Length;
        for (int i = 0; i < sortingOrdersCount; i++)
        {
            options.Add(new Dropdown.OptionData() { text = appSettings.SortingOrders[i].Name });
        }

        sortingDropdown.AddOptions(options);

        sortingDropdown.onValueChanged.AddListener((index) => {
            if (index >= defaultSortings.Length)
            {
                string[] order = appSettings.SortingOrders[index - defaultSortings.Length].SortingOrderItemIds;
                SortByItem(order);
            }
            else
            {
                switch (index)
                {
                    case 0: case 1:
                        SortByName(index == 0);
                        break;

                    case 2: case 3:
                        SortByPrice(Payments.Products.Price.Currency.USDollars, index == 2);
                        break;
                }
            }
        });
        #endregion

        // Update sorting.
        sortingDropdown.onValueChanged?.Invoke(0);

        onShopLoaded?.Invoke();

        purchaseButton.onClick.AddListener(() => {
            OnPurchaseItem?.Invoke(activeItem);
        });
    }

    public void TemplateFailed()
    {
        Debug.Log("[SHOP] Template could not retrieved.");
    }
    #endregion

    #region sorting functions
    private void SortByItem(string[] ItemIds)
    {
        int length = ItemIds.Length;
        for (int i = length - 1; i >= 0; i--)
        {
            Items = Items.
                OrderByDescending(x => x.GetBase().ContainsItem(ItemIds[i])).ToList();
        }

        TransformReOrder();
    }

    private void SortByName(bool IsDescending = true)
    {
        Items.Sort(delegate (ShopItem a, ShopItem b)
        {
            var nameA = a.GetBase().Name;
            var nameB = b.GetBase().Name;
            if (string.IsNullOrEmpty (nameA) || string.IsNullOrEmpty(nameB)) 
                return 0;

            return IsDescending ? nameA.CompareTo(nameB) : nameB.CompareTo (nameA);
        });

        TransformReOrder();
    }

    private void SortByPrice(Payments.Products.Price.Currency Currency, bool IsDescending = true)
    {
        if (IsDescending)
            Items = Items.OrderByDescending(x => x.GetBase().GetPrice(Currency)).ToList();
        else Items = Items.OrderBy(x => x.GetBase().GetPrice(Currency)).ToList();

        TransformReOrder();
    }

    private void TransformReOrder()
    {
        // apply to transforms sibling index.
        int itemLength = Items.Count;
        for (int i = itemLength - 1; i >= 0; i--)
        {
            Items[i].Obj.SetSiblingIndex(i);
        }
    }
    #endregion

    #region filters

    /// <summary>
    /// Is the shop item in the shop categories?
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool InFilter(ShopItem item)
    {
        return currentShopFilter == 0 ||
            (currentShopFilter == 1 && item.IsProduct()) ||
            (currentShopFilter == 2 && item.IsBundle());
    }

    /// <summary>
    /// Is the shop item in the filter by item Id?
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool InItemFilter(ShopItem item)
    {
        if (currentItemFilter == null)
            return true;

        foreach (var itemId in currentItemFilter)
        {
            if (item.GetBase().ContainsItem(itemId))
            {
                return true;
            }
        }

        return false;
    }
    public void FilterShop(int value)
    {
        currentShopFilter = value;
        // Value = 0 => All
        // Value = 1 => ProductsOnly
        // Value = 2 => BundlesOnly

        foreach (var item in Items)
        {
            bool isActive = InItemFilter(item);
            if (isActive)
            {
                isActive = InFilter(item);
            }

            item.Obj.gameObject.SetActive(isActive);
        }

    }

    private void FilterShopByItemId(string[] itemId)
    {
        currentItemFilter = itemId;
        foreach (var item in Items)
        {
            bool isActive = InFilter(item);
            if (isActive)
            {
                isActive = InItemFilter (item);
            }

            item.Obj.gameObject.SetActive (isActive);
        }

    }
    #endregion

    #region UI initializers
    private void AddProduct(Payments.Products.Product product)
    {
        Transform obj = Instantiate(itemPrefab, holder);
        ShopProduct newProduct = new ShopProduct(obj);
        newProduct.Product = product;

        Items.Add(newProduct);

        SetItem(newProduct, true);
    }

    private void AddBundle(Payments.Products.Bundle bundle)
    {
        Transform obj = Instantiate(itemPrefab, holder);
        ShopBundle newBundle = new ShopBundle(obj);
        newBundle.Bundle = bundle;

        Items.Add(newBundle);

        SetItem(newBundle, true);
    }

    private void SetItem(ShopItem @shopItem, bool catalogueItem)
    {
        // Set Item Properties.
        shopItem.Obj.Find("Name").GetComponentInChildren<Text>().text = shopItem.GetBase().Name;
        shopItem.Obj.Find("PriceTag").GetComponentInChildren<Text>().text = shopItem.GetBase().GetPrice(Payments.Products.Price.Currency.USDollars) + "$";

        if (catalogueItem)
        {
            shopItem.Obj.GetComponentInChildren<Button>().onClick.AddListener(() => {
                // clone the item and show it on showcase.
                var newShopItem = (ShopItem)shopItem.Clone();
                newShopItem.Obj = showcaseItem;
                SetItem(newShopItem, false); // false means showcase item.

                showcase.gameObject.SetActive(true);

                activeItem = shopItem;
            });
        }

        var tooltip = shopItem.Obj.Find("Tooltip");
        tooltip.GetComponentInChildren<Text>().text = shopItem.GetBase().Desc;

        var itemHolder = tooltip.Find("Items");

        void CreateItem(Transform holder, string URL, string Value, string Name)
        {
            Transform prefab = holder.GetChild(0);
            var newItem = Instantiate(prefab, holder);

            newItem.gameObject.SetActive(true);

            newItem.Find("Name").GetComponent<Text>().text = Name;
            newItem.Find("Value").GetComponent<Text>().text = Value;

            // Load Item Icon
            HTTP.SendGetImage(URL, (isSuccess, T2D) => {
                if (!isSuccess)
                {
                    return;
                }
                else
                {
                    Sprite newSprite = Sprite.Create(T2D, new Rect(0, 0, T2D.width, T2D.height), Vector2.one / 2);
                    newItem.Find("Icon").GetComponent<Image>().sprite = newSprite;
                }
            });
        }

        // clear old tooltips.
        int childCount = itemHolder.childCount;
        var childs = new Transform[childCount-1];
        for (int i = 1; i < childCount; i++)
            childs[i-1] = itemHolder.GetChild(i);

        foreach (var child in childs)
            Destroy(child.gameObject);
        //

        // Tooltip draw items & amounts.
        if (shopItem.IsProduct())
        {
            var itemId = shopItem.GetProduct().Product.ItemId;
            // find this item in ItemDefinitions.
            var targetItem = currentTemplate.ItemDefinitions.Find(x => x.Id == itemId);
            CreateItem(itemHolder, targetItem.IconURL, "x" + shopItem.GetProduct().Product.Amount, targetItem.Name);
            // load item icon & name.
        }
        else if (shopItem.IsBundle ())
        {
            var bundle = shopItem.GetBundle().Bundle;
            // bundle.
            foreach (var bundleItem in bundle.BundleItems)
            {
                // find this item in ItemDefinitions.
                var targetItem = currentTemplate.ItemDefinitions.Find(x => x.Id == bundleItem.ItemId);
                CreateItem(itemHolder, targetItem.IconURL, "x" + bundleItem.Amount, targetItem.Name);
            }
        }

        // Load Icon.
        HTTP.SendGetImage(shopItem.GetBase().IconURL, (isSuccess, T2D) => {
            if (!isSuccess)
            {
                return;
            }
            else
            {
                Sprite newSprite = Sprite.Create(T2D, new Rect(0, 0, T2D.width, T2D.height), Vector2.one /2);
                shopItem.Obj.Find("Icon").GetComponent<Image>().sprite = newSprite;
            }
        });
    }
    #endregion

    #region public static methods
    /// <summary>
    /// Is the shop successfully loaded?
    /// </summary>
    public static bool IsLoaded => Instance != null && Instance.currentTemplate != null;

    public static void Open()
    {
        if (Instance == null)
        {
            Debug.LogError("[SHOP] Prefab is not in the scene.");
            return;
        }

        if (IsLoaded)

            Instance.mainPanel.SetActive(true);

        else Debug.LogError("[SHOP] is not initialized!");
    }

    public static void Close()
    {
        if (Instance == null)
        {
            Debug.LogError("[SHOP] Prefab is not in the scene.");
            return;
        }

        if (IsLoaded)

            Instance.mainPanel.SetActive(false);

        else Debug.LogError("[SHOP] is not initialized!");
    }
    #endregion
}
