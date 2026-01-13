namespace OpenMind.Fulfillment.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app, string serviceName)
    {
        app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Service = serviceName }))
            .WithName("HealthCheck")
            .WithOpenApi();

        return app;
    }
}
