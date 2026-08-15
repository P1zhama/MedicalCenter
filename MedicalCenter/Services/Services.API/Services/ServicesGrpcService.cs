using System.Globalization;
using Grpc.Core;
using MediatR;
using Services.Api.ErrorMapping;
using Services.Api.Protos;
using Services.Application.Commands.ChangeServiceStatus;
using Services.Application.Commands.ChangeSpecializationStatus;
using Services.Application.Commands.CreateService;
using Services.Application.Commands.CreateSpecialization;
using Services.Application.Commands.UpdateService;
using Services.Application.Commands.UpdateSpecialization;
using Services.Application.Queries.GetActiveSpecializations;
using Services.Application.Queries.GetServiceById;
using Services.Application.Queries.GetServiceCatalog;
using Services.Application.Queries.GetServiceForAppointment;
using Services.Application.Queries.GetServicesSummary;
using Services.Application.Queries.GetSpecializationById;
using Services.Application.Queries.GetSpecializations;
using Services.Application.Queries.IsSpecializationActive;
using Services.Domain.Enums;

namespace Services.Api.Services;

public class ServicesGrpcService : ServicesService.ServicesServiceBase
{
    private readonly ISender _sender;

    public ServicesGrpcService(ISender sender)
    {
        _sender = sender;
    }

    public override async Task<CreateSpecializationResponse> CreateSpecialization(
        CreateSpecializationRequest request,
        ServerCallContext context)
    {
        var services = request.Services
            .Select(item => new CreateSpecializationServiceItem(
                item.Name,
                ParsePrice(item.Price),
                ParseGuid(item.CategoryId, "category id"),
                ParseStatusOrDefault(item.Status)))
            .ToList();

        var command = new CreateSpecializationCommand(
            request.Name,
            ParseStatusOrDefault(request.Status),
            services);

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateSpecializationResponse { SpecializationId = result.Value.ToString() };
    }

    public override async Task<UpdateSpecializationResponse> UpdateSpecialization(
        UpdateSpecializationRequest request,
        ServerCallContext context)
    {
        var command = new UpdateSpecializationCommand(
            ParseGuid(request.SpecializationId, "specialization id"),
            request.Name,
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateSpecializationResponse();
    }

    public override async Task<ChangeSpecializationStatusResponse> ChangeSpecializationStatus(
        ChangeSpecializationStatusRequest request,
        ServerCallContext context)
    {
        var command = new ChangeSpecializationStatusCommand(
            ParseGuid(request.SpecializationId, "specialization id"),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ChangeSpecializationStatusResponse();
    }

    public override async Task<GetSpecializationsResponse> GetSpecializations(
        GetSpecializationsRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetSpecializationsQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetSpecializationsResponse();

        foreach (var specialization in result.Value)
        {
            response.Specializations.Add(new SpecializationListItem
            {
                SpecializationId = specialization.Id.ToString(),
                Name = specialization.Name,
                Status = specialization.Status
            });
        }

        return response;
    }

    public override async Task<GetActiveSpecializationsResponse> GetActiveSpecializations(
        GetActiveSpecializationsRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetActiveSpecializationsQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetActiveSpecializationsResponse();

        foreach (var specialization in result.Value)
        {
            response.Specializations.Add(new PublicSpecialization
            {
                SpecializationId = specialization.Id.ToString(),
                Name = specialization.Name
            });
        }

        return response;
    }

    public override async Task<SpecializationResponse> GetSpecializationById(
        GetSpecializationByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetSpecializationByIdQuery(ParseGuid(request.SpecializationId, "specialization id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var specialization = result.Value;

        var response = new SpecializationResponse
        {
            SpecializationId = specialization.Id.ToString(),
            Name = specialization.Name,
            Status = specialization.Status
        };

        foreach (var service in specialization.Services)
        {
            response.Services.Add(new ServiceListItem
            {
                ServiceId = service.Id.ToString(),
                Name = service.Name,
                Price = FormatPrice(service.Price),
                Status = service.Status,
                CategoryId = service.CategoryId.ToString(),
                CategoryName = service.CategoryName
            });
        }

        return response;
    }

    public override async Task<IsSpecializationActiveResponse> IsSpecializationActive(
        IsSpecializationActiveRequest request,
        ServerCallContext context)
    {
        var query = new IsSpecializationActiveQuery(ParseGuid(request.SpecializationId, "specialization id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new IsSpecializationActiveResponse { IsActive = result.Value };
    }

    public override async Task<CreateServiceResponse> CreateService(
        CreateServiceRequest request,
        ServerCallContext context)
    {
        var command = new CreateServiceCommand(
            request.Name,
            ParsePrice(request.Price),
            ParseGuid(request.SpecializationId, "specialization id"),
            ParseGuid(request.CategoryId, "category id"),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new CreateServiceResponse { ServiceId = result.Value.ToString() };
    }

    public override async Task<UpdateServiceResponse> UpdateService(
        UpdateServiceRequest request,
        ServerCallContext context)
    {
        var command = new UpdateServiceCommand(
            ParseGuid(request.ServiceId, "service id"),
            request.Name,
            ParsePrice(request.Price),
            ParseGuid(request.CategoryId, "category id"),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new UpdateServiceResponse();
    }

    public override async Task<ChangeServiceStatusResponse> ChangeServiceStatus(
        ChangeServiceStatusRequest request,
        ServerCallContext context)
    {
        var command = new ChangeServiceStatusCommand(
            ParseGuid(request.ServiceId, "service id"),
            ParseStatusOrDefault(request.Status));

        var result = await _sender.Send(command, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        return new ChangeServiceStatusResponse();
    }

    public override async Task<ServiceResponse> GetServiceById(
        GetServiceByIdRequest request,
        ServerCallContext context)
    {
        var query = new GetServiceByIdQuery(ParseGuid(request.ServiceId, "service id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var service = result.Value;

        return new ServiceResponse
        {
            ServiceId = service.Id.ToString(),
            Name = service.Name,
            Price = FormatPrice(service.Price),
            Status = service.Status,
            CategoryId = service.CategoryId.ToString(),
            CategoryName = service.CategoryName,
            SpecializationId = service.SpecializationId.ToString(),
            SpecializationName = service.SpecializationName
        };
    }

    public override async Task<GetServiceCatalogResponse> GetServiceCatalog(
        GetServiceCatalogRequest request,
        ServerCallContext context)
    {
        var result = await _sender.Send(new GetServiceCatalogQuery(), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetServiceCatalogResponse();

        foreach (var category in result.Value.Categories)
        {
            var categoryMessage = new CatalogCategory
            {
                CategoryId = category.Id.ToString(),
                Name = category.Name
            };

            foreach (var specialization in category.Specializations)
            {
                var specializationMessage = new CatalogSpecialization
                {
                    SpecializationId = specialization.Id.ToString(),
                    Name = specialization.Name
                };

                foreach (var service in specialization.Services)
                {
                    specializationMessage.Services.Add(new CatalogService
                    {
                        ServiceId = service.Id.ToString(),
                        Name = service.Name,
                        Price = FormatPrice(service.Price)
                    });
                }

                categoryMessage.Specializations.Add(specializationMessage);
            }

            response.Categories.Add(categoryMessage);
        }

        return response;
    }

    public override async Task<ServiceForAppointmentResponse> GetServiceForAppointment(
        GetServiceForAppointmentRequest request,
        ServerCallContext context)
    {
        var query = new GetServiceForAppointmentQuery(ParseGuid(request.ServiceId, "service id"));

        var result = await _sender.Send(query, context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var service = result.Value;

        return new ServiceForAppointmentResponse
        {
            ServiceId = service.Id.ToString(),
            Name = service.Name,
            Price = FormatPrice(service.Price),
            SpecializationId = service.SpecializationId.ToString(),
            CategoryId = service.CategoryId.ToString(),
            TimeSlotMinutes = service.TimeSlotMinutes,
            IsActive = service.IsActive
        };
    }

    public override async Task<GetServicesSummaryResponse> GetServicesSummary(
        GetServicesSummaryRequest request,
        ServerCallContext context)
    {
        var ids = request.ServiceIds.Select(id => ParseGuid(id, "service id")).ToList();

        var result = await _sender.Send(new GetServicesSummaryQuery(ids), context.CancellationToken);

        if (result.IsError)
            throw result.Errors.ToRpcException();

        var response = new GetServicesSummaryResponse();

        foreach (var service in result.Value)
        {
            response.Services.Add(new ServiceSummary
            {
                ServiceId = service.Id.ToString(),
                Name = service.Name
            });
        }

        return response;
    }

    private static Guid ParseGuid(string value, string fieldName)
        => Guid.TryParse(value, out var id)
            ? id
            : throw new RpcException(new Status(StatusCode.InvalidArgument, $"Invalid {fieldName} format."));

    private static decimal ParsePrice(string value)
        => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price)
            ? price
            : throw new RpcException(new Status(StatusCode.InvalidArgument, "You've entered an invalid price"));

    private static string FormatPrice(decimal price)
        => price.ToString("0.##", CultureInfo.InvariantCulture);

    private static ActivityStatus ParseStatusOrDefault(string status)
    {
        if (string.IsNullOrEmpty(status))
            return ActivityStatus.Active;

        if (!Enum.TryParse<ActivityStatus>(status, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid status."));

        return parsed;
    }
}
