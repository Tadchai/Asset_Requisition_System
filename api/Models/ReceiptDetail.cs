using System;
using System.Collections.Generic;

namespace api.Models;

public partial class ReceiptDetail
{
    public int ReceiptDetailId { get; set; }

    public int ReceiptId { get; set; }

    public int? InstanceId { get; set; }

    public string? NewInstance { get; set; }

    public int Quantity { get; set; }

    public string Unit { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal TotalValue { get; set; }

    public virtual ItemInstance? Instance { get; set; }

    public virtual Receipt Receipt { get; set; } = null!;
}
