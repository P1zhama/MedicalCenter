using Grpc.Core;
using MediatR;
using Offices.API.Protos;
using Offices.Application.Commands.ChangeOfficeStatus;
using Offices.Application.Commands.CreateOffice;
using Offices.Application.Commands.UpdateOffice;
using Offices.Application.Queries.GetOfficeById;
using Offices.Application.Queries.GetOffices;
using Offices.Domain.Enums;

namespace Offices.API.Services;

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

        var officeId = await _sender.Send(command, context.CancellationToken);

        return new CreateOfficeResponse
        {
            OfficeId = officeId.ToString(),
            Message = "Office created successfully."
        };
    }

    public override async Task<UpdateOfficeResponse> UpdateOffice(UpdateOfficeRequest request, ServerCallContext context)
    {
        var command = new UpdateOfficeCommand(
            Guid.Parse(request.OfficeId),
            request.City,
            request.Street,
            request.HouseNumber,
            NullIfEmpty(request.OfficeNumber),
            request.RegistryPhoneNumber,
            NullIfEmpty(request.PhotoUrl),
            ParseStatusOrDefault(request.Status));

        await _sender.Send(command, context.CancellationToken);

        return new UpdateOfficeResponse { Message = "Office updated successfully." };
    }

    public override async Task<ChangeOfficeStatusResponse> ChangeOfficeStatus(ChangeOfficeStatusRequest request, ServerCallContext context)
    {
        var command = new ChangeOfficeStatusCommand(
            Guid.Parse(request.OfficeId),
            ParseStatusOrDefault(request.Status));

        await _sender.Send(command, context.CancellationToken);

        return new ChangeOfficeStatusResponse { Message = "Office status changed successfully." };
    }

    public override async Task<GetOfficesResponse> GetOffices(GetOfficesRequest request, ServerCallContext context)
    {
        var offices = await _sender.Send(new GetOfficesQuery(), context.CancellationToken);

        var response = new GetOfficesResponse();

        foreach (var office in offices)
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

    public override async Task<OfficeResponse> GetOfficeById(GetOfficeByIdRequest request, ServerCallContext context)
    {
        var office = await _sender.Send(new GetOfficeByIdQuery(Guid.Parse(request.OfficeId)), context.CancellationToken);

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

    // US-31 F-7: статус по умолчанию — Active. Некорректное значение бросит ArgumentException,
    // которую интерцептор переведёт в InvalidArgument
    private static OfficeStatus ParseStatusOrDefault(string status) =>
        string.IsNullOrEmpty(status)
            ? OfficeStatus.Active
            : Enum.Parse<OfficeStatus>(status, ignoreCase: true);

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
}
