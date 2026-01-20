using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyOnion.Application.Behaviours;
using MyOnion.Application.Events;
using MyOnion.Application.Helpers;
using MyOnion.Application.Interfaces;
using MyOnion.Application.Messaging;
using MyOnion.Domain.Entities;
using Scrutor;

namespace MyOnion.Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            var mapsterConfig = new TypeAdapterConfig();
            mapsterConfig.Scan(Assembly.GetExecutingAssembly());
            services.AddSingleton(mapsterConfig);
            services.AddScoped<IMapper, ServiceMapper>();
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IMediator, Mediator>();
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IRequestHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped<IDataShapeHelper<Position>, DataShapeHelper<Position>>();
            services.AddScoped<IDataShapeHelper<Employee>, DataShapeHelper<Employee>>();
            services.AddScoped<IDataShapeHelper<Department>, DataShapeHelper<Department>>();
            services.AddScoped<IDataShapeHelper<SalaryRange>, DataShapeHelper<SalaryRange>>();
            services.AddScoped<IModelHelper, ModelHelper>();
            services.AddSingleton<IEventDispatcher, EventDispatcher>();
            services.AddTransient<IPipelineBehavior<GetEmployeesQuery, PagedResult<IEnumerable<Entity>>>, GetEmployeesCachingDecorator>();
            services.AddTransient<IPipelineBehavior<GetPositionsQuery, PagedResult<IEnumerable<Entity>>>, GetPositionsCachingBehavior>();
            // Register IDataShapeHelper implementations without relying on deprecated API surface.
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IDataShapeHelper<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IDomainEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
        }
    }
}
