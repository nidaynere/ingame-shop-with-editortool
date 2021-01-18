using UnityEngine;
using System;

namespace Payments
{
    public class PaymentsClient 
    {
        public static void Load (Action<PaymentTemplate, PaymentsAppSettings> OnPaymentsLoaded, Action OnPaymentsFailed)
        { 
            // Load Payments App Settings
            var appSettings = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings");
            if (appSettings == null)
            {
                Debug.LogError("[Payments] Payment app settings is not found. Use PaymentsManager to set the appid. And pull please.");
                OnPaymentsFailed.Invoke();
                return;
            }

            // Load Payments Config.
            var configAsset = Resources.Load<TextAsset>("PaymentsConfig");
            var config = JsonUtility.FromJson<PaymentsConfig>(configAsset.text);

            if (config == null)
            {
                Debug.LogError("[Payments] PaymentsConfig is not found. Please use PaymentsManager to pull the config.");
                OnPaymentsFailed.Invoke();
                return;
            }

            // Load the latest pull from resources folder.
            var defaultPaymentsTemplate = Resources.Load<TextAsset>("defaultPaymentsTemplate");

            if (defaultPaymentsTemplate == null)
            {
                Debug.LogError("[PAYMENTS] Default template is not found on resources folder. Use payments tool to create app & template.");
                OnPaymentsFailed.Invoke();
            }
            else
            {
                // Load Template.
                var template = JsonUtility.FromJson<PaymentTemplate>(defaultPaymentsTemplate.text);
                OnPaymentsLoaded?.Invoke(template, appSettings);
                Debug.Log("[PAYMENTS] Template loaded.");
            }
        }
    }

}
