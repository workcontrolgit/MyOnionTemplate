using System.Reflection;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyOnion.Application.Behaviours;
using MyOnion.Application.Helpers;
using MyOnion.Application.Interfaces;
using MyOnion.Domain.Entities;
using Scrutor;

namespace MyOnion.Application
{
    public static class ServiceExtensions
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddScoped<IDataShapeHelper<Position>, DataShapeHelper<Position>>();
            services.AddScoped<IDataShapeHelper<Employee>, DataShapeHelper<Employee>>();
            services.AddScoped<IDataShapeHelper<Department>, DataShapeHelper<Department>>();
            services.AddScoped<IDataShapeHelper<SalaryRange>, DataShapeHelper<SalaryRange>>();
            services.AddScoped<IModelHelper, ModelHelper>();
            // Register IDataShapeHelper implementations without relying on deprecated API surface.
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IDataShapeHelper<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
            services.Scan(selector => selector
                .FromAssemblies(Assembly.GetExecutingAssembly())
                .AddClasses(classSelector => classSelector.AssignableTo(typeof(IPipelineBehavior<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
        }
    }
}
