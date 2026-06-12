using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using perfumery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using perfumery.Context;
using Microsoft.EntityFrameworkCore;

namespace perfumery;

public partial class OrderWindow : Window
{
    private readonly UserSession _session;
    private readonly Dictionary<int, int> _orderItems;
    public OrderWindow(UserSession session, Dictionary<int, int> orderItems)
    {
        InitializeComponent();
        _session = session;
        _orderItems = orderItems;

        Opened += async (_, _) =>
        {
            await LoadOrderAsync();
        };
    }
    private sealed class OrderProductView
    {
        public int ProductId { get; init; }
        public string ProductName { get; init; } = "";
        public decimal Cost { get; init; }
        public int CurrentDiscount { get; init; }
        public int Count { get; init; }

        public decimal FinalCost => Cost * (1 - CurrentDiscount / 100m);
        public decimal Total => FinalCost * Count;

        public string FinalCostText => $"{FinalCost:0.00} руб.";
        public string TotalText => $"{Total:0.00} руб.";
    }

    private async Task LoadOrderAsync()
    {
        await using var db = new PerfumeryContext();

        var ids = _orderItems.Keys.ToList();

        var products = await db.Products
            .AsNoTracking()
            .Where(x => ids.Contains(x.ProductId))
            .ToListAsync();

        var list = products.Select(product => new OrderProductView
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Cost = product.Cost,
            CurrentDiscount = product.CurrentDiscount,
            Count = _orderItems[product.ProductId]
        }).ToList();

        OrderList.ItemsSource = list;

        var total = list.Sum(x => x.Total);
        TotalText.Text = $"Итого: {total:0.00} руб.";
    }

    private async void Plus_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button?.DataContext as OrderProductView;

        if (item == null)
            return;

        _orderItems[item.ProductId]++;

        await LoadOrderAsync();
    }

    private async void Minus_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button?.DataContext as OrderProductView;

        if (item == null)
            return;

        _orderItems[item.ProductId]--;

        if (_orderItems[item.ProductId] <= 0)
            _orderItems.Remove(item.ProductId);

        if (_orderItems.Count == 0)
        {
            Close();
            return;
        }

        await LoadOrderAsync();
    }

    private async void Delete_Click(object? sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var item = button?.DataContext as OrderProductView;

        if (item == null)
            return;

        _orderItems.Remove(item.ProductId);

        if (_orderItems.Count == 0)
        {
            Close();
            return;
        }

        await LoadOrderAsync();
    }

    private async void CreateOrder_Click(object? sender, RoutedEventArgs e)
    {
        if (_orderItems.Count == 0)
        {
            await ShowError("Корзина пуста.");
            return;
        }

        try
        {
            await using var db = new PerfumeryContext();

            var ids = _orderItems.Keys.ToList();

            var products = await db.Products.Where(x => ids.Contains(x.ProductId))
                .ToListAsync();

            foreach (var product in products)
            {
                var count = _orderItems[product.ProductId];

                if (product.QuantityInStock < count)
                {
                    await ShowError($"Недостаточно товара: {product.ProductName}");
                    return;
                }
            }

            var total = products.Sum(product =>
                product.Cost *
                (1 - product.CurrentDiscount / 100m) *
                _orderItems[product.ProductId]);

            var status = await db.Statuses
                .FirstAsync(x => x.StatusName == "В ожидании");

            var order = new Order
            {
                ClientId = _session.UserId,
                CreateAt = DateTime.Now,
                StatusId = status.StatusId,
                TotalPrice = total
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            foreach (var product in products)
            {
                var count = _orderItems[product.ProductId];

                db.OrderProducts.Add(new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Count = count
                });

                product.QuantityInStock -= count;
            }

            await db.SaveChangesAsync();

            _orderItems.Clear();

            await MessageBoxManager
                .GetMessageBoxStandard("Готово", "Заказ оформлен.", ButtonEnum.Ok)
                .ShowAsync();

            Close();
        }
        catch (Exception ex)
        {
            await ShowError("Не удалось оформить заказ.\n" + ex.Message);
        }
    }

    private async Task ShowError(string text)
    {
        await MessageBoxManager
            .GetMessageBoxStandard("Ошибка", text, ButtonEnum.Ok)
            .ShowAsync();
    }

    private void Close_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}