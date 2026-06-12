using System;
using System.Collections.Generic;

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
}
