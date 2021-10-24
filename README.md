# Minimal-Api

This project contains sample code demonstrating routing, handlers and validation in a .net 6.0 Minimal Api project.

This project is based off this youtube video: 

[How to structure & organise aspnetcore 6 mini api](https://www.youtube.com/watch?v=3SfA5m4CmAU)

Original Source code repo: https://github.com/raw-coding-youtube/aspnetcore-mini-api

3 Projects exist within the [Minimal-Api](https://github.com/tinytone/Minimal-Api/blob/master/Minimal-Api.sln) solution:
- [API](https://github.com/tinytone/Minimal-Api/tree/master/Api) - (.net 6) contains a minimal Api project with various /blog endpoints and 1 /test endpoint.
- [MVC](https://github.com/tinytone/Minimal-Api/tree/master/MVC) - (.net 6) contains a similar /test endpoint as the Api project for benchmarking comparisons.
- [Nbomber-netcore](https://github.com/tinytone/Minimal-Api/tree/master/Nbomber-netcore) - (.net 6) Runs benchmark comparison tests between the API and MVC projects.
# Testing out the Minimal Api

### Creating Blog Data

- Ensure the Api is running
- Run this curl command

```powershell
$ curl -i -X POST -H "Content-Type: application/json" -d "{\"title\":\"boi\"}" http://localhost:5000/admin/blogs

HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Sun, 24 Oct 2021 14:13:00 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"id":1,"title":"boi","createdBy":"bob master 3000","posts":[]}
```

- This uses an endpoint that requires Administrative permissions. (/admin/blogs)
- The user principle is mocked and hardcoded to be "bob master 3000" within the code. 
- This user is retrieved and set against the createdby field.

### Retrieving Blog Data:

Get Blog with Id 1:
    
```
$ curl --location --request GET 'http://localhost:5000/blogs/1'

{
    "id":1,
    "title":"boi",
    "createdBy":"bob master 3000",
    "posts":[]
}
```

- This assumes the previous "create blog" command has run as the database as in memory.

Get all Blogs:
    
```
$ curl --location --request GET 'http://localhost:5000/blogs'

[
    {
        "id":1,
        "title":"boi",
        "createdBy":"bob master 3000",
        "posts":[]
    }
]
```

Failure Scenario:

This should fail Validation as the Id need to be greater than 0.

```bash
$ curl --location --request GET 'http://localhost:5000/blogs/0'

{"message":"failed validation.","errors":{"Id":"Id needs to be greater than 0"}}
```

Testing out the /test endpoint:

```bash
$ curl -i -X POST -H "Content-Type: application/json" -d "{\"title\":\"test body\"}" "http://localhost:5000/test/1?v=test"
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
Date: Sun, 24 Oct 2021 14:05:02 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"result":"test body_1_test"}
```

# Testing out the MVC Project

```bash
$ curl --location --request POST 'http://localhost:5001/test/1?surname=Tony' --header 'Content-Type: application/json' --data-raw '{"title": "Mr"}'

{"result":"Mr_1_Tony"}
```

This correctly:
- strips out the Id from the Route (value = 1)
- strips out the surname from the QueryString (value = Tony)
- strips out the title from the Json Body (value = Mr)

All 3 inputs are stripped out and returned in the result: Mr_1_Tony

```c#
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpPost("/test/{id}")]
        public object GetBlog(int id, [FromQuery] string surname, [FromBody] TestRequest request)
        {
            return new
            {
                result = $"{request.Title}_{id}_{surname}"
            };
        }

        public class TestRequest
        {
            public string Title { get; set; }
        }
    }
```

The above code will automatically bind and string out the various inputs from the QueryString, Body and route.

In the Minimal Api project, this has been hand written via various interfaces:
- [IFromQuery](https://github.com/tinytone/Minimal-Api/blob/master/Api/Framework/IFromQuery.cs)
- [IFromRoute](https://github.com/tinytone/Minimal-Api/blob/master/Api/Framework/IFromRoute.cs)
- [IFromJsonBody](https://github.com/tinytone/Minimal-Api/blob/master/Api/Framework/IFromJsonBody.cs)

The **TestRequest** class (in the [Test.cs](https://github.com/tinytone/Minimal-Api/blob/master/Api/Services/Blogs/Test.cs) file)in the minimal Api project then has 3 ways of extracting the various inputs from the QueryString, Route and body:

```c#
    public class TestRequest : IRequest<object>, IFromJsonBody, IFromRoute, IFromQuery
    {
        public string Title { get; set; }

        public int FromRoute { get; set; }

        public string FromQuery { get; set; }

        public void BindFromQuery(IQueryCollection queryCollection)
        {
            if (queryCollection.TryGetValue("v", out var v))
            {
                this.FromQuery = v;
            }
        }

        public void BindFromRoute(RouteValueDictionary routeValues)
        {
            this.FromRoute = routeValues.GetInt("id");
        }
    }
```


# Assembly Scanning

### Fluent Validators

The Fluent Validators are added automatically using reflection and Assembly Scanning:

[program.cs](https://github.com/tinytone/Minimal-Api/blob/master/Api/Program.cs):

```c#
builder.Services.AddApiServices();
```

[RegisterServices.cs](https://github.com/tinytone/Minimal-Api/blob/master/Api/RegisterServices.cs):


```c#
public static void AddApiServices(this IServiceCollection services)
{
    // Assembly Scanners
    services.AddServices<Program>()
            .AddValidation<Program>();
}

public static IServiceCollection AddValidation<T>(this IServiceCollection services)
{
    var validators = typeof(T).Assembly.GetTypes()
        .Select(t => (t, t.BaseType))
        .Where((tuple) => tuple.BaseType != null)
        .Where((tuple) => tuple.BaseType.IsGenericType
            && tuple.BaseType.IsAbstract
            && tuple.BaseType.GetGenericTypeDefinition().IsEquivalentTo(typeof(AbstractValidator<>)))
        .Select((tuple) => (tuple.t, tuple.BaseType.GetGenericArguments()[0]));

    foreach (var v in validators)
    {
        var validorInterfaceType = typeof(IValidator<>).MakeGenericType(v.Item2);
        services.AddTransient(validorInterfaceType, v.Item1);
    }

    return services;
}
```

This Validator class would reside within the Mediatr handler e.g. [GetBlog.cs](https://github.com/tinytone/Minimal-Api/blob/master/Api/Services/Blogs/GetBlog.cs)

```c#
    public class GetBlogRequestValidation : AbstractValidator<GetBlogRequest>
    {
        public GetBlogRequestValidation()
        {
            this.RuleFor(r => r.Id)
                .Must(v => v > 0)
                .WithMessage("Id needs to be greater than 0");
        }
    }
```

This ensures that bad data passed into the API is caught and returned:

e.g. Navigate to http://localhost:5283/blogs/0

```json
{
  "message": "failed validation.",
  "errors": {
    "Id": "Id needs to be greater than 0"
  }
}
```


# Performance Testing

Use [Nbomber](https://github.com/PragmaticFlow/NBomber) to test performance between the API and MVC projects:

>Very simple load testing framework for Pull and Push scenarios. It's 100% written in F# and targeting .NET Core and full .NET Framework.

Note: Nick Chapsas talks about NBomber in [this](https://www.youtube.com/watch?v=mwHWPoKEmyY&t=143s) video.

### Using LinqPad 6 to run NBomber

The benchmarking comparison tests have been written in linq file which is associated with linqpad.

I downloaded LinqPad 6 from [here](https://www.linqpad.net/Download.aspx)

This is the benchmarking code within the [performance_test.linq](https://github.com/tinytone/Minimal-Api/blob/master/performance_test.linq) file:

```c#
void Main()
{
	var factory = HttpClientFactory.Create();
	var content = JsonContent.Create(new {title = "test title"});

	var mvc = Step.Create("mvc", factory, async context =>
	{
		var response = await context.Client.PostAsync("http://localhost:5001/test/1?v=test", content);

		return response.IsSuccessStatusCode
			? Response.Ok(statusCode: (int) response.StatusCode)
			: Response.Fail(statusCode: (int) response.StatusCode);
	});

	var minApi = Step.Create("min_api", factory, async context =>
	{
		var response = await context.Client.PostAsync("http://localhost:5000/test/1?v=test", content);

		return response.IsSuccessStatusCode
			? Response.Ok(statusCode: (int)response.StatusCode)
			: Response.Fail(statusCode: (int)response.StatusCode);
	});

	var mvc_scenario = ScenarioBuilder
		.CreateScenario("mvc_scenario", mvc)
		.WithWarmUpDuration(TimeSpan.FromSeconds(10))
		.WithLoadSimulations(Simulation.KeepConstant(24, TimeSpan.FromSeconds(60)));

	var minApiScenario = ScenarioBuilder
		.CreateScenario("min_api_scenario", minApi)
		.WithWarmUpDuration(TimeSpan.FromSeconds(10))
		.WithLoadSimulations(Simulation.KeepConstant(24, TimeSpan.FromSeconds(60)));
		
	NBomberRunner
	.RegisterScenarios(minApiScenario, mvc_scenario)
	.Run();
}
```

- Ensure both the API and MVC projects are started and running
- Run the benchmarks using linqpad

### Results:

```
  _   _   ____                        _                                        
 | \ | | | __ )    ___    _ __ ___   | |__     ___   _ __                      
 |  \| | |  _ \   / _ \  | '_ ` _ \  | '_ \   / _ \ | '__|                     
 | |\  | | |_) | | (_) | | | | | | | | |_) | |  __/ | |                        
 |_| \_| |____/   \___/  |_| |_| |_| |_.__/   \___| |_|                        
                                                                                
15:43:50 [INF] NBomber '2.1.1' Started a new session:
'2021-10-24_14.43.88_session_fc0e313f'.
15:43:51 [INF] NBomber started as single node.
15:43:51 [INF] Plugins: no plugins were loaded.
15:43:51 [INF] Reporting sinks: no reporting sinks were loaded.
15:43:51 [INF] Starting init...
15:43:51 [INF] Target scenarios: 'min_api_scenario', 'mvc_scenario'.
15:43:51 [INF] Init finished.
15:43:51 [INF] Starting warm up...
15:44:02 [INF] Starting bombing...
15:45:04 [INF] Stopping scenarios...
15:45:04 [INF] Calculating final statistics...

────────────────────────────────── test info ───────────────────────────────────

test suite: 'nbomber_default_test_suite_name'
test name: 'nbomber_default_test_name'

──────────────────────────────── scenario stats ────────────────────────────────

scenario: 'min_api_scenario'
duration: '00:01:00', ok count: 369672, fail count: 0, all data: 0 MB
load simulation: 'keep_constant', copies: 24, during: '00:01:00'
┌────────────────────┬─────────────────────────────────────────────────────┐
│               step │ ok stats                                            │
├────────────────────┼─────────────────────────────────────────────────────┤
│               name │ min_api                                             │
│      request count │ all = 369672, ok = 369672, RPS = 6161.2             │
│            latency │ min = 0.34, mean = 3.89, max = 75.11, StdDev = 3.28 │
│ latency percentile │ 50% = 3.16, 75% = 4.1, 95% = 8.24, 99% = 17.44      │
└────────────────────┴─────────────────────────────────────────────────────┘

status codes for scenario: min_api_scenario
┌─────────────┬────────┬─────────┐
│ status code │ count  │ message │
├─────────────┼────────┼─────────┤
│         200 │ 369672 │         │
└─────────────┴────────┴─────────┘

scenario: 'mvc_scenario'
duration: '00:01:00', ok count: 406111, fail count: 0, all data: 0 MB
load simulation: 'keep_constant', copies: 24, during: '00:01:00'
┌────────────────────┬────────────────────────────────────────────────────┐
│               step │ ok stats                                           │
├────────────────────┼────────────────────────────────────────────────────┤
│               name │ mvc                                                │
│      request count │ all = 406111, ok = 406111, RPS = 6768.5            │
│            latency │ min = 0.28, mean = 3.54, max = 74.7, StdDev = 3.77 │
│ latency percentile │ 50% = 2.64, 75% = 3.67, 95% = 8.42, 99% = 19.89    │
└────────────────────┴────────────────────────────────────────────────────┘

status codes for scenario: mvc_scenario
┌─────────────┬────────┬─────────┐
│ status code │ count  │ message │
├─────────────┼────────┼─────────┤
│         200 │ 406111 │         │
└─────────────┴────────┴─────────┘

──────────────────────────────────── hints ─────────────────────────────────────

hint for Scenario 'min_api_scenario':
Step 'min_api' in scenario 'min_api_scenario' didn't track data transfer. In 
order to track data transfer, you should use Response.Ok(sizeInBytes: value)

hint for Scenario 'mvc_scenario':
Step 'mvc' in scenario 'mvc_scenario' didn't track data transfer. In order to 
track data transfer, you should use Response.Ok(sizeInBytes: value)

15:45:04 [INF] Reports saved in folder:
'C:\Users\{My User}\AppData\Local\Temp\LINQPad6\_cvziirun\shadow-1\reports\2021-10-24_14.43.88_session_fc0e313f'
```

Interesting - the Requests per Second (RPS) were as follows:

- Minimal Api: 6161.2
- MVC: 6768.5

The MVC code outperformed the MinApi code which is different to the youtube results by Nick Chapsas and Raw coding.

The MVC controller isn't using Mediatr or Validation so would perform quicker due to less middleware code.

### Using .Net 6.0 to run NBomber

A 3rd project has been added to run the NBomber benchmark comparison. 

This is a Console Application written in .net 6.

Running NBomber:

```powershell
PS C:\SourceCode\GitHub\Minimal-Api\Nbomber-netcore\bin\Release\net6.0> .\Nbomber-netcore.exe
```

When using this project, the results were favourable towards the Minimal Api:

- Minimal Api: 21501.3 RPS
- MVC: 14926 RPS

```powershell
scenario: 'min_api_scenario'
duration: '00:01:00', ok count: 1290078, fail count: 0, all data: 0 MB
load simulation: 'keep_constant', copies: 24, during: '00:01:00'
┌────────────────────┬────────────────────────────────────────────────────┐
│               step │ ok stats                                           │
├────────────────────┼────────────────────────────────────────────────────┤
│               name │ min_api                                            │
│      request count │ all = 1290078, ok = 1290078, RPS = 21501.3         │
│            latency │ min = 0.13, mean = 1.11, max = 75.7, StdDev = 2.22 │
│ latency percentile │ 50% = 0.6, 75% = 0.89, 95% = 3.85, 99% = 9.84      │
└────────────────────┴────────────────────────────────────────────────────┘

status codes for scenario: min_api_scenario
┌─────────────┬─────────┬─────────┐
│ status code │  count  │ message │
├─────────────┼─────────┼─────────┤
│         200 │ 1290078 │         │
└─────────────┴─────────┴─────────┘

scenario: 'mvc_scenario'
duration: '00:01:00', ok count: 895560, fail count: 0, all data: 0 MB
load simulation: 'keep_constant', copies: 24, during: '00:01:00'
┌────────────────────┬────────────────────────────────────────────────────┐
│               step │ ok stats                                           │
├────────────────────┼────────────────────────────────────────────────────┤
│               name │ mvc                                                │
│      request count │ all = 895560, ok = 895560, RPS = 14926             │
│            latency │ min = 0.11, mean = 1.6, max = 63.52, StdDev = 2.87 │
│ latency percentile │ 50% = 0.74, 75% = 1.29, 95% = 5.8, 99% = 15.01     │
└────────────────────┴────────────────────────────────────────────────────┘

status codes for scenario: mvc_scenario
┌─────────────┬────────┬─────────┐
│ status code │ count  │ message │
├─────────────┼────────┼─────────┤
│         200 │ 895560 │         │
└─────────────┴────────┴─────────┘

──────────────────────────────────────────────────────── hints ─────────────────────────────────────────────────────────

hint for Scenario 'min_api_scenario':
Step 'min_api' in scenario 'min_api_scenario' didn't track data transfer. In order to track data transfer, you should
use Response.Ok(sizeInBytes: value)

hint for Scenario 'mvc_scenario':
Step 'mvc' in scenario 'mvc_scenario' didn't track data transfer. In order to track data transfer, you should use
Response.Ok(sizeInBytes: value)

16:33:38 [INF] Reports saved in folder:
'C:\SourceCode\GitHub\Minimal-Api\Nbomber-netcore\bin\Release\net6.0\reports\2021
-10-24_15.32.84_session_29bcba45'
PS C:\SourceCode\GitHub\Minimal-Api\Nbomber-netcore\bin\Release\net6.0> .\Nbomber-netcore.exe
```

# Summary

Minimal Api's now give you a table of contents of all your routes in your program file, where you can see what's Anonymous and what's Admin controlled:

```c#
Anonymous(
    app.MapGet<GetBlogsRequest>("/blogs"),
    app.MapGet<GetBlogRequest>("/blogs/{id}"),
    app.MapPost<TestRequest>("/test/{id}")
);

Admin(
    app.MapPost<CreateBlogRequest>("/admin/blogs")
);
```