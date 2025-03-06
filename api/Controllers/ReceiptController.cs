using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ReceiptController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public ReceiptController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReceiptRequest request)
        {
            if (request.ReceiptDetail == null || !request.ReceiptDetail.Any())
                return new JsonResult(new MessageResponse { Message = "ReceiptDetail is null", StatusCode = HttpStatusCode.BadRequest });

            foreach (var instance in request.ReceiptDetail)
            {
                if (instance.InstanceId.HasValue)
                {
                    var instanceModel = await (from i in _context.ItemInstances
                                               where i.ItemInstanceId == instance.InstanceId
                                               select i)
                                                .SingleAsync();
                    if (instanceModel.RequisitionId.HasValue)
                    {
                        return new JsonResult(new MessageResponse { Message = "instance has borrowed.", StatusCode = HttpStatusCode.BadRequest });
                    }
                    else if ((int)instanceModel.SoldStatus == (int)InstanceStatus.Sold)
                    {
                        return new JsonResult(new MessageResponse { Message = "instance has been sold", StatusCode = HttpStatusCode.BadRequest });
                    }
                }
                if (instance.Quantity <= 0)
                    return new JsonResult(new MessageResponse { Message = "quantity is empty or negative", StatusCode = HttpStatusCode.BadRequest });

                if (string.IsNullOrWhiteSpace(instance.Unit))
                    return new JsonResult(new MessageResponse { Message = "Unit is null,empty or whitespace", StatusCode = HttpStatusCode.BadRequest });

                if (instance.Price <= 0)
                    return new JsonResult(new MessageResponse { Message = "Price is empty or negative", StatusCode = HttpStatusCode.BadRequest });

                if (instance.Quantity * instance.Price != instance.TotalValue)
                    return new JsonResult(new MessageResponse { Message = "TotalValue in row is not correct", StatusCode = HttpStatusCode.BadRequest });
            }
            if (request.TotalAmount < 0)
                return new JsonResult(new MessageResponse { Message = "TotalAmount must not be negative", StatusCode = HttpStatusCode.BadRequest });

            if (request.Discount < 0)
                return new JsonResult(new MessageResponse { Message = "Discount must not be negative", StatusCode = HttpStatusCode.BadRequest });

            if (request.TotalAmount - request.Discount != request.TotalValue)
                return new JsonResult(new MessageResponse { Message = "Final TotalValue is not correct", StatusCode = HttpStatusCode.BadRequest });

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var receiptName = "";
                    var lastReceipName = await (from r in _context.Receipts
                                                orderby r.ReceiptId
                                                select r.ReceiptName)
                                                .LastOrDefaultAsync();
                    if (lastReceipName == null)
                    {
                        receiptName = "R0001";
                    }
                    else
                    {
                        var numberPart = int.Parse(lastReceipName.Substring(1)) + 1;
                        receiptName = "R" + numberPart.ToString().PadLeft(4, '0');
                    }
                    var receiptModel = new Receipt
                    {
                        ReceiptName = receiptName,
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        TotalAmount = request.TotalAmount,
                        Discount = request.Discount,
                        TotalValue = request.TotalValue,

                    };
                    await _context.Receipts.AddAsync(receiptModel);
                    await _context.SaveChangesAsync();

                    foreach (var instance in request.ReceiptDetail)
                    {
                        if (instance.InstanceId.HasValue)
                        {
                            var instanceModel = await (from i in _context.ItemInstances
                                                       where i.ItemInstanceId == instance.InstanceId
                                                       select i)
                                                .SingleAsync();
                            instanceModel.SoldStatus = (int)InstanceStatus.Sold;
                        }

                        var receiptDetailModel = new ReceiptDetail
                        {
                            ReceiptId = receiptModel.ReceiptId,
                            InstanceId = instance.InstanceId,
                            NewInstance = instance.NewInstance,
                            Quantity = instance.Quantity,
                            Unit = instance.Unit,
                            Price = instance.Price,
                            TotalValue = instance.TotalValue,
                        };
                        await _context.ReceiptDetails.AddAsync(receiptDetailModel);
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Receipt Create successfully.", StatusCode = HttpStatusCode.Created });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int receiptId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var receiptModel = await _context.Receipts.SingleAsync(r => r.ReceiptId == receiptId);
                    var responseReceipt = new GetReceiptResponse
                    {
                        ReceiptName = receiptModel.ReceiptName,
                        Date = receiptModel.Date,
                        TotalAmount = receiptModel.TotalAmount,
                        Discount = receiptModel.Discount,
                        TotalValue = receiptModel.TotalValue,
                        ReceiptDetail = new List<GetReceiptDetail>()
                    };

                    var query = from r in _context.ReceiptDetails
                                join i in _context.ItemInstances on r.InstanceId equals i.ItemInstanceId into rJoin
                                from i in rJoin.DefaultIfEmpty()
                                where r.ReceiptId == receiptId
                                select new GetReceiptDetail
                                {
                                    IsInstance = r.InstanceId.HasValue,
                                    InstanceName = r.NewInstance ?? i.AssetId,
                                    Quantity = r.Quantity,
                                    Unit = r.Unit,
                                    Price = r.Price,
                                    TotalValue = r.TotalValue,
                                };
                    var receiptDetails = await query.ToListAsync();

                    responseReceipt.ReceiptDetail = receiptDetails;

                    return new JsonResult(responseReceipt);
                }
                catch (Exception ex)
                {
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchReciptRequest input)
        {
            if (input.PageSize <= 0)
            {
                return new JsonResult(new MessageResponse { Message = "PageSize must be greater than or equal to 0.", StatusCode = HttpStatusCode.BadRequest });
            }

            int skip = input.Page * input.PageSize;
            IQueryable<Receipt> Items;

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                Items = _context.Receipts;
            }
            else
            {
                Items = _context.Receipts.Where(e => e.ReceiptName.ToLower().Contains(input.Name.ToLower()));
            }

            int totalItems = await Items.CountAsync();
            var paginatedItems = await Items.Skip(skip).Take(input.PageSize).ToListAsync();

            var data = paginatedItems.Select(r => new PaginationReciptResponse
            {
                ReceiptName = r.ReceiptName,
                ReceiptId = r.ReceiptId,
            }).ToList();

            var response = new SearchItemResponse<PaginationReciptResponse>
            {
                Data = data,
                PageIndex = input.Page,
                PageSize = input.PageSize,
                RowCount = totalItems,
            };

            return new JsonResult(response);
        }
    }
}