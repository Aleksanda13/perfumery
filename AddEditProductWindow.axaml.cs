using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using perfumery.Context;
using perfumery.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace perfumery;

public partial class AddEditProductWindow : Window
{
    private int? _productId;
    private string? _savedImageName;
    private string? _selectedImagePath;
    public AddEditProductWindow(int productId)
    {
        InitializeComponent();
        Init(productId);
    }
    public AddEditProductWindow()
    {
        InitializeComponent();
        Init(null);
    }

    private void Init(int? productId)
    {
        _productId = productId;
        Opened += async (_, _) =>
        {
            await LoadData();
            if (_productId.HasValue)
            {
                TitleText.Text = "Редактирование";
                await LoadProductAsync(_productId.Value);
                IdBoxLabel.IsVisible = true;
                IdBox.IsVisible = true;
                IdBox.IsReadOnly = true;
            }
            else
            {
                TitleText.Text = "Новый товар";
                ImageSorce.Source = null;
                MaxDiscountBox.Text = "0";
                DiscountBox.Text = "0";
                CostBox.Text = "0";
                QuantityBox.Text = "0";
            }
        };
    }

    private async Task LoadData()
    {
       await using var db = new PerfumeryContext();

        UnitBox.ItemsSource = await db.Units.ToListAsync();
        ManufacturerBox.ItemsSource = await db.Manufacturers.ToListAsync();
        SupplierBox.ItemsSource = await db.Suppliers.ToListAsync();
        CategoryBox.ItemsSource= await db.Categories.ToListAsync();

        UnitBox.SelectedIndex = 0;
        ManufacturerBox.SelectedIndex = 0;
        SupplierBox.SelectedIndex = 0;
        CategoryBox.SelectedIndex = 0;
    }
    private async Task LoadProductAsync(int productId)
    {
        await using var db = new PerfumeryContext();
        var product = await db.Products.FirstAsync(x => x.ProductId == productId);

        IdBox.Text = product.ProductId.ToString();
        ArticleBox.Text = product.Article;
        NameBox.Text = product.ProductName;
        CostBox.Text = product.Cost.ToString();
        DiscountBox.Text = product.CurrentDiscount.ToString();
        MaxDiscountBox.Text = product.DiscountMax.ToString();
        QuantityBox.Text = product.QuantityInStock.ToString();
        DescriptionBox.Text = product.Description;

        ImageName.Text = product.ImagePath ?? "не выбрано";
        ImageSorce.Source = LoadImage(product.ImagePath);

        UnitBox.SelectedItem = UnitBox.Items.Cast<Unit>()
                                            .FirstOrDefault(u => u.UnitId == product.UnitId);
        ManufacturerBox.SelectedItem = ManufacturerBox.Items.Cast<Manufacturer>()
                                                            .FirstOrDefault(m => m.ManufacturerId == product.ManufacturerId);
        SupplierBox.SelectedItem = SupplierBox.Items.Cast<Supplier>()
                                                    .FirstOrDefault(s => s.SupplierId == product.SupplierId);
        CategoryBox.SelectedItem = CategoryBox.Items.Cast<Category>()
                                                    .FirstOrDefault(c => c.CategoryId == product.CategoryId);
    }

    private Bitmap? LoadImage(string? imagePath)
    {
        try
        {
            if(!string.IsNullOrWhiteSpace(imagePath))
            {
                var path = Path.Combine(
                    AppContext.BaseDirectory,
                    "Images",
                    "Product",
                    imagePath);
                if (File.Exists(path))
                {
                    return new Bitmap(path);
                }
            }

        }catch{ }
        try
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
        catch { }
        return null;
    }

    private async void ChooseImage_Click(object? sender, RoutedEventArgs e)
    {
        var imageFolder = Path.Combine(AppContext.BaseDirectory, "Images", "Product");
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображение",
            AllowMultiple = false,
            SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(imageFolder)
        });
        var file = files.FirstOrDefault();
        if (file == null) { return; }
        _selectedImagePath = file.Path.LocalPath;
        ImageSorce.Source = new Bitmap(_selectedImagePath);
    }


    private async Task<string> SaveImage(string source)
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Images", "Product");
        Directory.CreateDirectory(folder);

        var filename = $"{Guid.NewGuid()}{Path.GetExtension(source)}";
        var destination = Path.Combine(folder, filename);
        await using var input = File.OpenRead(source);
        await using var output = File.Create(destination);
        await input.CopyToAsync(output);
        return filename;
    }

    private async void SaveProduct_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ArticleBox.Text) || string.IsNullOrWhiteSpace(NameBox.Text))
        {
            await ShowError("Введите артикул и название товара");
            return;
        }
        if(UnitBox.SelectedItem == null || SupplierBox.SelectedItem == null || ManufacturerBox.SelectedItem == null || CategoryBox.SelectedItem == null)
        {
            await ShowError("Звполните данные о товаре");
            return;
        }
        if (ArticleBox.Text.Length > 6)
        {
            await ShowError("Артикул не может состоять более чем из 6 символов");
            return;
        }
        if(!decimal.TryParse(CostBox.Text, out var cost) || !int.TryParse(DiscountBox.Text, out var discount) || 
            !int.TryParse(MaxDiscountBox.Text, out var maxdiscount) || !int.TryParse(QuantityBox.Text, out var quantity))
        {
            await ShowError("Введите числовые значения");
            return;
        }
        if(cost < 0 || maxdiscount < 0 || maxdiscount > 100 || discount < 0 || discount > 100 || maxdiscount < discount)
        {
            await ShowError("Введите числовые значения");
            return;
        }
        try
        {
            await using var db = new PerfumeryContext();

            string imageName = _savedImageName;

            if (!string.IsNullOrWhiteSpace(_selectedImagePath))
            {
                imageName = await SaveImage(_selectedImagePath);
            }

            Product p;

            if (_productId.HasValue)
            {
                p = await db.Products.FirstAsync(x => x.ProductId == _productId.Value);
            }
            else
            {
                p = new Product();
                db.Products.Add(p);
            }

            p.Article = ArticleBox.Text.Trim();
            p.ProductName = NameBox.Text.Trim();
            p.CategoryId = ((Category)CategoryBox.SelectedItem).CategoryId;
            p.UnitId = ((Unit)UnitBox.SelectedItem).UnitId;
            p.ManufacturerId = ((Manufacturer)ManufacturerBox.SelectedItem).ManufacturerId;
            p.SupplierId = ((Supplier)SupplierBox.SelectedItem).SupplierId;
            p.Cost = cost;
            p.QuantityInStock = quantity;
            p.CurrentDiscount = discount;
            p.Description = DescriptionBox.Text?.Trim();
            p.ImagePath = imageName;

            await db.SaveChangesAsync();

            Close();
        }
        catch (Exception ex)
        {
            await ShowError("Ошибка сохранения\n" + ex.Message);
        }
    }

    private async Task ShowError(string message)
    {
        await MessageBoxManager
            .GetMessageBoxStandard("Ошибка",message, ButtonEnum.Ok)
            .ShowAsync();
    }
    private async void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (!_productId.HasValue)
            return;

        try
        {
            await using var db = new PerfumeryContext();

            var product = await db.Products
                .FirstAsync(x => x.ProductId == _productId.Value);

            var hasOrders = await db.OrderProducts
                .AnyAsync(x => x.ProductId == product.ProductId);

            if (hasOrders)
            {
                await ShowError("Товар используется в заказах.");
                return;
            }
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            Close();
        }
        catch (Exception ex)
        {
            await ShowError("ошибк " + ex.Message);
        }
    }
    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}