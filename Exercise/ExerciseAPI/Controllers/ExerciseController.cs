using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExerciseAPI.Controllers
{
    [ApiController]
    [Route("/")]
    public class ExerciseController : ControllerBase
    {
        private readonly string _pattern;
        private readonly Regex _reg;
        private readonly ILogger<ExerciseController> _logger;
        private CancellationTokenSource cancel;
        private CancellationToken token;
        private readonly static int max_degree_of_concurrency= 15;

        //max timeout allowed for retriving, merging, duplicate removing and sorting external URLs information
        //due to delay to response and write output to stream for large lists, this number is below the 500 (target)
        private static readonly int timeout = 470;
        private readonly HttpClient _client;

        public ExerciseController(ILogger<ExerciseController> logger)
        {

            cancel = new CancellationTokenSource(timeout);
            token = cancel.Token;

            _client = new HttpClient();
            _logger = logger;
            _pattern = @"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$";
            _reg = new Regex(_pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// Returns a json response including the merged and sorted numbers extracted from URLs
        /// </summary>
        /// <param name="u">List of URLs to fetch source numbers from</param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("numbers")]
        [HttpGet]

        public ActionResult numbers([FromQuery] string[] u)
        {
            if (u.Length == 0)
            {
                return Problem(detail: $"The input was empty. you need to privide the URLs.", statusCode: 400);
            }
            var parallelism_sem = new SemaphoreSlim(max_degree_of_concurrency, max_degree_of_concurrency);
            //the locker to lock on across tasks
            object _locker = new object();
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Restart();
                var result = new List<int>();
                var Third_list = new List<int>();
                object set_lock = new object();


                var tasks = new List<Task>() { };
                //creating a task foreach URL
                foreach (var uri in u)
                {

                    var t1 = Task.Factory.StartNew(() =>
                    {
                        parallelism_sem.Wait();
                        if (!IsUrlValid(uri))
                        {
                            Console.WriteLine($"URL isn't valid {uri}");
                            parallelism_sem.Release();
                            return;
                        }
                        var numbers = new List<int>();
                        try
                        {
                            //sort and remove dups from local result
                            numbers = get_uri_numbers(uri).ToHashSet().OrderBy(x => x).ToList();
                            if (!numbers.Any())
                            {
                                // Console.WriteLine("timeout or error retriving numbers");
                                parallelism_sem.Release();


                                return;
                            }
                        }
                        catch (Exception ex)
                        {
                            // Console.WriteLine("get request failed");
                            //logging ex
                            parallelism_sem.Release();


                            return;
                        }

                        var local_index = 0;
                        var result_index = 0;
                        lock (_locker)
                        {
                            var local_count = numbers.Count;
                            Third_list = new List<int>();
                            var result_count = result.Count;
                            while (true)
                            {
                                //handling the cancelation operation within the loop
                                if (cancel.IsCancellationRequested)
                                {

                                    return;
                                }
                                if (local_index < local_count && (result_index >= result_count || numbers[local_index] < result.ElementAt(result_index)))
                                {
                                    //preventing to push duplicates
                                    if (!Third_list.Any() || Third_list.Last() != numbers[local_index])
                                        Third_list.Add(numbers[local_index]);
                                    local_index++;
                                }

                                else if (result_index < result_count && (local_index >= local_count || numbers[local_index] > result.ElementAt(result_index)))
                                {
                                    //preventing to push duplicates
                                    if (!Third_list.Any() || Third_list.Last() != result.ElementAt(result_index))
                                        Third_list.Add(result.ElementAt(result_index));
                                    result_index++;
                                }
                                //move both pointers and insert duplicate value only once
                                else if (result_index < result_count && local_index < local_count && numbers[local_index] == result.ElementAt(result_index))
                                {
                                    Third_list.Add(result.ElementAt(result_index));
                                    local_index++;
                                    result_index++;
                                }
                                //break if traversing both lists is finished
                                if (result_index >= result_count && local_index >= local_count)
                                {
                                    break;
                                }

                            }
                            // now the Third_list is sorted but we need to update result as well for the use of the other tasks
                            result = new List<int>(Third_list);
                        }
                        parallelism_sem.Release();


                    }, cancellationToken: token);
                    tasks.Add(t1);


                }
                //the time which was needed for creation of tasks
                var spent_time = sw.Elapsed.TotalMilliseconds;
                //calculatig the remaining time to fetch, merge and sort result
                var remaining_time = timeout - (int)spent_time;
                if (remaining_time > 0)
                {
                    Task.WaitAll(tasks.ToArray(), remaining_time);
                }
                //cancel all running tasks
                cancel.Cancel();
                Console.WriteLine($"({nameof(numbers)}) {DateTime.Now.ToString("HH:mm:ss")} {(int)sw.Elapsed.TotalMilliseconds} ms Completed Tasks:{tasks.Where(x => x.IsCompletedSuccessfully).Count()}  Result Length:{Math.Max(Third_list.Count(), result.Count())}");
                //return the longer ordered result
                //to prevent the short result due to timeout in the middle of copying elements to result or just before it
                return Ok(new { numbers = Third_list.Count() > result.Count() ? Third_list : result });


            }
            catch (Exception ex)
            {

                return Problem(detail: $"something went wrong. ({ex.ToString()})", statusCode: 500);
            }

        }
        /// <summary>
        /// Checks wether a string is in corect URL format or not
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool IsUrlValid(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            return _reg.IsMatch(url);
        }
        /// <summary>
        /// Gets numbers from an provided URL address
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        private IEnumerable<int> get_uri_numbers(string uri)
        {
            try
            {



                //HTTP GET
                var responseTask = _client.GetAsync(uri, token);
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var stringResult = result.Content.ReadAsStringAsync();
                    var dict = JsonSerializer.Deserialize<Dictionary<string, int[]>>(stringResult.Result);
                    var numbers = dict["numbers"];



                    return numbers;
                }
                else
                {
                    // Console.WriteLine($"HTTP GET failed. {result.StatusCode.ToString()}");
                    //log error
                }


            }
            catch (OperationCanceledException ex)
            {

                // Console.WriteLine($"HTTP GET failed. {ex.Message}");
            }
            catch (Exception ex)
            {

                // Console.WriteLine($"HTTP GET failed. {ex.Message}");
            }

            return new List<int>();
        }


    }
}
