using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MultipleAsyncInOne.APIs {

    public class Car {

        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public float Price { get; set; }
    }

    public class CarsController : BaseController {

        public static readonly ReadOnlyCollection<string> PayloadSources = new ReadOnlyCollection<string>(
            new List<string> { 
                "http://localhost:2700/api/cars/cheap",
                "http://localhost:2700/api/cars/expensive"
            }
        );

        public static readonly HttpClient HttpClient = new HttpClient();

        [HttpGet] // Asynchronous and not In Parallel
        public async Task<IEnumerable<Car>> AllCarsAsync() {

            var carsResult = new List<Car>();
            foreach (var uri in PayloadSources) {

                IEnumerable<Car> cars = await GetCars(uri);
                carsResult.AddRange(cars);
            }

            return carsResult;
        }

        [HttpGet] // Asynchronous and In Parallel (In a Blocking Fashion)
        public IEnumerable<Car> AllCarsInParallelBlockingAsync() {

            IEnumerable<Task<IEnumerable<Car>>> allTasks = PayloadSources.Select(uri => GetCars(uri));
            Task.WaitAll(allTasks.ToArray());

            return allTasks.SelectMany(task => task.Result);
        }

        [HttpGet] // Asynchronous and In Parallel (In a Non-Blocking Fashion)
        public async Task<IEnumerable<Car>> AllCarsInParallelNonBlockingAsync() {

            IEnumerable<Task<IEnumerable<Car>>> allTasks = PayloadSources.Select(uri => GetCars(uri));
            IEnumerable<Car>[] allResults = await Task.WhenAll(allTasks);

            return allResults.SelectMany(cars => cars);
        }

        // private helpers

        private async Task<IEnumerable<Car>> GetCars(string uri) {

            var response = await HttpClient.GetAsync(uri);
            var content = await response.Content.ReadAsAsync<IEnumerable<Car>>();

            return content;
        }
    }
}