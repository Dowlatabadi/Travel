using ExerciseAPI.Controllers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            var controller = new ExerciseController(null);
           
            Stopwatch sw = new Stopwatch();
            // Act
            sw.Restart();
            var wins1 = 0;
            var wins2 = 0;
            var wins3 = 0;
            var winslist1 = new List<float>();
            var winslist2 = new List<float>();
            var winslist3 = new List<float>();
            var test_cases =60;
            var time_limit = 500;
            for (int i = 0; i < test_cases; i++)
            {
                sw.Restart();
                var result = controller.numbers1();
                var time1 = sw.Elapsed.TotalMilliseconds;

                if (result.  && time1< time_limit)
                {
                    wins1++;
                    winslist1.Add(result.Item2);

                }
                sw.Restart();
                var result2 = controller.numbers2().Result;
                 time1 = sw.Elapsed.TotalMilliseconds;

                if (result2.Item2 == 1f && time1 < time_limit)
                {
                    wins2++;
                    winslist2.Add(result2.Item2);
                }
                sw.Restart();
                var result3 = controller.numbers3().Result;
                time1 = sw.Elapsed.TotalMilliseconds;

                if (result3.Item2== 1f && time1 < time_limit)
                {
                    wins3++;
                    winslist3.Add(result3.Item2);
                }
            }

            // Assert
            var time = sw.Elapsed.TotalSeconds;
            //var max1 =Math.Max( Math.Max(winslist1.Max(), winslist2.Max()), winslist3.Max());
            //var acc_ratio1 = (float)winslist1.Where(x => x == max1).Count() / winslist1.Count();
            //var acc_ratio2= (float)winslist2.Where(x => x == max1).Count() / winslist2.Count();
            //var acc_ratio3= (float)winslist3.Where(x => x == max1).Count() / winslist3.Count();
            var win_ratio1 = (float)wins1 / test_cases;
            var win_ratio2 = (float)wins2 / test_cases;
            var win_ratio3 = (float)wins3 / test_cases;
        }
    }
}
