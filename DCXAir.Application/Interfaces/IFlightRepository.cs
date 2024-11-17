using DCXAir.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCXAir.Application.Interfaces
{
    public interface IFlightRepository
    {
        List<Flight> GetRoutes();
    }
}
