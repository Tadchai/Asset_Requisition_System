using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;
using api.Models;
using api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ItemController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public ItemController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetCategory()
        {
            try
            {
                var query = from c in _context.ItemCategories
                            select new GetCategoryResponse
                            {
                                CategoryName = c.Name,
                                CategoryId = c.ItemCategoryId
                            };

                var result = await query.ToListAsync();
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetClassificationByCategoryId([FromQuery] int categoryId)
        {
            try
            {
                var query = from cs in _context.ItemClassifications
                            where cs.ItemCategoryId == categoryId
                            select new GetClassificationByCategoryResponse
                            {
                                ClassificationId = cs.ItemClassificationId,
                                ClassificationName = cs.Name,
                            };

                var result = await query.ToListAsync();
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetInstanceByClassificationId([FromQuery] int classificationId)
        {
            try
            {
                var query = from i in _context.ItemInstances
                            where i.ItemClassificationId == classificationId &&
                            i.RequisitionId == null &&
                            i.SoldStatus == (int)InstanceStatus.NotSold
                            select new GetInstanceByClassificationIdResponse
                            {
                                instanceId = i.ItemInstanceId,
                                AssetId = i.AssetId,
                            };

                var result = await query.ToListAsync();
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var itemCategory = await _context.ItemCategories.SingleAsync(ic => ic.ItemCategoryId == id);
                var response = new GetByIdItemResponse
                {
                    Id = itemCategory.ItemCategoryId,
                    Name = itemCategory.Name,
                    ItemClassifications = new List<ClassificationResponse>()
                };

                var classifications = await _context.ItemClassifications.Where(c => c.ItemCategoryId == id).ToListAsync();
                foreach (var classification in classifications)
                {
                    var classificationResponse = new ClassificationResponse
                    {
                        Id = classification.ItemClassificationId,
                        Name = classification.Name,
                        ItemInstances = new List<InstanceResponse>()
                    };

                    var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                    foreach (var instance in instances)
                    {
                        classificationResponse.ItemInstances.Add(new InstanceResponse
                        {
                            Id = instance.ItemInstanceId,
                            AssetId = instance.AssetId,
                        });
                    }
                    response.ItemClassifications.Add(classificationResponse);
                }
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetByIdWithName([FromQuery] int id)
        {
            try
            {
                var itemCategory = await _context.ItemCategories.SingleAsync(ic => ic.ItemCategoryId == id);
                var response = new GetByIdItemWithNameResponse
                {
                    Id = itemCategory.ItemCategoryId,
                    Name = itemCategory.Name,
                    ItemClassifications = new List<ClassificationWithNameResponse>()
                };

                var classifications = await _context.ItemClassifications.Where(c => c.ItemCategoryId == id).ToListAsync();
                foreach (var classification in classifications)
                {
                    var classificationResponse = new ClassificationWithNameResponse
                    {
                        Id = classification.ItemClassificationId,
                        Name = classification.Name,
                        ItemInstances = new List<InstanceWithNameResponse>()
                    };

                    var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                    foreach (var instance in instances)
                    {
                        var requisition = await (from e in _context.Employees
                                                 join r in _context.RequisitionedItems on e.EmployeeId equals r.EmployeeId
                                                 where r.RequisitionId == instance.RequisitionId
                                                 select new { EmployeeId = r.EmployeeId, EmployeeName = r.Employee.Name }).SingleOrDefaultAsync();

                        classificationResponse.ItemInstances.Add(new InstanceWithNameResponse
                        {
                            Id = instance.ItemInstanceId,
                            AssetId = instance.AssetId,
                            RequisitionEmployeeId = requisition?.EmployeeId,
                            RequisitionEmployeeName = requisition?.EmployeeName
                        });
                    }
                    response.ItemClassifications.Add(classificationResponse);
                }
                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return new JsonResult(new MessageResponse { Message = "Item Category Name is null, empty, or whitespace.", StatusCode = HttpStatusCode.BadRequest });
            }
            var hasName = await _context.ItemCategories.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.Name);
            if (hasName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemCategory = new ItemCategory
                    {
                        Name = input.Name,
                        CreateDate = DateTime.Now
                    };
                    await _context.ItemCategories.AddAsync(itemCategory);
                    await _context.SaveChangesAsync();

                    foreach (var classificationRequest in input.ItemClassifications)
                    {
                        var itemClassification = new ItemClassification
                        {
                            ItemCategoryId = itemCategory.ItemCategoryId,
                            Name = classificationRequest.Name,
                            CreateDate = DateTime.Now
                        };
                        await _context.ItemClassifications.AddAsync(itemClassification);
                        await _context.SaveChangesAsync();

                        foreach (var instanceRequest in classificationRequest.ItemInstances)
                        {
                            var instance = new ItemInstance
                            {
                                ItemClassificationId = itemClassification.ItemClassificationId,
                                AssetId = instanceRequest.AssetId,
                                CreateDate = DateTime.Now
                            };
                            await _context.ItemInstances.AddAsync(instance);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Id = itemCategory.ItemCategoryId, Message = "Items Create successfully.", StatusCode = HttpStatusCode.Created });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ItemUpdateRequest input)
        {
            var hasName = await _context.ItemCategories.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.Name && e.ItemCategoryId != input.Id);
            if (hasName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var oldCategory = await _context.ItemCategories.SingleAsync(i => i.ItemCategoryId == input.Id);
                    oldCategory.Name = input.Name;
                    oldCategory.UpdateDate = DateTime.Now;

                    List<int> deleteClassifications = new List<int>();
                    List<int> deleteInstances = new List<int>();

                    var oldClassifications = await _context.ItemClassifications
                                            .Where(i => i.ItemCategoryId == input.Id)
                                            .Select(x => x.ItemClassificationId)
                                            .ToListAsync();

                    var queryInstances = from c in _context.ItemClassifications
                                         join i in _context.ItemInstances on c.ItemClassificationId equals i.ItemClassificationId into ijoin
                                         from i in ijoin.DefaultIfEmpty()
                                         where c.ItemCategoryId == input.Id
                                         select new
                                         {
                                             c.ItemClassificationId,
                                             ItemInstanceId = (int?)i.ItemInstanceId,
                                             RequisitionId = (int?)i.RequisitionId,
                                         };

                    var oldInstance = queryInstances
                                    .AsEnumerable()
                                    .GroupBy(i => i.ItemClassificationId, i => i)
                                    .ToDictionary(x => x.Key, x => x.Where(y => y.ItemInstanceId.HasValue).Select(y => y.ItemInstanceId.Value).ToHashSet());

                    var classificationsWithId = input.ItemClassifications.Where(x => x.Id != null).ToDictionary(x => x.Id.Value);

                    foreach (var classification in oldClassifications)
                    {
                        if (!classificationsWithId.ContainsKey(classification))
                        {
                            deleteClassifications.Add(classification);
                            var instances = oldInstance[classification];
                            foreach (var instance in instances)
                            {
                                deleteInstances.Add(instance);
                            }
                        }
                    }

                    foreach (var classification in input.ItemClassifications)
                    {
                        if (classification.Id == null)
                        {
                            var newDbClassification = new ItemClassification
                            {
                                ItemCategoryId = oldCategory.ItemCategoryId,
                                Name = classification.Name,
                                CreateDate = DateTime.Now,
                                ItemInstances = new List<ItemInstance>()
                            };
                            await _context.ItemClassifications.AddAsync(newDbClassification);
                            await _context.SaveChangesAsync();

                            foreach (var newinstance in classification.ItemInstances)
                            {
                                var newDBInstance = new ItemInstance
                                {
                                    ItemClassificationId = newDbClassification.ItemClassificationId,
                                    AssetId = newinstance.AssetId,
                                    CreateDate = DateTime.Now
                                };
                                await _context.ItemInstances.AddAsync(newDBInstance);
                            }
                        }
                        else
                        {
                            var oldClassification = await _context.ItemClassifications.SingleAsync(oc => oc.ItemClassificationId == classification.Id);
                            oldClassification.Name = classification.Name;
                            oldClassification.UpdateDate = DateTime.Now;

                            foreach (var instance in classification.ItemInstances)
                            {
                                if (instance.Id == null)
                                {
                                    var newDbInstance = new ItemInstance
                                    {
                                        ItemClassificationId = oldClassification.ItemClassificationId,
                                        AssetId = instance.AssetId,
                                        CreateDate = DateTime.Now
                                    };
                                    await _context.ItemInstances.AddAsync(newDbInstance);
                                }
                                else
                                {
                                    var updateInstances = await _context.ItemInstances.SingleAsync(oi => oi.ItemInstanceId == instance.Id);
                                    updateInstances.AssetId = instance.AssetId;
                                    updateInstances.UpdateDate = DateTime.Now;
                                }
                            }

                            var instancesWithId = classification.ItemInstances
                                                .Where(x => x.Id != null)
                                                .Select(x => x.Id)
                                                .ToList();
                            var instances = oldInstance[classification.Id.Value];

                            foreach (var instanceId in instances)
                            {
                                if (!instancesWithId.Contains(instanceId))
                                {
                                    deleteInstances.Add(instanceId);
                                }
                            }
                        }
                    }
                    foreach (var instance in deleteInstances)
                    {
                        var itemInstance = await _context.ItemInstances.SingleAsync(x => x.ItemInstanceId == instance);
                        if (itemInstance.RequisitionId != null)
                            return new JsonResult(new MessageResponse { Message = "ItemInstance has not been returned and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });

                        var hasrequisition = await _context.RequisitionedItems.Where(ni => ni.ItemInstanceId == instance).ToListAsync();
                        foreach (var requisition in hasrequisition)
                        {
                            _context.RequisitionedItems.Remove(requisition);
                        }
                        _context.ItemInstances.Remove(itemInstance);
                    }
                    await _context.SaveChangesAsync();

                    foreach (var classification in deleteClassifications)
                    {
                        var itemClassification = await _context.ItemClassifications.SingleAsync(x => x.ItemClassificationId == classification);
                        _context.ItemClassifications.Remove(itemClassification);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Items update successfully.", StatusCode = HttpStatusCode.OK }); ;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ItemDeleteRequest input)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var itemCategory = await _context.ItemCategories.SingleAsync(i => i.ItemCategoryId == input.Id);
                    var hasClassification = await _context.ItemClassifications.Where(i => i.ItemCategoryId == input.Id).ToListAsync();
                    foreach (var classification in hasClassification)
                    {
                        var instances = await _context.ItemInstances.Where(i => i.ItemClassificationId == classification.ItemClassificationId).ToListAsync();
                        foreach (var instance in instances)
                        {
                            var requisitions = await _context.RequisitionedItems.Where(i => i.ItemInstanceId == instance.ItemInstanceId).ToListAsync();
                            foreach (var requisition in requisitions)
                            {
                                if (requisition.ReturnDate == null)
                                {
                                    return new JsonResult(new MessageResponse { Message = "ItemInstance has not been returned and cannot be deleted.", StatusCode = HttpStatusCode.BadRequest });
                                }
                                _context.RequisitionedItems.Remove(requisition);
                            }
                            _context.ItemInstances.Remove(instance);
                        }
                        _context.ItemClassifications.Remove(classification);
                    }
                    _context.ItemCategories.Remove(itemCategory);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "ItemCategory deleted successfully.", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchItemRequest input)
        {
            if (input.PageSize <= 0)
            {
                return new JsonResult(new MessageResponse { Message = "PageSize must be greater than or equal to 0.", StatusCode = HttpStatusCode.BadRequest });
            }

            int skip = input.Page * input.PageSize;
            IQueryable<ItemCategory> Items;

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                Items = _context.ItemCategories;
            }
            else
            {
                Items = _context.ItemCategories.Where(e => e.Name.ToLower().Contains(input.Name.ToLower()));
            }

            int totalItems = await Items.CountAsync();
            var paginatedItems = await Items.Skip(skip).Take(input.PageSize).ToListAsync();

            var data = paginatedItems.Select(i => new PaginationItemResponse
            {
                ItemCategoryId = i.ItemCategoryId,
                Name = i.Name
            }).ToList();

            var response = new SearchItemResponse<PaginationItemResponse>
            {
                Data = data,
                PageIndex = input.Page,
                PageSize = input.PageSize,
                RowCount = totalItems,
            };

            return new JsonResult(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetFreeItems()
        {
            var requisition = await (from ii in _context.ItemInstances
                                     join ic in _context.ItemClassifications on ii.ItemClassificationId equals ic.ItemClassificationId
                                     where ii.RequisitionId == null
                                     select new { AssetId = ii.AssetId, ClassificationName = ic.Name, ItemInstanceId = ii.ItemInstanceId }).ToListAsync();

            var data = requisition.Select(r => new FreeItemResponse
            {
                AssetId = r.AssetId,
                ClassificationName = r.ClassificationName,
                ItemInstanceId = r.ItemInstanceId,
            });

            return new JsonResult(data);
        }

        [HttpGet]
        public async Task<IActionResult> History([FromQuery] int id)
        {
            try
            {
                var history = await (from r in _context.RequisitionedItems
                                     join e in _context.Employees on r.EmployeeId equals e.EmployeeId
                                     where r.ItemInstanceId == id
                                     select new { Id = e.EmployeeId, Name = e.Name, RequisitionDate = r.RequisitonDate, ReturnDate = r.ReturnDate }).ToListAsync();

                var data = history.Select(h => new ItemHistoryResponse
                {
                    EmployeeId = h.Id,
                    EmployeeName = h.Name,
                    RequisitonDate = h.RequisitionDate,
                    ReturnDate = h.ReturnDate
                });

                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }
    }
}