using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Retail.Api.Orders.CustomInterface;
using Retail.Api.Orders.Data;
using Retail.Api.Orders.DefaultRepositories;
using Retail.Api.Orders.Dto;
using Retail.Api.Orders.Interface;
using Retail.Api.Orders.Model;
using System.Collections.Immutable;

namespace Retail.Api.Orders.Repositories
{
    /// <summary>
    /// Customer repository class.
    /// </summary>
    public class OrderEntityRepository : EntityRepository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderEntityRepository"/> class.
        /// </summary>
        /// <param name="context">Db context.</param>
        public OrderEntityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets collection of object asynchronously.
        /// </summary>
        /// <returns>Returns collection of object of type parameter T.</returns>
        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var list = await (from o in _context.Orders
                       select new OrderDto
                       {
                           Id = o.Id,
                           CustomerId = o.CustomerId,
                           OrderDate = o.OrderDate,
                           TotalAmount = o.TotalAmount,
                           LineItems = _context.LineItems != null ? _context.LineItems
                                        .Where(i => i.OrderId == o.Id)
                                        .Select(i => new LineItemDto
                                        {
                                            Id= i.Id,
                                            OrderId = i.OrderId,
                                            SkuId = i.SkuId,
                                            Qty = i.Qty,
                                        })
                                        .ToList() : null, 
                       }).ToListAsync<OrderDto>();

            return list;
        }

        /// <summary>
        /// Gets object by Id asynchronously.
        /// </summary>
        /// <param name="id">Id of object.</param>
        /// <returns>Returns object.</returns>
        public async Task<OrderDto?> GetOrderByIdAsync(long id)
        {
            var obj = await (from o in _context.Orders
                              where o.Id == id
                              select new OrderDto
                              {
                                  Id = o.Id,
                                  CustomerId = o.CustomerId,
                                  OrderDate = o.OrderDate,
                                  TotalAmount = o.TotalAmount,
                                  LineItems = _context.LineItems != null ? _context.LineItems
                                               .Where(i => i.OrderId == o.Id)
                                               .Select(i => new LineItemDto
                                               {
                                                   Id = i.Id,
                                                   OrderId = i.OrderId,
                                                   SkuId = i.SkuId,
                                                   Qty = i.Qty,
                                               })
                                               .ToList() : null,
                              }).FirstOrDefaultAsync();
            return obj;
        }
    }
}

