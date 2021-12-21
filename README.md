
Example
-------

The service receives an HTTP request:

	>>> GET /numbers?u=http://example.com/primes&u=http://foobar.com/fibo HTTP/1.0

It then retrieves the URLs specified as parameters.

The first URL returns this response:

	>>> GET /primes HTTP/1.0
	>>> Host: example.com
	>>> 
	<<< HTTP/1.0 200 OK
	<<< Content-Type: application/json
	<<< Content-Length: 34
	<<< 
	<<< { "numbers": [ 2, 3, 5, 7, 11, 13 ] }

The second URL returns this response:

	>>> GET /fibo HTTP/1.0
	>>> Host: foobar.com
	>>> 
	<<< HTTP/1.0 200 OK
	<<< Content-Type: application/json
	<<< Content-Length: 40
	<<< 
	<<< { "numbers": [ 1, 1, 2, 3, 5, 8, 13, 21 ] }

The service then calculates the result and returns it.

	<<< HTTP/1.0 200 OK
	<<< Content-Type: application/json
	<<< Content-Length: 44
	<<< 
	<<< { "numbers": [ 1, 2, 3, 5, 7, 8, 11, 13, 21 ] }




To improve your experience additional remote server will be provided to you.
The same server will be later used by us to evaluate the solution. 
It exposes single endpoint `/numbers` that can be used in a similar way 
like ones provided by the reference. 
This endpoint in its behaviour is trying to mimic real-world application.
Different payload sizes, status codes and response times can be expected.




Document your source code, both using comments and in a separate text file that 
describes the intentions and rationale behind your solution. Also write down 
any ambiguities that you see in the task description, and describe you how you 
interpreted them and why. If applicable, write automated tests for your code.

Please return your working solution within 7 days of receiving the challenge.

