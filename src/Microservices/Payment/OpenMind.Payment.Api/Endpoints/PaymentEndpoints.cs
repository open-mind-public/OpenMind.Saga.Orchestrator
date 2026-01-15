using MediatR;
using OpenMind.Payment.Application.Commands.RetryPayment;

namespace OpenMind.Payment.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/retry/{orderId:guid}", async (Guid orderId, IMediator mediator) =>
        {
            var command = new RetryPaymentCommand
            {
                OrderId = orderId
            };

            var result = await mediator.Send(command);

            return result.IsSuccess
                ? Results.Ok(new { Message = "Payment retry initiated", OrderId = orderId })
                : Results.BadRequest(new { Error = result.ErrorMessage, Code = result.ErrorCode });
        })
        .WithName("RetryPayment")
        .WithOpenApi()
        .WithDescription("Retry payment for a failed order");

        return app;
    }
}
