using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCXAir.Domain.Entities;

namespace DCXAir.Application.Interfaces
{
    public interface IFlightService
    {
        List<Journey> SearchFlights(string origin, string destination, string currency);

    }

}
