# Minimal-Api

This project contains sample code demonstrating routing, handlers and validation in a .net 6.0 Minimal Api project.

This project is based off this youtube video: 

[How to structure & organise aspnetcore 6 mini api](https://www.youtube.com/watch?v=3SfA5m4CmAU)

Original Source code repo: https://github.com/raw-coding-youtube/aspnetcore-mini-api


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

This should fail Validation as the Id need to be greater than 0.

```bash
$ curl --location --request GET 'http://localhost:5000/blogs/0'

{"message":"failed validation.","errors":{"Id":"Id needs to be greater than 0"}}
```

Testing out the /test endpoint:

```
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

# Assembly Scanning

### Fluent Validators

The Fluent Validators are added automatically using reflection and Assembly Scanning:

program.cs:

```c#
builder.Services.AddApiServices();
```

RegisterServices.cs:


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

This Validator class would reside within the Mediatr handler e.g. GetBlog.cs

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

Use Nbomber to test performance between the API and MVC projects:

