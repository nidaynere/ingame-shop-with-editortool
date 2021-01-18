using System;

namespace Payments
{
    [Serializable]
    public class PaymentsConfig
    {
        public bool IsUpdateAvailable = false;

        public int Error;

        public string AppId;
        public string Template;
        public string[] Templates;

        /// <summary>
        /// Id of the application on the payments cloud.
        /// </summary>
        public string _AppId
        {
            get
            {
                return AppId;
            }

            set
            {
                AppId = value;
            }
        }

        /// <summary>
        /// Current payments template.
        /// </summary>
        public string _Template
        {
            get
            {
                return Template;
            }

            set
            {
                if (Template != value)
                {
                    IsUpdateAvailable = true;
                }

                Template = value;
            }
        }
    }
}

