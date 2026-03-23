using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoMarket.Data;
using CryptoMarket.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CryptoMarket.Services
{
    public class GatewayService : ControllerBase
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
