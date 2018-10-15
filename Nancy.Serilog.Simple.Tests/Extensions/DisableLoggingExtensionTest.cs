using Nancy.Serilog.Simple.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Nancy.Serilog.Simple.Tests.Extensions
{
    /// <summary>
    /// Disable logging test
    /// </summary>
    public class DisableLoggingExtensionTest
    {
        [Fact]
        public static void DisableLogging_Should_Not_Have_Effect_When_Module_Is_Null()
        {
            // arrange
            MyModule module = null;

            // act
            module.DisableLogging();

            // assert
            Assert.Null(module);
        }

        [Fact]
        public static void DisableLogging_Should_Not_Have_Effect_When_Module_Context_Is_Null()
        {
            // arrange
            MyModule module = new MyModule();
            module.Context = null;

            // act
            module.DisableLogging();

            // assert
            Assert.Null(module.Context);
        }

        [Fact]
        public static void DisableLogging_Should_Not_Have_Effect_When_Module_Context_Items_Is_Null()
        {
            // arrange
            MyModule module = new MyModule();
            module.Context = new NancyContext();

            // act
            module.DisableLogging();

            // assert
            Assert.NotNull(module.Context.Items["DisableLogging"]);
        }
    }

    public class MyModule : NancyModule
    {
    }
}
