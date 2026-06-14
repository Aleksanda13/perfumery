using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using perfumery.Context;
using System.Linq;
using System.Threading.Tasks;

namespace perfumery;

public partial class OrdersListWindow : Window
{
    public OrdersListWindow()
    {
        InitializeComponent();
        Opened += async (_, _) =>
        {
            await LoadOrdersAsync();
        };
    }
 
    private async Task LoadOrdersAsync()
    {
        await using var db = new PerfumeryContext();

        var orders = await db.Orders
            .AsNoTracking()
            .Include(x => x.Client)
            .Include(x => x.Status)
            .Include(x => x.OrderProducts)
                .ThenInclude(x => x.Product)
            .OrderByDescending(x => x.CreateAt)
            .ToListAsync();

        OrdersList.ItemsSource = orders;
    }
}