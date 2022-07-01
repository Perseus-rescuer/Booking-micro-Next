﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using BuildingBlocks.Core.Model;
using BuildingBlocks.MassTransit;
using BuildingBlocks.Mongo;
using BuildingBlocks.Web;
using Grpc.Net.Client;
using Identity.Data;
using MassTransit;
using MassTransit.Testing;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mongo2Go;
using NSubstitute;
using Respawn;
using Serilog;
using Xunit;
using Xunit.Abstractions;

namespace Integration.Test;

[CollectionDefinition(nameof(IntegrationTestFixture))]
public class FixtureCollection : ICollectionFixture<IntegrationTestFixture>
{
}

public class IntegrationTestFixture : IAsyncLifetime
{
    private Checkpoint _checkpoint;
    private IConfiguration _configuration;
    private WebApplicationFactory<Program> _factory;
    private MongoDbRunner _mongoRunner;
    private IServiceProvider _serviceProvider;
    private Action<IServiceCollection>? _testRegistrationServices;
    public HttpClient HttpClient => _factory.CreateClient();
    public ITestHarness TestHarness => CreateHarness();
    public GrpcChannel Channel => CreateChannel();


    public virtual Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("test");
                builder.ConfigureServices(services =>
                {
                    _testRegistrationServices?.Invoke(services);
                });
            });

        RegisterServices(services =>
        {
            services.ReplaceSingleton(AddHttpContextAccessorMock);
            services.AddMassTransitTestHarness(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqOptions = services.GetOptions<RabbitMqOptions>("RabbitMq");
                    var host = rabbitMqOptions.HostName;

                    cfg.Host(host, h =>
                    {
                        h.Username(rabbitMqOptions.UserName);
                        h.Password(rabbitMqOptions.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            });
        });

        _serviceProvider = _factory.Services;
        _configuration = _factory.Services.GetRequiredService<IConfiguration>();

        _checkpoint = new Checkpoint {TablesToIgnore = new[] {"__EFMigrationsHistory"}};

        _mongoRunner = MongoDbRunner.Start();
        var mongoOptions = _factory.Services.GetRequiredService<IOptions<MongoOptions>>();
        if (mongoOptions.Value.ConnectionString != null)
            mongoOptions.Value.ConnectionString = _mongoRunner.ConnectionString;

        return Task.CompletedTask;
    }

    public virtual async Task DisposeAsync()
    {
        if (!string.IsNullOrEmpty(_configuration?.GetConnectionString("DefaultConnection")))
            await _checkpoint.Reset(_configuration?.GetConnectionString("DefaultConnection"));

        await _factory.DisposeAsync();
        _mongoRunner.Dispose();
    }

    public void RegisterServices(Action<IServiceCollection> services)
    {
        _testRegistrationServices = services;
    }

    // ref: https://github.com/trbenning/serilog-sinks-xunit
    public ILogger CreateLogger(ITestOutputHelper output)
    {
        if (output != null)
        {
            return new LoggerConfiguration()
                .WriteTo.TestOutput(output)
                .CreateLogger();
        }

        return null;
    }

    public async Task ExecuteScopeAsync(Func<IServiceProvider, Task> action)
    {
        using var scope = _serviceProvider.CreateScope();
        await action(scope.ServiceProvider);
    }

    public async Task<T> ExecuteScopeAsync<T>(Func<IServiceProvider, Task<T>> action)
    {
        using var scope = _serviceProvider.CreateScope();

        var result = await action(scope.ServiceProvider);

        return result;
    }

    public Task ExecuteDbContextAsync(Func<IdentityContext, Task> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>()));
    }

    public Task ExecuteDbContextAsync(Func<IdentityContext, ValueTask> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>()).AsTask());
    }

    public Task ExecuteDbContextAsync(Func<IdentityContext, IMediator, Task> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>(), sp.GetService<IMediator>()));
    }

    public Task<T> ExecuteDbContextAsync<T>(Func<IdentityContext, Task<T>> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>()));
    }

    public Task<T> ExecuteDbContextAsync<T>(Func<IdentityContext, ValueTask<T>> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>()).AsTask());
    }

    public Task<T> ExecuteDbContextAsync<T>(Func<IdentityContext, IMediator, Task<T>> action)
    {
        return ExecuteScopeAsync(sp => action(sp.GetService<IdentityContext>(), sp.GetService<IMediator>()));
    }

    public Task InsertAsync<T>(params T[] entities) where T : class
    {
        return ExecuteDbContextAsync(db =>
        {
            foreach (var entity in entities) db.Set<T>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity>(TEntity entity) where TEntity : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2>(TEntity entity, TEntity2 entity2)
        where TEntity : class
        where TEntity2 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3>(TEntity entity, TEntity2 entity2, TEntity3 entity3)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);

            return db.SaveChangesAsync();
        });
    }

    public Task InsertAsync<TEntity, TEntity2, TEntity3, TEntity4>(TEntity entity, TEntity2 entity2, TEntity3 entity3,
        TEntity4 entity4)
        where TEntity : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        return ExecuteDbContextAsync(db =>
        {
            db.Set<TEntity>().Add(entity);
            db.Set<TEntity2>().Add(entity2);
            db.Set<TEntity3>().Add(entity3);
            db.Set<TEntity4>().Add(entity4);

            return db.SaveChangesAsync();
        });
    }

    public Task<T> FindAsync<T>(long id)
        where T : class, IEntity
    {
        return ExecuteDbContextAsync(db => db.Set<T>().FindAsync(id).AsTask());
    }

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return mediator.Send(request);
        });
    }

    public Task SendAsync(IRequest request)
    {
        return ExecuteScopeAsync(sp =>
        {
            var mediator = sp.GetRequiredService<IMediator>();

            return mediator.Send(request);
        });
    }

    private ITestHarness CreateHarness()
    {
        var harness = _serviceProvider.GetTestHarness();
        harness.Start().GetAwaiter().GetResult();
        return harness;
    }

    private GrpcChannel CreateChannel()
    {
        return GrpcChannel.ForAddress(HttpClient.BaseAddress!, new GrpcChannelOptions {HttpClient = HttpClient});
    }

    private IHttpContextAccessor AddHttpContextAccessorMock(IServiceProvider serviceProvider)
    {
        var httpContextAccessorMock = Substitute.For<IHttpContextAccessor>();
        using var scope = serviceProvider.CreateScope();
        httpContextAccessorMock.HttpContext = new DefaultHttpContext {RequestServices = scope.ServiceProvider};

        httpContextAccessorMock.HttpContext.Request.Host = new HostString("localhost", 6012);
        httpContextAccessorMock.HttpContext.Request.Scheme = "http";

        return httpContextAccessorMock;
    }
}
