using System;
using System.Collections.Generic;

namespace perfumery.Models;

public partial class OrderProduct
{
    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Count { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    internal static object?[] Select(Func<object, string> value)
    {
        throw new NotImplementedException();
    }
}
