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
    public class EmployeeController : ControllerBase
    {
        private readonly EquipmentBorrowingContext _context;
        public EmployeeController(EquipmentBorrowingContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            try
            {
                var employee = await _context.Employees.SingleAsync(e => e.EmployeeId == id);

                var data = await (from ri in _context.RequisitionedItems
                                  join ii in _context.ItemInstances on ri.ItemInstanceId equals ii.ItemInstanceId
                                  join ic in _context.ItemClassifications on ii.ItemClassificationId equals ic.ItemClassificationId
                                  join cat in _context.ItemCategories on ic.ItemCategoryId equals cat.ItemCategoryId
                                  where ri.EmployeeId == id && ri.ReturnDate == null
                                  select new RequisitionedItemResponse
                                  {
                                      AssetId = ii.AssetId,
                                      ItemCategoryId = cat.ItemCategoryId,
                                      ItemCategoryName = cat.Name,
                                      ItemClassificationId = ic.ItemClassificationId,
                                      ItemClassificationName = ic.Name,
                                      InstanceId = ii.ItemInstanceId,
                                      RequisitonDate = ri.RequisitonDate,
                                      requisitionId = ri.RequisitionId
                                  }).ToListAsync();

                return new JsonResult(new GetByIdEmployeeResponse
                {
                    EmployeeId = employee.EmployeeId,
                    FullName = employee.Name,
                    RequisitionedItems = data
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest input)
        {
            var hasName = await _context.Employees.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.FullName);
            if (hasName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var employee = new Employee
                    {
                        Name = input.FullName,
                        CreateDate = DateTime.Now
                    };

                    await _context.Employees.AddAsync(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Id = employee.EmployeeId, Message = "Employee Created successfully.", StatusCode = HttpStatusCode.Created });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UpdateEmployeeRequest input)
        {
            var AnyName = await _context.Employees.AnyAsync(e => EF.Functions.Collate(e.Name, "utf8mb4_bin") == input.FullName && e.EmployeeId != input.Id);
            if (AnyName)
            {
                return new JsonResult(new MessageResponse { Message = "Name is already in use.", StatusCode = HttpStatusCode.Conflict });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var employee = await _context.Employees.SingleAsync(e => e.EmployeeId == input.Id);
                    employee.Name = input.FullName;
                    employee.UpdateDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Employee Updated successfully.", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteEmployeeRequest input)
        {
            var requisition = await _context.RequisitionedItems.AnyAsync(e => e.EmployeeId == input.Id && e.ReturnDate == null);
            if (requisition)
            {
                return new JsonResult(new MessageResponse { Message = "Employee is currently borrowing", StatusCode = HttpStatusCode.Conflict });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var employee = await _context.Employees.SingleAsync(e => e.EmployeeId == input.Id);
                    var employeerequistion = await _context.RequisitionedItems.SingleOrDefaultAsync(r => r.EmployeeId == input.Id);
                    if (employeerequistion != null)
                    {
                        _context.RequisitionedItems.Remove(employeerequistion);
                    }
                    _context.Employees.Remove(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return new JsonResult(new MessageResponse { Message = "Employee deleted successfully.", StatusCode = HttpStatusCode.OK });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new JsonResult(new MessageResponse { Message = $"An error occurred: {ex.Message}", StatusCode = HttpStatusCode.InternalServerError });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Search([FromBody] SearchEmployeeRequest input)
        {
            if (input.PageSize <= 0)
            {
                return new JsonResult(new MessageResponse { Message = "PageSize must be greater than or equal to 0.", StatusCode = HttpStatusCode.BadRequest });
            }

            int skip = input.Page * input.PageSize;
            IQueryable<Employee> employee;

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                employee = _context.Employees;
            }
            else
            {
                employee = _context.Employees.Where(e => e.Name.ToLower().Contains(input.Name.ToLower()));
            }

            int totalItems = await employee.CountAsync();
            var paginatedItems = await employee.Skip(skip).Take(input.PageSize).ToListAsync();

            var data = paginatedItems.Select(i => new GetPaginatedEmployee
            {
                EmployeeId = i.EmployeeId,
                FullName = i.Name
            }).ToList();

            var response = new GetSearchEmployee
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