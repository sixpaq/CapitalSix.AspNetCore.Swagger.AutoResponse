# CapitalSix.AspNetCore.Swagger.AutoResponse

This module automatically checks if response types like 400, 401, 403, 417, 429, 500 apply to methods and automatically adds it to the Swagger response documentation.

### How to configure the module.
```c#
void ConfigureServices(IServicesCollection services)
{
    ...
    services.AddSwaggerGen(c =>
        {
            // Add the response filter. This filter automatically adds
            // 400 and 500 response definition.
            c.OperationFilter<DefaultOperationResponseFilter>();
            
            ...
        });
    ...
}
```
