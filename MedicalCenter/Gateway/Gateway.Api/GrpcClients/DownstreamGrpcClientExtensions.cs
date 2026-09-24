using System;
using System.Linq;
using Gateway.Api.Interceptors;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoServiceDescriptor = Google.Protobuf.Reflection.ServiceDescriptor;

namespace Gateway.Api.GrpcClients;

public static class DownstreamGrpcClientExtensions
{
    private static readonly string[] ReadMethodPrefixes = ["Get", "Is", "Has"];
    private const string ReadMethodSuffix = "Exists";

    public static IHttpClientBuilder AddDownstreamGrpcClient<TClient>(
        this IServiceCollection services,
        string address,
        ProtoServiceDescriptor service)
        where TClient : class
        => services
            .AddGrpcClient<TClient>(options => options.Address = new Uri(address))
            .ConfigureChannel(options => options.ServiceConfig = RetryReadsWhenUnavailable(service))
            .AddInterceptor<CorrelationIdClientInterceptor>()
            .AddInterceptor<IdentityForwardingInterceptor>()
            .AddInterceptor<DefaultDeadlineInterceptor>();

    private static ServiceConfig RetryReadsWhenUnavailable(ProtoServiceDescriptor service)
    {
        var readMethods = new MethodConfig
        {
            RetryPolicy = new RetryPolicy
            {
                MaxAttempts = 3,
                InitialBackoff = TimeSpan.FromMilliseconds(500),
                MaxBackoff = TimeSpan.FromSeconds(2),
                BackoffMultiplier = 2,
                RetryableStatusCodes = { StatusCode.Unavailable }
            }
        };

        foreach (var method in service.Methods.Where(IsReadMethod))
            readMethods.Names.Add(new MethodName { Service = service.FullName, Method = method.Name });

        return readMethods.Names.Count == 0
            ? new ServiceConfig()
            : new ServiceConfig { MethodConfigs = { readMethods } };
    }

    private static bool IsReadMethod(MethodDescriptor method)
        => ReadMethodPrefixes.Any(prefix => method.Name.StartsWith(prefix, StringComparison.Ordinal))
           || method.Name.EndsWith(ReadMethodSuffix, StringComparison.Ordinal);
}
