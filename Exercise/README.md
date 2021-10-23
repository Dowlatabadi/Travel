Travel Audience Challenge - Solution
============================

Summary
----

The API exposes endpoint "/numbers". This endpoint receives a list of URLs 
though GET query parameters. The parameter in use is called "u". It can appear 
more than once.

	http://yourserver:8080/numbers?u=http://example.com/primes&u=http://foobar.com/fibo
The Algorithm
-----
All the process is done within tasks/threads and main thread just returns the answer.

When the /numbers is called, the service will retrieve each of these URLs if 
they turn out to be syntactically valid URLs with the use of `Regex` matchings.


Since The endpoint needs to return the result always 
within 500 milliseconds, the solution uses multi-task processing with the max limit of 500 milliseconds for the completion. If a URL takes too long to respond, it must be ignored.
 After retrieving the numbers through http `get`, each task would `sort` its numbers.

**Notice**: In this step task does not remove duplicate values and only the sorting process would happen.
I have defined an object as a locker to access and modify cross-thread lists that would be returned as final answers. 
After locking, the task tries to merge its numbers (and remove duplicates) with the `final list` in `O(n)` with the help of a merging sorted lists algorithm and the result would be stored in a `third list`.

The whole process is a multi-task process like below chart:

![alt text](https://raw.githubusercontent.com/Dowlatabadi/Travel/master/Exercise/Capture.PNG)

The `3rd list` needs to be copied to `result list` to have updated results.

**Notice**: if the task time outs just before copying the `3rd list` to `final list` or in middle of the copying, `final list` is not complete and the `3rd list` would come with longer and better results, so at the end of the process a comparison between lengthes of `3rd` and `final list` would reveal the best result.



Since the provided solution, *all the time* can have a relativily and partially correct answer, It only returns an empty list as result only if all URLs returned errors or took too long to respond.
The `Timeout` is set on parallel tasks running time, therefore The timeout is set regardless of 
the size of the data.





Time Complexity
---------------------
since every request localy would be sorted using builtin quick sort sorting algorithm, the order is `O(n*lg(n))`, where `n` is the length of local task's number list. 


After local sorting, since the merging of sorted lists happens in a locked process. it's time complexity can be evaluated like a sequential process, therefore the order of this part is:
    
    O(l1+l2+(l1+l2+l3)+....)=O(n*l1+(n-1)*l2+....)

If the number of `l`s(URLs) are constant number with boundaries, the order can be rewitten:

    o(n^2)

Overal time complixity:

    O(n^2+n*lg(n))=O(n^2)


Additional Details
---------------------

I only used what's provided in the .NET 
standard library. 

To test with the help of the provided go server (assuming it listens to port 8090), the URLs should be in correct format having the `http` portion:

    http://localhost:8090/primes

So the whole get URL should be like this:

    http://localhost:8080/numbers?u=http://localhost:8090/primes&u=http://localhost:8090/odd&u=http://localhost:8090/rand&u=http://localhost:8090/fibo

Sample oresponse body:

    {"numbers":[1,2,3,5,7,8,9,10,11,13,15,17,19,21,23,24,27,34,76]}

Installation
--------
The solution is a .net 5 API project. It can be run in different ways:

1. using dotnet cli: 
 
        ..\Exercise\ExerciseAPI>dotnet run .
2. using the docker:
  
        ..\Exercise>docker build . -t name
        docker run -detach -p 8080:80 name
3. using visual studio itself and running the solution.

In all cases the swagger is accessible here:

        http://localhost:8080/swagger/index.html
    
And the API end point is:

        http://localhost:8080/numbers

  
Conclusion
--------

 Although there can be other and slightly faster solutions with the sorting in the main thread, this solution tries to respect the timeout restriction, therefore the whole process is happening within the threads/tasks by facilitating locks and a fast merging algorithm.



