using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayZone.BLL.Interfaces;
using PlayZone.DAL.Data.Contexts;
using PlayZone.DAL.Models;

namespace PlayZone.BLL.Repositories
{
    public class DeviceRepository : GenericRepository<Device>, IDeviceRepository
    {
        private readonly PlayZoneDbContext _context;

        public DeviceRepository(PlayZoneDbContext context) : base(context)
        {
            this._context = context;
        }


    }
}
