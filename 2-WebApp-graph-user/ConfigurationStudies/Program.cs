using Microsoft.Extensions.DependencyInjection;
using System;

namespace ConfigurationStudies
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ClassA>();
        }
    }

    class ClassA
    {
        public string MyName { get; set; }
    }
}
