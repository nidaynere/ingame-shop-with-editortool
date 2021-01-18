using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Payments.Editor
{
    public class TemplateEditor : EditorWindow
    {
        private static string CurrentTemplateId;
        public static PaymentTemplate CurrentTemplate;

        public static void Init(string TemplateId, Action<bool, string> OnEditorResult)
        {
            CurrentTemplateId = TemplateId;

            var config = PaymentsManager.CurrentConfig;
            var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;

            /// Download target template.
            HTTPEditor.SendGet(address + "/GetApp/" + config.AppId + "/" + TemplateId, (isSuccess, responseBody) => {
                if (!isSuccess)
                {
                    OnEditorResult?.Invoke(false, "HTTP Error");
                    return;
                }
                else
                {
                    CurrentTemplate = JsonUtility.FromJson<PaymentTemplate>(responseBody);

                    if (CurrentTemplate == null)
                    {
                        OnEditorResult?.Invoke(true, "Template data is null.");
                        return;
                    }

                    OnEditorResult?.Invoke(true, "Template received.");

                    // Create window.
                    var window = (TemplateEditor)GetWindow(typeof(TemplateEditor));
                    window.Show();
                }
            });
        }

        Vector2 scrollPos;

        private void OnGUI()
        {
            GUILayout.Label("TemplateEditor");

            if (GUILayout.Button("Create Item"))
            {
                ItemEditor.Init(null, (product) => {
                    // New Item created.
                    CurrentTemplate.ItemDefinitions.Add(product);
                    UpdateTemplate();
                });
            }

            if (CurrentTemplate.ItemDefinitions != null && CurrentTemplate.ItemDefinitions.Count > 0)
            {
                if (GUILayout.Button("Create Product"))
                {
                    ProductEditor.Init(null, (product) => {
                        // New Product
                        // TODO => CHECK FOR ID CONFLICT.
                        CurrentTemplate.Products.Add(product);
                        UpdateTemplate();
                    });
                }
            }

            if (CurrentTemplate.Products != null && CurrentTemplate.Products.Count > 0)
            {
                if (GUILayout.Button("Create Bundle"))
                {
                    BundleEditor.Init(null, (bundle) => {
                        // New Bundle
                        // TODO => CHECK FOR ID CONFLICT.
                        CurrentTemplate.Bundles.Add(bundle);
                        UpdateTemplate();
                    });
                }
            }

            // List Items & Products & Bundles
            DrawItems();
            DrawProducts();
            DrawBundles();
            // 
        }
        private void UpdateTemplate()
        {
            Debug.Log("Updating template " + CurrentTemplateId);

            var config = PaymentsManager.CurrentConfig;
            var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;

            var templateData = JsonUtility.ToJson(CurrentTemplate, true);

            HTTPEditor.SendPost(address + "/SetApp/" + config.AppId + "/" + CurrentTemplateId, templateData, (isSuccess, responseData) => {
                if (!isSuccess)
                {
                    Debug.LogError("Failed to update template.");
                }
                else
                {
                    Debug.Log(responseData);
                }
            });
        }

        private Vector2 itemsScrollPos;
        private void DrawItems()
        {
            if (CurrentTemplate.ItemDefinitions == null || CurrentTemplate.ItemDefinitions.Count == 0)
                return;

            GUILayout.Label("Items", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            itemsScrollPos = EditorGUILayout.BeginScrollView(itemsScrollPos, GUILayout.Width(300), GUILayout.Height(100));

            // DrawItems

            /// Storage variable if one of them is gonna be removed.
            Products.Item m_itemToRemove = null;

            foreach (var @base in CurrentTemplate.ItemDefinitions)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(@base.Id);
                if (GUILayout.Button("Edit"))
                {
                    ItemEditor.Init(@base, (product) => {
                        // New Item created.
                        int target = CurrentTemplate.ItemDefinitions.FindIndex(x => x.Id.Equals(@base.Id));
                        if (target != -1)
                        {
                            CurrentTemplate.ItemDefinitions[target] = product;
                            UpdateTemplate();
                        }
                    });
                }

                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Beware!", "Do you want to delete this item? All products using this item will be removed also!!", "Go ahead", "Cancel"))
                    {
                        m_itemToRemove = @base;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (m_itemToRemove != null)
            {
                // Find products, using this id.
                if (CurrentTemplate.Products != null)
                    CurrentTemplate.Products.RemoveAll(x => x.ItemId == m_itemToRemove.Id);

                // Remove the item.
                CurrentTemplate.ItemDefinitions.Remove(m_itemToRemove);

                // Update the template.
                UpdateTemplate();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private Vector2 productsScrollPos;
        private void DrawProducts()
        {
            if (CurrentTemplate.Products == null || CurrentTemplate.Products.Count == 0)
                return;

            GUILayout.Label("Products", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            productsScrollPos = EditorGUILayout.BeginScrollView(productsScrollPos, GUILayout.Width(300), GUILayout.Height(100));

            // DrawProducts

            /// Storage variable if one of them is gonna be removed.
            Products.Product m_itemToRemove = null;

            foreach (var @base in CurrentTemplate.Products)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(@base.Id);
                if (GUILayout.Button("Edit"))
                {
                    ProductEditor.Init(@base, (product) => {
                        // New Item created.
                        int target = CurrentTemplate.Products.FindIndex(x => x.Id.Equals(product.Id));
                        if (target != -1)
                        {
                            CurrentTemplate.Products[target] = product;
                            UpdateTemplate();
                        }
                    });
                }

                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Beware!", "Do you want to delete this item? All products using this item will be removed also!!", "Go ahead", "Cancel"))
                    {
                        m_itemToRemove = @base;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (m_itemToRemove != null)
            {
                // Remove the base.
                CurrentTemplate.Products.Remove(m_itemToRemove);

                // Update the template.
                UpdateTemplate();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private Vector2 bundleScrollPos;
        private void DrawBundles()
        {
            if (CurrentTemplate.Bundles == null || CurrentTemplate.Bundles.Count == 0)
                return;

            GUILayout.Label("Bundles", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical();
            bundleScrollPos = EditorGUILayout.BeginScrollView(bundleScrollPos, GUILayout.Width(300), GUILayout.Height(100));

            // DrawBundles

            /// Storage variable if one of them is gonna be removed.
            Products.Bundle m_itemToRemove = null;

            foreach (var @base in CurrentTemplate.Bundles)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(@base.Id);
                if (GUILayout.Button("Edit"))
                {
                    BundleEditor.Init(@base, (bundle) => {
                        // New Item created.
                        int target = CurrentTemplate.Bundles.FindIndex(x => x.Id.Equals(bundle.Id));
                        if (target != -1)
                        {
                            CurrentTemplate.Bundles[target] = bundle;
                            UpdateTemplate();
                        }
                    });
                }

                if (GUILayout.Button("Delete"))
                {
                    if (EditorUtility.DisplayDialog("Beware!", "Do you want to delete this item? All products using this item will be removed also!!", "Go ahead", "Cancel"))
                    {
                        m_itemToRemove = @base;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (m_itemToRemove != null)
            {
                // Remove the base.
                CurrentTemplate.Bundles.Remove(m_itemToRemove);

                // Update the template.
                UpdateTemplate();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// Easy functions to create UI elements.
    /// </summary>
    public class EditorGUIHelper
    {
        public static string DrawTextField(string header, ref string variable)
        {
            GUILayout.Label(header);
            return EditorGUILayout.TextField(variable);
        }
    }

    public class ItemEditor : EditorWindow
    {
        private static Products.Item CurrentBase;
        private static Action<Products.Item> CompletedAction;
        private static int selection = -1;
        private static PaymentsAppSettings appSettings;
        public static void Init(Products.Item targetBase = null, Action<Products.Item> OnComplete = null)
        {
            CompletedAction = OnComplete;

            if (targetBase != null)
                CurrentBase = (Products.Item)targetBase.Clone();
            else
                CurrentBase = new Products.Item();

            appSettings = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings");

            selection = appSettings.GameItemList.ToList().FindIndex(x => x.Equals(CurrentBase.Id));

            // Create window.
            var window = (ItemEditor)GetWindow(typeof(ItemEditor));
            window.minSize = new Vector2(320, 235);
            window.maxSize = new Vector2(320, 235);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Item Editor");

            GUILayout.Label("Id");

            selection = EditorGUILayout.Popup(selection, appSettings.GameItemList);
            if (selection != -1)
                CurrentBase.Id = appSettings.GameItemList[selection];

            CurrentBase.Name = EditorGUIHelper.DrawTextField("Name", ref CurrentBase.Name);
            CurrentBase.Desc = EditorGUIHelper.DrawTextField("Desc", ref CurrentBase.Desc);
            CurrentBase.IconURL = EditorGUIHelper.DrawTextField("Icon URL", ref CurrentBase.IconURL);

            if (GUILayout.Button("Save"))
            {
                CompletedAction?.Invoke(CurrentBase);

                var window = (ItemEditor)GetWindow(typeof(ItemEditor));
                window.Close();
            }
        }
    }

    public class PricesEditor
    {
        private Vector2 scrollPos;

        public void Draw(Products.Purchasable purchasable)
        {
            GUILayout.Space(20);

            GUILayout.Label("Prices");
            if (GUILayout.Button("Add New Price"))
            {
                // Remove price.
                var list = purchasable.Prices.ToList();
                list.Add(new Products.Price());
                purchasable.Prices = list.ToArray();
            }
            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(100));

            if (purchasable.Prices == null || purchasable.Prices.Length == 0)
            {
                purchasable.Prices = new Products.Price[1];
                purchasable.Prices[0] = new Products.Price();
            }

            int priceToRemove = -1;

            int pricesLength = purchasable.Prices.Length;
            for (int i = 0; i < pricesLength; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Width(200);
                purchasable.Prices[i].CurrencyType = (Products.Price.Currency)EditorGUILayout.EnumPopup("Currency", purchasable.Prices[i].CurrencyType);

                string value = EditorGUILayout.TextField(purchasable.Prices[i].Value.ToString());
                float result;
                if (float.TryParse(value, out result))
                {
                    purchasable.Prices[i].Value = result;
                }

                if (i > 0)
                {
                    if (GUILayout.Button("-"))
                    {
                        // Remove price.
                        priceToRemove = i;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (priceToRemove != -1)
            {
                var list = purchasable.Prices.ToList();
                list.RemoveAt(priceToRemove);
                purchasable.Prices = list.ToArray();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
            // draw value.
        }
    }

    public class ProductEditor : EditorWindow
    {
        private static Products.Product CurrentBase;
        private static Action<Products.Product> CompletedAction;
        private static int selection = -1;
        private static string[] itemList;
        private static PricesEditor pricesEditor;
        public static void Init(Products.Product targetBase = null, Action<Products.Product> OnComplete = null)
        {
            CompletedAction = OnComplete;

            if (targetBase != null)
                CurrentBase = (Products.Product)targetBase.Clone();
            else
                CurrentBase = new Products.Product();

            int itemCount = TemplateEditor.CurrentTemplate.ItemDefinitions.Count;
            itemList = new string[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                if (TemplateEditor.CurrentTemplate.ItemDefinitions[i].Id == CurrentBase.ItemId)
                    selection = i;

                itemList[i] = TemplateEditor.CurrentTemplate.ItemDefinitions[i].Id;
            }

            pricesEditor = new PricesEditor();

            // Create window.
            var window = (ProductEditor)GetWindow(typeof(ProductEditor));
            window.minSize = new Vector2(320, 435);
            window.maxSize = new Vector2(320, 435);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Product Editor");

            CurrentBase.Id = EditorGUIHelper.DrawTextField("Id", ref CurrentBase.Id);

            // ITEM ID.
            GUILayout.Label("Select item from the list.");
            selection = EditorGUILayout.Popup(selection, itemList);
            if (selection != -1)
                CurrentBase.ItemId = itemList[selection];

            var amountAsString = CurrentBase.Amount.ToString();
            // draw amount.
            amountAsString = EditorGUIHelper.DrawTextField("Amount", ref amountAsString);
            int result;
            if (int.TryParse(amountAsString, out result))
            {
                CurrentBase.Amount = result;
            }

            CurrentBase.Name = EditorGUIHelper.DrawTextField("Name", ref CurrentBase.Name);
            CurrentBase.Desc = EditorGUIHelper.DrawTextField("Desc", ref CurrentBase.Desc);
            CurrentBase.IconURL = EditorGUIHelper.DrawTextField("Icon URL", ref CurrentBase.IconURL);

            pricesEditor.Draw(CurrentBase);

            if (GUILayout.Button("Save"))
            {
                CompletedAction?.Invoke(CurrentBase);

                var window = (ProductEditor)GetWindow(typeof(ProductEditor));
                window.Close();
            }
        }
    }

    public class BundleEditor : EditorWindow
    {
        private static Products.Bundle CurrentBase;
        private static Action<Products.Bundle> CompletedAction;
        private static int[] selection;
        private static string[] itemList;
        private static Vector2 scrollPos;
        private static PricesEditor pricesEditor;

        public static void Init(Products.Bundle targetBase = null, Action<Products.Bundle> OnComplete = null)
        {
            CompletedAction = OnComplete;

            if (targetBase != null)
                CurrentBase = (Products.Bundle)targetBase.Clone();
            else
                CurrentBase = new Products.Bundle();

            int templateProductsCount = CurrentBase.BundleItems.Count;
            int totalItemsCount = TemplateEditor.CurrentTemplate.ItemDefinitions.Count;

            selection = new int[templateProductsCount];

            itemList = new string[totalItemsCount];
            for (int i = 0; i < totalItemsCount; i++)
            {
                itemList[i] = TemplateEditor.CurrentTemplate.ItemDefinitions[i].Id;
            }

            // set current selections
            for (int i = 0; i < templateProductsCount; i++)
            {
                selection[i] = itemList.ToList().FindIndex(x => x.Equals(CurrentBase.BundleItems[i].ItemId));
            }

            pricesEditor = new PricesEditor();
            // Create window.
            var window = (BundleEditor)GetWindow(typeof(BundleEditor));
            window.minSize = new Vector2(320, 510);
            window.maxSize = new Vector2(320, 510);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Bundle Editor");

            CurrentBase.Id = EditorGUIHelper.DrawTextField("Id", ref CurrentBase.Id);
            CurrentBase.Name = EditorGUIHelper.DrawTextField("Name", ref CurrentBase.Name);
            CurrentBase.Desc = EditorGUIHelper.DrawTextField("Desc", ref CurrentBase.Desc);
            CurrentBase.IconURL = EditorGUIHelper.DrawTextField("Icon URL", ref CurrentBase.IconURL);

            GUILayout.Label("Products");
            if (itemList.Length > 0)
                if (GUILayout.Button("Add Product"))
                {
                    CurrentBase.BundleItems.Add(new Products.Bundle.BundleItem() { ItemId = itemList[0], Amount = 1 });
                    selection = selection.Append(0).ToArray();
                }
            /// Storage variable if one of them is gonna be removed.
            Products.Bundle.BundleItem m_itemToRemove = null;

            EditorGUILayout.BeginVertical();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(100));

            // Draw products included in this bundle.
            int length = CurrentBase.BundleItems.Count;
            for (int i = 0; i < length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                selection[i] = EditorGUILayout.Popup(selection[i], itemList);
                if (selection[i] != -1)
                    CurrentBase.BundleItems[i].ItemId = itemList[selection[i]];

                var amountAsString = CurrentBase.BundleItems[i].Amount.ToString();
                // draw amount.
                amountAsString = EditorGUIHelper.DrawTextField("Amount", ref amountAsString);
                int result;
                if (int.TryParse(amountAsString, out result))
                {
                    CurrentBase.BundleItems[i].Amount = result;
                }

                if (GUILayout.Button("Remove"))
                {
                    m_itemToRemove = CurrentBase.BundleItems[i];
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (m_itemToRemove != null)
            {
                int indexToRemove = CurrentBase.BundleItems.FindIndex(x => x.ItemId == m_itemToRemove.ItemId);
                if (indexToRemove != -1)
                {
                    CurrentBase.BundleItems.RemoveAt(indexToRemove);
                }
            }

            pricesEditor.Draw(CurrentBase);

            GUILayout.Space(20);
            if (GUILayout.Button("Save"))
            {
                CompletedAction?.Invoke(CurrentBase);

                var window = (BundleEditor)GetWindow(typeof(BundleEditor));
                window.Close();
            }
        }
    }
}

