using Microsoft.Extensions.DependencyInjection;

namespace MyOnion.Application.Messaging
{
    public sealed class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var requestType = request.GetType();
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = _serviceProvider.GetRequiredService(handlerType);
            var behaviorsType = typeof(IEnumerable<>).MakeGenericType(
                typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)));
            var behaviors = (IEnumerable<object>?)_serviceProvider.GetService(behaviorsType) ?? Array.Empty<object>();

            RequestHandlerDelegate<TResponse> next = () => ((dynamic)handler).Handle((dynamic)request, cancellationToken);

            foreach (var behavior in behaviors.Reverse())
            {
                var current = next;
                next = () => ((dynamic)behavior).Handle((dynamic)request, current, cancellationToken);
            }

            return next();
        }
    }
}
