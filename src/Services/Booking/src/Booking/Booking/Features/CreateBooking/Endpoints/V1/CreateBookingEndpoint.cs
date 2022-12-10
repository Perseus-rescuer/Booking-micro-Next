using Booking.Booking.Features.CreateBooking.Commands.V1;
using Booking.Booking.Features.CreateBooking.Dtos.V1;
using BuildingBlocks.Web;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Booking.Booking.Features.CreateBooking.Endpoints.V1;

public class CreateBookingEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{EndpointConfig.BaseApiPath}/booking", CreateBooking)
            .RequireAuthorization()
            .WithTags("Booking")
            .WithName("CreateBooking")
            .WithMetadata(new SwaggerOperationAttribute("Create Booking", "Create Booking"))
            .WithApiVersionSet(endpoints.NewApiVersionSet("Booking").Build())
            .Produces<ulong>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .HasApiVersion(1.0);

        return endpoints;
    }

    private async Task<IResult> CreateBooking(CreateBookingRequestDto request, IMediator mediator, IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateBookingCommand>(request);

        var result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}
