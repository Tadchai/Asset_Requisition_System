using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace api.ViewModels
{
    public class CreateReceiptRequest
    {
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalValue { get; set; }
        public List<CreateReceiptDetail> ReceiptDetail { get; set; } = new List<CreateReceiptDetail>();
    }

    public class CreateReceiptDetail
    {
        public int? InstanceId { get; set; }
        public string? NewInstance { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class GetReceiptResponse
    {
        public string ReceiptName { get; set;}
        public DateOnly Date { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalValue { get; set; }
        public List<GetReceiptDetail> ReceiptDetail { get; set; } = new List<GetReceiptDetail>();
    }

    public class GetReceiptDetail
    {
        public bool IsInstance { get; set; }
        public string InstanceName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Price { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class SearchReciptRequest
    {
        public string? Name { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class PaginationReciptResponse
    {
        public int ReceiptId { get; set; }
        public string ReceiptName { get; set; }

    }

}