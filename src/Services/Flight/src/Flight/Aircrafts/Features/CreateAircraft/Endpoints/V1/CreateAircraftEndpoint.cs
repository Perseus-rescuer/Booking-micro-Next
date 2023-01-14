using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Web;
using Flight.Aircrafts.Dtos;
using Flight.Aircrafts.Features.CreateAircraft.Commands.V1;
using Flight.Aircrafts.Features.CreateAircraft.Dtos.V1;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.Annotations;

namespace Flight.Aircrafts.Features.CreateAircraft.Endpoints.V1;

using Hellang.Middleware.ProblemDetails;

public class CreateAircraftEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{EndpointConfig.BaseApiPath}/flight/aircraft", CreateAircraft)
            .RequireAuthorization()
            .WithTags("Flight")
            .WithName("CreateAircraft")
            .WithMetadata(new SwaggerOperationAttribute("Create Aircraft", "Create Aircraft"))
            .WithApiVersionSet(endpoints.NewApiVersionSet("Flight").Build())
            .WithMetadata(
                new SwaggerResponseAttribute(
                    StatusCodes.Status200OK,
                    "Aircraft Created",
                    typeof(AircraftResponseDto)))
            .WithMetadata(
                new SwaggerResponseAttribute(
                    StatusCodes.Status400BadRequest,
                    "BadRequest",
                    typeof(StatusCodeProblemDetails)))
            .WithMetadata(
                new SwaggerResponseAttribute(
                    StatusCodes.Status401Unauthorized,
                    "UnAuthorized",
                    typeof(StatusCodeProblemDetails)))
            .HasApiVersion(1.0);

        return endpoints;
    }

    private async Task<IResult> CreateAircraft(CreateAircraftRequestDto request, IMediator mediator, IMapper mapper,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateAircraftCommand>(request);

        var result = await mediator.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}
