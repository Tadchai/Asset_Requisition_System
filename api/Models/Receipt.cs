using System;
using System.Collections.Generic;

namespace api.Models;

public partial class Receipt
{
    public int ReceiptId { get; set; }

    public DateOnly Date { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal Discount { get; set; }

    public decimal TotalValue { get; set; }

    public string ReceiptName { get; set; } = null!;

    public virtual ICollection<ReceiptDetail> ReceiptDetails { get; set; } = new List<ReceiptDetail>();
}
