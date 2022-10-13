using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace EventPS
{
    class Program
    {
        public static IServiceProvider ServiceProvider;
        private static IEventPublisher _eventPublisher;
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddSingleton<IEventPublisher, EventPublisher>();

            var consumerType = typeof(IConsumer<>);
            var consumers = FindClassesOfType(consumerType, new List<Assembly> { Assembly.GetExecutingAssembly() });

            foreach (var consumer in consumers)
            {
                var interfaces = consumer.FindInterfaces((type, criteria) =>
                {
                    return type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                }, consumerType);
                foreach (var consumerInterface in interfaces)
                    services.AddScoped(consumerInterface, consumer);
            }

            ServiceProvider = services.BuildServiceProvider();

            _eventPublisher = ServiceProvider.GetService<IEventPublisher>();
            _eventPublisher.PublishAsync(new SampleEvent { Arg1 = "first message" });

            var oldDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            DateTimeConsumer.DateTime = oldDateTime;

            var newDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(5));
            _eventPublisher.PublishAsync(newDateTime);

            if (newDateTime == DateTimeConsumer.DateTime)
                Console.WriteLine("It works..");
            Console.Read();
        }

        static IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }
                    catch
                    {
                    }

                    if (types == null)
                        continue;

                    foreach (var t in types)
                    {
                        if (!assignTypeFrom.IsAssignableFrom(t) && (!assignTypeFrom.IsGenericTypeDefinition || !DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                            continue;

                        if (t.IsInterface)
                            continue;

                        if (onlyConcreteClasses)
                        {
                            if (t.IsClass && !t.IsAbstract)
                            {
                                result.Add(t);
                            }
                        }
                        else
                        {
                            result.Add(t);
                        }
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                Debug.WriteLine(fail.Message, fail);

                throw fail;
            }

            return result;
        }

        static bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.IsGenericType)
                        continue;

                    if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                        return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
