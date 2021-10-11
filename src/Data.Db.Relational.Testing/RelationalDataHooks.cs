using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RecShark.Extensions.DependencyInjection;
using RecShark.Extensions.DependencyInjection.Testing;
using StackExchange.Profiling;
using StackExchange.Profiling.Internal;

namespace RecShark.Data.Db.Relational.Testing
{
    public class RelationalDataHooks : Hooks
    {
        private readonly DbConnectionFactoryTesting testingFactory;

        protected RelationalDataHooks(params DIModule[] modules) : base(modules)
        {
            var factory = this.Provider.GetService<IDbConnectionFactory>();
            this.testingFactory = new DbConnectionFactoryTesting(factory);

            this.Services.Reset(ServiceDescriptor.Singleton<IDbConnectionFactory>(this.testingFactory));

            InitMiniProfiler();
        }

        public override void Dispose()
        {
            DisposeMiniProfiler();
            this.testingFactory.Dispose();
            base.Dispose();
        }

        private static void InitMiniProfiler()
        {
            if (!Debugger.IsAttached)
                return;

            MiniProfiler.DefaultOptions.ProfilerProvider = new SingletonProfilerProvider();
            MiniProfiler.StartNew("database integration test");
        }

        private static void DisposeMiniProfiler()
        {
            var profiler = MiniProfiler.Current;
            if (profiler == null)
                return;

            var json         = MiniProfiler.Current.ToJson();
            var indentedJson = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(json), Formatting.Indented);
            Console.WriteLine(indentedJson);

            MiniProfiler.Current.Stop();
        }
    }
}