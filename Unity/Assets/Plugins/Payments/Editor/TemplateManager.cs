using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Payments.Editor
{
    public class TemplateManager : EditorWindow
    {
        #region private variables.
        private static PaymentsConfig paymentsConfig;
        private static int templateIndex;
        private static bool newTemplateCreator = false;
        private static string newTemplateName;
        private static string[] duplicationList;
        private static string lastMessage;
        #endregion

        public static void Init(PaymentsConfig config)
        {
            newTemplateCreator = false;

            paymentsConfig = config;

            if (paymentsConfig.Templates != null && !string.IsNullOrEmpty(paymentsConfig.Template))
                templateIndex = paymentsConfig.Templates.ToList().FindIndex(x => x.Equals(paymentsConfig.Template));
            else templateIndex = -1;

            // Get existing open window
            //or if none, make a new one:
            var window = (TemplateManager)GetWindow(typeof(TemplateManager));
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Templates");

            GUILayout.Label(lastMessage);

            if (newTemplateCreator)
            {
                if (duplicationList.Length > 0)
                {
                    GUILayout.Label("You can select a template from the currents to duplicate.");
                    templateIndex = EditorGUILayout.Popup(templateIndex, duplicationList);
                }

                newTemplateName = GUILayout.TextField(newTemplateName);
                if (GUILayout.Button("Create template"))
                {
                    newTemplateCreator = false;
                    // Send this new template to the service.

                    var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;

                    if (templateIndex >= 1)
                    { // duplication is required.
                        if (templateIndex < duplicationList.Length)
                        {
                            // First download the target template.
                           
                            HTTPEditor.SendGet(address + "/GetApp/" + paymentsConfig.AppId + "/" + duplicationList[templateIndex],
                                (isError, responseBody) =>
                                {
                                    Debug.Log(responseBody);

                                    // Save the new template.
                                    HTTPEditor.SendPost(address + "/SetApp/" + paymentsConfig.AppId + "/" + newTemplateName, responseBody,
                                    (_isSuccess, _responseBody) =>
                                    {
                                        if (!!!_isSuccess)
                                        {
                                            lastMessage = "HTTP Error";
                                        }
                                        else
                                        {
                                            RefreshWithResponse(_responseBody);
                                        }
                                    });
                                });
                        }
                    }
                    else
                    {
                        var body = "{ }";
                        // Create empty template.
                        // Save the new template.
                        HTTPEditor.SendPost(address + "/SetApp/" + paymentsConfig.AppId + "/" + newTemplateName, body,
                        (_isSuccess, _responseBody) =>
                        {
                            Debug.Log(_responseBody);

                            if (!!!_isSuccess)
                            {
                                lastMessage = "HTTP Error";
                            }
                            else
                            {
                                RefreshWithResponse(_responseBody);
                            }
                        });
                    }
                }
                return;
            }

            if (paymentsConfig.IsUpdateAvailable)
            {
                // Show update button.
                if (GUILayout.Button("Save config."))
                {
                    paymentsConfig.IsUpdateAvailable = false;

                    File.WriteAllText(PaymentsManager.ConfigPath, JsonUtility.ToJson(paymentsConfig, true));

                    // Load asset database again.
                    AssetDatabase.Refresh();
                }
            }

            // Template selection enabled.
            if (paymentsConfig.Templates != null && paymentsConfig.Templates.Length > 0)
                templateIndex = EditorGUILayout.Popup(templateIndex, paymentsConfig.Templates);
            else GUILayout.Label("No templates on this app yet.");

            if (templateIndex >= 0)
            {
                // template options.
                if (GUILayout.Button("Edit Template"))
                {
                    TemplateEditor.Init(paymentsConfig.Templates[templateIndex], (isSuccess, Message) => {
                        lastMessage = "Edit Template Result  => " + isSuccess + " Message: " + Message;
                        Debug.Log(lastMessage);
                    }); 
                }

                if (GUILayout.Button("Set as default"))
                {
                    if (EditorUtility.DisplayDialog("Beware!", "Are you sure you want to assign this template as defauşt?", "Yes", "Cancel"))
                    {
                        paymentsConfig.Template = paymentsConfig.Templates[templateIndex];
                        // Update on server.
                        PaymentsManager.Push((isPushed) => {
                            lastMessage = "Pushed => " + isPushed;
                            Debug.Log(lastMessage);
                        });
                    }
                }

                if (GUILayout.Button("DeleteTemplate"))
                {
                    if (EditorUtility.DisplayDialog("Beware!", "You are about to remove this template. Cannot be undo.", "Go ahead", "Cancel"))
                    {
                        var address = Resources.Load<PaymentsAppSettings>("PaymentsAppSettings").ServiceURL;

                        HTTPEditor.SendGet(address + "/RemoveTemplate/" + paymentsConfig.AppId + "/" + paymentsConfig.Templates[templateIndex], 
                            (isSuccess, _responseBody) => {
                                lastMessage = _responseBody;
                                Debug.Log(lastMessage);
                                if (isSuccess)
                                {
                                    RefreshWithResponse(_responseBody);
                                }
                            }
                        );
                    }
                }
            }

            GUILayout.Space(20);
            if (GUILayout.Button("Create new template"))
            {
                newTemplateName = "NewTemplate";
                if (paymentsConfig.Templates != null && paymentsConfig.Templates.ToList().Find(x => x.Equals(newTemplateName)) != null)
                {
                    int i = 1;
                    while (i < 99)
                    {
                        var targetName = newTemplateName + "(" + i + ")";
                        if (paymentsConfig.Templates.ToList().Find(x => x.Equals(targetName)) == null)
                        {
                            newTemplateName = targetName;
                            break;
                        }

                        i++;
                    }
                }

                if (paymentsConfig.Templates == null || paymentsConfig.Templates.Length == 0)
                    duplicationList = new string[0];
                else
                {
                    var tList = paymentsConfig.Templates.ToList();
                    tList.Insert(0, "Select a template.");
                    duplicationList = tList.ToArray();
                }


                newTemplateCreator = true;
            }
        }


        private static void RefreshWithResponse(string responseBody)
        {
            var response = JsonUtility.FromJson<GeneralResponse>(responseBody);
            if (response == null)
            {
                lastMessage = "Invalid response.";
            }
            else
            {
                if (response.isError)
                {
                    lastMessage = response.message;
                }
                else
                {
                    // Renew PaymentsConfig.
                    PaymentsManager.Pull((val) => {
                        lastMessage = "Pulled => " + val;
                        Debug.Log(lastMessage);

                        if (val)
                        {
                            Init(PaymentsManager.CurrentConfig);
                        }
                    });
                }
            }
        }
    }
}
