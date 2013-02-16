using System.Collections.Generic;
using System.Threading;
using System.Web.Http;
using PayloadApplication.Models;
using System.Threading.Tasks;

namespace PayloadApplication.APIs {

    public class CarsController : ApiController {

        private const int MillisecondsDelay = 500;
        readonly CarsContext _carsContext = new CarsContext();

        [HttpGet]
        public async Task<IEnumerable<Car>> Cheap() {

            await Task.Delay(MillisecondsDelay);
            return _carsContext.GetCars(car => car.Price < 50000);
        }

        [HttpGet]
        public async Task<IEnumerable<Car>> Expensive() {

            await Task.Delay(MillisecondsDelay);
            return _carsContext.GetCars(car => car.Price >= 50000);
        }
    }
}