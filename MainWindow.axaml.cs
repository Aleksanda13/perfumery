using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using perfumery.Context;
using perfumery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace perfumery
{
    public partial class MainWindow : Window
    {
        private UserSession _session;
        public string FullName => _session.FullName;
        public string RoleName => _session.RoleName;
        private Dictionary<int, int> _orderItems = new();
        public MainWindow()
        {
            InitializeComponent();
            _session = UserSession.Guest();
            InitWindow();
        }

        public MainWindow(UserSession session)
        {
            InitializeComponent();
            _session = session;
            InitWindow();
        }

        private async void InitWindow()
        {
            DataContext = this;
            SetupRoleAcsess();
            SetupDiscountFilter();
            Opened += async (_, _) =>
            {
                await SetupManufacturerFilter();
                await LoadProductAsync();
            };

        }
        private void SetupRoleAcsess()
        {

        }
        private async Task SetupManufacturerFilter()
        {
            if (ManufacturerBox is null)
            {
                return;
            }

            await using var db = new PerfumeryContext();
            var manufacturers = await db.Manufacturers
                                .AsNoTracking()
                                .OrderBy(m => m.ManufacturerName)
                                .Select(m => m.ManufacturerName)
                                .ToListAsync();
            manufacturers.Insert(0, "Все производители");
            ManufacturerBox.ItemsSource = manufacturers;
            ManufacturerBox.SelectedIndex = 0;
        }

        private void SetupDiscountFilter()
        {
            DiscountBox.ItemsSource = new List<string>
       {
           "Все диапазоны",
           "0-10%",
           "10-20%",
           "21% и более"
       };
            DiscountBox.SelectedIndex = 0;
        }

        private async Task LoadProductAsync()
        {
            var search = (SearchBox.Text ?? "").Trim().ToLower();
            bool sortAscending = SortBox.SelectedIndex == 0;
            var selectedDiscount = DiscountBox.SelectedItem.ToString() ?? "Все диапазоны";
            var selectedManufacturer = ManufacturerBox.SelectedItem?.ToString() ?? "Все производители";

            try
            {
                await using var db = new PerfumeryContext();
                var query = db.Products
                            .AsNoTracking()
                            .Include(m => m.Category)
                             .Include(m => m.Manufacturer)
                             .AsQueryable();
                if (search.Length > 0)
                {
                    query = query.Where(product =>
                        product.ProductName.ToLower().Contains(search) ||
                        product.Description.ToLower().Contains(search));
                }
                if (selectedDiscount == "0-10%")
                {
                    query = query.Where(product =>
                    product.CurrentDiscount <= 10);
                }
                if (selectedDiscount == "10-20%")
                {
                    query = query.Where(product =>
                    product.CurrentDiscount > 10 && product.CurrentDiscount <= 20);
                }
                if (selectedDiscount == "21% и более")
                {
                    query = query.Where(product =>
                    product.CurrentDiscount >= 21);
                }
                if (selectedManufacturer != "Все производители")
                {
                    query = query.Where(product =>
                    product.Manufacturer.ManufacturerName == selectedManufacturer);
                }

                query = sortAscending
                    ? query.OrderBy(product => product.Cost * (1 - product.CurrentDiscount / 100m))
                    : query.OrderByDescending(product => product.Cost * (1 - product.CurrentDiscount / 100m));

                var totalCount = await db.Products.CountAsync();
                var list = await query.ToListAsync();
                ProductList.ItemsSource = list;
                CountBox.Text = $"{list.Count()} из {totalCount}";

            }
            catch (Exception ex)
            {
                await MessageBoxManager
              .GetMessageBoxStandard("Ошибка", "Ошибка загрузки" + ex, ButtonEnum.Ok)
              .ShowAsync();
                return;
            }
        }
        private async void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var selection = ProductList.SelectedItem as Product;
            if (selection == null) { return; }
            var window = new AddEditProductWindow(selection.ProductId);
            await window.ShowDialog(this);
            ProductList.SelectedItem = null;
            await LoadProductAsync();
        }
        private async void AddProduct_Click(object? sender, RoutedEventArgs e)
        {
            var window = new AddEditProductWindow();
            await window.ShowDialog(this);
            await LoadProductAsync();
        }

        private void AddToOrder_Click(object? sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var product = button?.DataContext as Product;

            if (product == null)
                return;

            if (_orderItems.ContainsKey(product.ProductId))
                _orderItems[product.ProductId]++;
            else
                _orderItems.Add(product.ProductId, 1);

            OrderButton.IsVisible = _orderItems.Count > 0;
        }

        private async void ViewOrder_Click(object? sender, RoutedEventArgs e)
        {
            var window = new OrderWindow(_session, _orderItems);
            await window.ShowDialog(this);

            OrderButton.IsVisible = _orderItems.Count > 0;

            await LoadProductAsync();
        }

        private async void Orders_Click(object? sender, RoutedEventArgs e)
        {
            var window = new OrdersListWindow();
            await window.ShowDialog(this);
        }
        private async void TextBox_TextChanged(object? sender, TextChangedEventArgs e)
        {
            await LoadProductAsync();
        }
        private async void ComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            await LoadProductAsync();
        }
    }
}