using UnityEngine;
using System;
public class ShopItem : ICloneable
{
    /// <summary>
    /// Instantiated object of the shop item.
    /// </summary>
    [NonSerialized] public Transform Obj;

    public ShopItem(Transform obj)
    {
        Obj = obj;
    }

    public virtual ShopProduct GetProduct() { return null; }
    public virtual ShopBundle GetBundle() { return null; }

    public virtual bool IsBundle() { return false; }
    public virtual bool IsProduct() { return false; }

    public virtual Payments.Products.Purchasable GetBase()
    {
        return null;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}

public class ShopProduct : ShopItem
{
    public Payments.Products.Product Product;

    public ShopProduct(Transform obj) : base(obj)
    {

    }

    public override ShopProduct GetProduct()
    {
        return this;
    }

    public override bool IsProduct()
    {
        return true;
    }

    public override Payments.Products.Purchasable GetBase()
    {
        return Product;
    }
}

public class ShopBundle : ShopItem
{
    public Payments.Products.Bundle Bundle;

    public ShopBundle(Transform obj) : base(obj)
    {

    }

    public override ShopBundle GetBundle()
    {
        return this;
    }

    public override bool IsBundle()
    {
        return true;
    }

    public override Payments.Products.Purchasable GetBase()
    {
        return Bundle;
    }
}