using System.Collections.Generic;
using System.Threading.Tasks;
using DarkMarket.Data;
using DarkMarket.Models;
using Microsoft.EntityFrameworkCore;

namespace DarkMarket.Services
{
    public class GatewayService
    {
        private readonly AppDbContext _db;

        public GatewayService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<GatewayInfo>> GetAllAsync()
        {
            return await _db.Gateways.ToListAsync();
        }

        public async Task<bool> SetStatusAsync(string name, bool enabled)
        {
            var gateway = await _db.Gateways.FindAsync(name);
            if (gateway == null) return false;
            gateway.Enabled = enabled;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}