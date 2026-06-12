using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;

namespace perfumery.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Article { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int? UnitId { get; set; }

    public decimal Cost { get; set; }

    public int DiscountMax { get; set; }

    public int ManufacturerId { get; set; }

    public int SupplierId { get; set; }

    public int CategoryId { get; set; }

    public int CurrentDiscount { get; set; }

    public int QuantityInStock { get; set; }

    public string? Description { get; set; }

    public string? ImagePath { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual Unit? Unit { get; set; }
    public bool HasDiscount => CurrentDiscount > 0;
    public string CostText => $"{Cost:0.00} руб.";
    public decimal FinalCost => Cost * (1 - CurrentDiscount / 100m);
    public string FinalCostText => $"{FinalCost:0.00} руб.";
    public string DiscountText => $"{CurrentDiscount}%";
    public IBrush BackgroundCard => CurrentDiscount > 21
        ? new SolidColorBrush(Color.Parse("#7FFF00"))
        : Brushes.White;

    private Bitmap? LoadImage
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(ImagePath))
            {
                var path = Path.Combine(
                    AppContext.BaseDirectory,
                    "Images",
                    "Product",
                    ImagePath);

                if (File.Exists(path))
                {
                    return new Bitmap(path);
                }
            }
            else
            {
                var fallback = Path.Combine(
                    AppContext.BaseDirectory,
                    "Images",
                    "Product",
                    "picture.png");

                if (File.Exists(fallback))
                {
                    return new Bitmap(fallback);
                }
            }
            return null;
        }
    }
}
