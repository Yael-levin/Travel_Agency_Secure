using Microsoft.AspNetCore.Mvc.Filters;
using TravelAgency_Secure.Data;
using TravelAgency_Secure.Services;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
public class SystemMaintenanceFilter : IActionFilter
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    public SystemMaintenanceFilter(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // 1️⃣ Auto cancel להזמנות Waiting List
        var autoCancelService = new AutoCancelService(_context, _config);
        autoCancelService.Run();

        // 2️⃣ פקיעת הנחות
        ExpireDiscounts();
    }

    private void ExpireDiscounts()
    {
        var expiredDiscounts = _context.Trips
            .Where(t =>
                t.DiscountPrice != null &&
                t.DiscountEndDate != null &&
                t.DiscountEndDate < DateTime.Today
            )
            .ToList();

        foreach (var trip in expiredDiscounts)
        {
            trip.DiscountPrice = null;
            trip.DiscountEndDate = null;
        }

        if (expiredDiscounts.Any())
        {
            _context.SaveChanges();
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
