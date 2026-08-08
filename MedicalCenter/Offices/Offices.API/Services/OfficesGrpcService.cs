using Grpc.Core;
using MediatR;
using Offices.Api.ErrorMapping;
using Offices.Api.Protos;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Application.Commands.CreateOffice;
using Offices.Application.Commands.UpdateOffice;
using Offices.Application.Queries.GetActiveOffices;
using Offices.Application.Queries.GetOfficeById;
using Offices.Application.Queries.GetOffices;
using Offices.Application.Queries.IsOfficeActive;
using Offices.Domain.Enums;

namespace Offices.Api.Services;

public class OfficesGrpcService : OfficesService.OfficesServiceBase
{
    private readonly ISender _sender;

    public OfficesGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreateOfficeResponse> CreateOffice(CreateOfficeRequest request, ServerCallContext context)
    {
        var command = new CreateOfficeCommand(
            request.City,
            request.Street,
            request.HouseNumber,
            NullIfEmpty(request.OfficeNumber),
            request.RegistryPhoneNumber,
            NullIfEmpty(request.PhotoUrl),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateOfficeResponse
        {
            OfficeId = result.Value.ToString()
        };
    }

    public override async Task<UpdateOfficeResponse> UpdateOffice(UpdateOfficeRequest request, ServerCallContext context)
    {
        var command = new UpdateOfficeCommand(
            ParseGuid(request.OfficeId),
            request.City,
            request.Street,
            request.HouseNumber,
            NullIfEmpty(request.OfficeNumber),
            request.RegistryPhoneNumber,
            NullIfEmpty(request.PhotoUrl),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateOfficeResponse();
    }

    public override async Task<ChangeOfficeStatusResponse> ChangeOfficeStatus(ChangeOfficeStatusRequest request, ServerCallContext context)
    {
        var command = new ChangeOfficeStatusCommand(
            ParseGuid(request.OfficeId),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ChangeOfficeStatusResponse();
    }

    public override async Task<GetOfficesResponse> GetOffices(GetOfficesRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new GetOfficesQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetOfficesResponse();

        foreach (var office in result.Value)
        {
            response.Offices.Add(new OfficeListItem
            {
                OfficeId = office.Id.ToString(),
                Address = office.Address,
                Status = office.Status,
                RegistryPhoneNumber = office.RegistryPhoneNumber
            });
        }

        return response;
    }

    public override async Task<GetActiveOfficesResponse> GetActiveOffices(GetActiveOfficesRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new GetActiveOfficesQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetActiveOfficesResponse();

        foreach (var office in result.Value)
        {
            response.Offices.Add(new PublicOffice
            {
                OfficeId = office.Id.ToString(),
                Address = office.Address,
                PhotoUrl = office.PhotoUrl ?? string.Empty,
                RegistryPhoneNumber = office.RegistryPhoneNumber
            });
        }

        return response;
    }

    public override async Task<OfficeResponse> GetOfficeById(GetOfficeByIdRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new GetOfficeByIdQuery(ParseGuid(request.OfficeId)), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var office = result.Value;

        return new OfficeResponse
        {
            OfficeId = office.Id.ToString(),
            PhotoUrl = office.PhotoUrl ?? string.Empty,
            Address = office.Address,
            City = office.City,
            Street = office.Street,
            HouseNumber = office.HouseNumber,
            OfficeNumber = office.OfficeNumber ?? string.Empty,
            Status = office.Status,
            RegistryPhoneNumber = office.RegistryPhoneNumber
        };
    }

    public override async Task<IsOfficeActiveResponse> IsOfficeActive(IsOfficeActiveRequest request, ServerCallContext context)
    {
        var result = await _sender.Send(new IsOfficeActiveQuery(ParseGuid(request.OfficeId)), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new IsOfficeActiveResponse { IsActive = result.Value };
    }

    private static Guid ParseGuid(string value)
        => Guid.TryParse(value, out var id)
            ? id
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid office id format."));

    private static OfficeStatus ParseStatusOrDefault(string status)
    {
        if (string.IsNullOrEmpty(status))
            return OfficeStatus.Active;

        if (!Enum.TryParse<OfficeStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid office status."));

        return parsed;
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
}
