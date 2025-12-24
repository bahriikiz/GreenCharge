using Microsoft.AspNetCore.Mvc;
using GreenCharge.Data;
using Microsoft.EntityFrameworkCore;

namespace GreenCharge.ViewComponents
{
    public class DashboardStatsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public DashboardStatsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // İstatistikler
            var stationCount = await _context.Stations.CountAsync();
            var reservationCount = await _context.Reservations.CountAsync();

            // Şehirleri tekrarsız (Distinct) olarak çek
            var cities = await _context.Stations
                                       .Select(s => s.City)
                                       .Distinct()
                                       .ToListAsync();

            // Verileri Tuple olarak gönderiyoruz (Sayılar ve Şehir Listesi)
            var model = (StationCount: stationCount, ReservationCount: reservationCount, Cities: cities);

            return View(model);
        }
    }
}