using System.Collections;
using UnityEngine;
using System;
using Unity.EditorCoroutines.Editor;
using Payments;

namespace Payments.Editor
{
    public class HTTPEditor
    {
        public static void SendPost(string URL, string Data, Action<bool, string> Response)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(HTTP._Post(URL, Data, Response));
        }

        public static void SendGet (string URL, Action<bool, string> Response)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(HTTP._Get(URL, Response));
        }
    }
}
