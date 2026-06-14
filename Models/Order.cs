using System;
using System.Collections.Generic;
using System.Linq;

namespace perfumery.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public DateTime CreateAt { get; set; }

    public DateTime? CloseAt { get; set; }

    public int ClientId { get; set; }

    public decimal TotalPrice { get; set; }

    public int StatusId { get; set; }

    public virtual User Client { get; set; } = null!;

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Status Status { get; set; } = null!;

    public string ClientName => $"{Client.LastName} {Client.FirstName} {Client.Patronymic}";
    public string StatusName => Status.StatusName;
    public string CreateAtText => CreateAt.ToString("dd.MM.yyyy");
    public string CloseAtText => CloseAt.HasValue
        ? CloseAt.Value.ToString("dd.MM.yyyy")
        : "-";
    public string ProductsText => string.Join(", ",
        OrderProducts.Select(x =>
            $"{x.Product.Article} - {x.Product.ProductName} ({x.Count} шт.)"));
}
