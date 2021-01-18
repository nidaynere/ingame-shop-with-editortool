using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo : MonoBehaviour
{
    public void DemoOpenShop()
    {
        ShopManager.Open();
    }

    public void DemoCloseShop()
    {
        ShopManager.Close();
    }
}
