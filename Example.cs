using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using System;
using System.Diagnostics;
using System.Linq;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var calc = ProxyFactory.CreateNew<Calc>();
            calc.Sum(1, 1);

            Console.ReadLine();
        }
    }

    public class Logger : InterceptorAttribute, IInterceptor
    {
        public Logger() : base(typeof(Logger))
        { }

        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine("Executing method: {0} {1}", invocation.TargetType.FullName, invocation.Method);

            if (invocation.Arguments.Any())
                Console.WriteLine("Arguments: {0}", string.Join(", ", invocation.Arguments));

            var sw = Stopwatch.StartNew();

            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: {0}", ex.ToString());
                throw;
            }
            finally
            {
                Console.WriteLine("Elapsed Time: {0} ms", sw.ElapsedMilliseconds);
            }
        }
    }

    public abstract class ProxyFactory
    {
        static WindsorContainer _container = new WindsorContainer();

        static ProxyFactory()
        {
            _container.Register(Component.For<Logger>());
        }

        public static TType CreateNew<TType>()
           where TType : class
        {
            if (!_container.Kernel.HasComponent(typeof(TType)))
                _container.Register(Component.For(typeof(TType)));

            return _container.Resolve<TType>();
        }
    }

    [Logger]
    public class Calc
    {
        public virtual void Sum(int a, int b)
        {
            Console.WriteLine("{0} + {1} = {2}", a, b, a + b);
        }
    }
}
