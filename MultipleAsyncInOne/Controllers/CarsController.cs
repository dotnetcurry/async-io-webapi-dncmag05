using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MultipleAsyncInOne.Controllers {

    public class Car {

        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public float Price { get; set; }
    }

    public class CarsController : BaseController {

        public static readonly string[] PayloadSources = new[] { 
            "http://localhost:2700/api/cars/cheap",
            "http://localhost:2700/api/cars/expensive"
        };

        [HttpGet] // Synchronous and not In Parallel
        public IEnumerable<Car> AllCarsSync() {

            List<Car> carsResult = new List<Car>();
            foreach (var uri in PayloadSources) {

                IEnumerable<Car> cars = GetCars(uri);
                carsResult.AddRange(cars);
            }

            return carsResult;
        }

        [HttpGet] // Synchronous and In Parallel
        public IEnumerable<Car> AllCarsInParallelSync() {

            ConcurrentDictionary<IEnumerable<Car>, bool> carsResult = new ConcurrentDictionary<IEnumerable<Car>, bool>();
            Parallel.ForEach(PayloadSources, uri => {

                IEnumerable<Car> cars = GetCars(uri);
                carsResult.TryAdd(cars, true);
            });

            return carsResult.SelectMany(x => x.Key);
        }

        [HttpGet] // Asynchronous and not In Parallel
        public async Task<IEnumerable<Car>> AllCarsAsync() {

            List<Car> carsResult = new List<Car>();
            foreach (var uri in PayloadSources) {

                IEnumerable<Car> cars = await GetCarsAsync(uri);
                carsResult.AddRange(cars);
            }

            return carsResult;
        }

        [HttpGet] // Asynchronous and In Parallel (In a Blocking Fashion)
        public IEnumerable<Car> AllCarsInParallelBlockingAsync() {

            // NOTE: As we are using async/await keyword for our client requests below,
            // it will use System.Threading.SynchronizationContext by default.
            // If we don't use ConfigureAwait(false), it will introduce deadlock here.
            
            IEnumerable<Task<IEnumerable<Car>>> allTasks = PayloadSources.Select(uri => GetCarsAsync(uri));
            Task.WaitAll(allTasks.ToArray());

            return allTasks.SelectMany(task => task.Result);
        }

        [HttpGet] // Asynchronous and In Parallel (In a Non-Blocking Fashion)
        public async Task<IEnumerable<Car>> AllCarsInParallelNonBlockingAsync() {

            IEnumerable<Task<IEnumerable<Car>>> allTasks = PayloadSources.Select(uri => GetCarsAsync(uri));
            IEnumerable<Car>[] allResults = await Task.WhenAll(allTasks);

            return allResults.SelectMany(cars => cars);
        }

        // private helpers

        private async Task<IEnumerable<Car>> GetCarsAsync(string uri) {

            using (HttpClient client = new HttpClient()) {

                var response = await client.GetAsync(uri).ConfigureAwait(false);
                var content = await response.Content.ReadAsAsync<IEnumerable<Car>>().ConfigureAwait(false);

                return content;
            }
        }

        private IEnumerable<Car> GetCars(string uri) {

            using (WebClient client = new WebClient()) {
                
                string carsJson = client.DownloadString(uri);
                IEnumerable<Car> cars = JsonConvert.DeserializeObject<IEnumerable<Car>>(carsJson);
                return cars;
            }
        }
    }
}