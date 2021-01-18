using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

namespace Payments.Editor
{
    public class PaymentsManager : EditorWindow
    {
        #region private variables, will be used in payments manager.
        private static bool dataPulled = false;
        private static bool appIdDoesntExist = false;
        private static bool templatesEnabled = false;
        private static string lastMessage = "";
        #endregion

        #region public variables
        public static PaymentsConfig CurrentConfig;
        public static string ConfigPath;
        #endregion

        // Add menu named "My Window" to the Window menu
        [MenuItem("Payments/Manager")]
        static void Init()
        {
            lastMessage = "";
            templatesEnabled = false;
            appIdDoesntExist = false;
            dataPulled = false;

            // payments app settings
            var paymentsAppSettings = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings");
            if (paymentsAppSettings == null)
            {
                // Create app settings.
                PaymentsAppSettings asset = CreateInstance<PaymentsAppSettings>();

                AssetDatabase.CreateAsset(asset, "Assets/Resources/PaymentsAppSettings.asset");
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();

                Selection.activeObject = asset;
            }
            //

            // resources folder.
            string targetFolder = Application.dataPath + "/Resources";
            ConfigPath = targetFolder + "/"+ "PaymentsConfig.json";
            if (!!!File.Exists(ConfigPath))
            {   // File doesnt Exist. Create.
                if (!!!Directory.Exists (targetFolder))
                {
                    // Directory is also Null??
                    Directory.CreateDirectory(targetFolder);
                }

                string NewConfig = "{ \"_AppId\": \"unassignedId\" }";
                File.WriteAllText(ConfigPath, NewConfig);

                // Load asset database again.
                AssetDatabase.Refresh();
            }

            // Read config.
            string readed = File.ReadAllText(ConfigPath);
            if (string.IsNullOrEmpty (readed)) // READ DATA IS NULL!!!
            {
                Debug.LogError("[Payments] 'Error code 001': Not sure what happened. But config data is not found & not created for some reason.");
                return;
            }

            CurrentConfig = JsonUtility.FromJson<PaymentsConfig>(readed);
            if (CurrentConfig == null)
            {
                Debug.LogError("[Payments] 'Error code 002': Broken config.");
                return;
            }

            // Get existing open window
            //or if none, make a new one:
            PaymentsManager window = (PaymentsManager)GetWindow(typeof(PaymentsManager));
            window.Show();
        }

        void OnGUI()
        {
            if (CurrentConfig == null)
            {
                Init(); // Code refresh probably. Redo.
                return;
            }

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            CurrentConfig._AppId = EditorGUILayout.TextField("AppId", CurrentConfig._AppId);

            if (!dataPulled)
            {
                // pull the latest.
                if (GUILayout.Button("Pull the latest config"))
                {
                    Pull();
                }
            }
            else
            {
                // pull the latest.
                if (GUILayout.Button("Push latest config."))
                {
                    // HTTPRequester Download.
                }
            }

            GUILayout.Label(lastMessage);

            if (appIdDoesntExist)
            {
                if (GUILayout.Button ("Create new app with this ID"))
                {
                    var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;

                    var appId = CurrentConfig.AppId;
                    CurrentConfig = new PaymentsConfig();
                    CurrentConfig.AppId = appId;

                    HTTPEditor.SendPost(address + "/SetApp/" + CurrentConfig.AppId, JsonUtility.ToJson(CurrentConfig),
                        (isSuccess, responseBody) => {
                            if (!isSuccess)
                            {
                                lastMessage = "HTTP Error.";
                                return;
                            }

                            var response = JsonUtility.FromJson<GeneralResponse>(responseBody);
                            if (response == null || response.error > 0)
                            {
                                appIdDoesntExist = true;

                                lastMessage = response != null ? response.message : "HTTP Error";
                            }
                            else
                            {
                                appIdDoesntExist = false;
                                lastMessage = response.message;
                            }
                        });
                }
            }

            if (templatesEnabled)
            {
                if (GUILayout.Button("Open templates window"))
                {
                    TemplateManager.Init(CurrentConfig);
                }
            }
        }

        public static void Pull(Action<bool> OnPulled = null)
        {
            lastMessage = "Pulling appId => " + CurrentConfig.AppId;

            // HTTPRequester Download.
            var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;
            HTTPEditor.SendGet(address + "/GetApp/" + CurrentConfig.AppId, (isSuccess, responseBody) => {
                if (!isSuccess)
                {
                    Debug.Log("[Payments] HTTP Error.");
                    return;
                }

                Debug.Log(responseBody);
                var response = JsonUtility.FromJson<GeneralResponse>(responseBody);

                if (response == null)
                {
                    lastMessage = "[Payments] Data is broken.";
                    OnPulled?.Invoke(false);
                }
                else
                {
                    lastMessage = "Pulled appId => " + CurrentConfig.AppId;

                    switch (response.error)
                    {
                        case 0: // App is exists with templates.
                            templatesEnabled = true;
                            appIdDoesntExist = false;

                            CurrentConfig = JsonUtility.FromJson<PaymentsConfig>(responseBody);
                            
                            // [KNOWN ISSUE]
                            // Service returns the templates list with the extension. Because template files could be a different file type.
                            // Server never checks the templates if they are proper json or not. They could be images also.
                            // But in this JSON case, im putting an easy fix here by removing the extension.
                            if (CurrentConfig.Templates != null)
                            {
                                int length = CurrentConfig.Templates.Length;
                                for (int i = 0; i < length; i++)
                                {
                                    int textLength = CurrentConfig.Templates[i].Length;
                                    CurrentConfig.Templates[i] = Path.GetFileNameWithoutExtension(CurrentConfig.Templates[i]);
                                }
                            }

                            File.WriteAllText(ConfigPath, JsonUtility.ToJson(CurrentConfig)); // Update payments config on resources.
                            AssetDatabase.Refresh();
                            
                            OnPulled?.Invoke(true);

                            if (!string.IsNullOrEmpty(CurrentConfig.Template) && CurrentConfig.Templates.ToList().Find(x => x.Equals(CurrentConfig.Template)) != null)
                            {
                                // Pull default template.
                                /// Download target template.
                                HTTPEditor.SendGet(address + "/GetApp/" + CurrentConfig.AppId + "/" + CurrentConfig.Template, (_isSuccess, _responseBody) => {
                                    if (!_isSuccess)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        // Default template Pulled.
                                        Debug.Log("[Payments] Default template pulled.");
                                        File.WriteAllText(Application.dataPath + "/Resources/defaultPaymentsTemplate.json", _responseBody);
                                        AssetDatabase.Refresh();
                                    }
                                });
                            }

                            break;

                        default:
                            appIdDoesntExist = true; templatesEnabled = false;
                            lastMessage = "AppId doesnt exist on the server.";
                            OnPulled?.Invoke(false);
                            break;
                    }
                }
            });
        }

        public static void Push (Action<bool> OnPushed = null)
        {
            var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;
            HTTPEditor.SendPost(address + "/SetApp/" + CurrentConfig.AppId, JsonUtility.ToJson(CurrentConfig),
            (isSuccess, responseBody) => {

                if (!isSuccess)
                {
                    lastMessage = "HTTP Error.";
                    OnPushed?.Invoke(false);
                    return;
                }

                var response = JsonUtility.FromJson<GeneralResponse>(responseBody);
                if (response == null || response.error > 0)
                {
                    appIdDoesntExist = true;
                    lastMessage = response != null ? response.message : "HTTP Error";
                    OnPushed?.Invoke(false);
                }
                else
                {
                    appIdDoesntExist = false;
                    lastMessage = response.message;
                    OnPushed?.Invoke(true);
                }
            });
        }
    }
}
