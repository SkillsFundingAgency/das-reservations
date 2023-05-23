using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.Kernel;
using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SFA.DAS.Reservations.Web.UnitTests.Customisations
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArrangeAuthorizationFilterContextAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.ParameterType != typeof(AuthorizationFilterContext))
            {
                throw new ArgumentException(nameof(parameter));
            }

            return new ArrangeActionContextCustomisation();
        }
    }
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArrangeDefaultHttpContextFilterContextAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.ParameterType != typeof(DefaultHttpContext))
            {
                throw new ArgumentException(nameof(parameter));
            }

            return new ArrangeDefaultHttpContextCustomisation();
        }
    }

    public class ArrangeAuthorizationFilterContextCustomisation : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new ActionExecutingContextBuilder());

            fixture.Customize<Microsoft.AspNetCore.Mvc.ModelBinding.BindingInfo>(c => c.OmitAutoProperties());
            fixture.Customize<ActionExecutingContext>(composer => composer
                .Without(context => context.Result));

            fixture.Behaviors.Add(new TracingBehavior());
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class ArrangeActionContextAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.ParameterType != typeof(ActionExecutingContext))
            {
                throw new ArgumentException(nameof(parameter));
            }

            return new ArrangeActionContextCustomisation();
        }
    }

    public class ArrangeActionContextCustomisation : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Add(new ActionExecutingContextBuilder());

            fixture.Customize<Microsoft.AspNetCore.Mvc.ModelBinding.BindingInfo>(c => c.OmitAutoProperties());
            fixture.Customize<ActionExecutingContext>(composer => composer
                .Without(context => context.Result));

            fixture.Behaviors.Add(new TracingBehavior());
        }
    }
    public class ArrangeDefaultHttpContextCustomisation: ICustomization
    {
        public void Customize(IFixture fixture)
        {
            //fixture.Customizations.Add(new DefaultHttpContextBuilder());
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            fixture.Customize<Microsoft.AspNetCore.Mvc.ModelBinding.BindingInfo>(c => c.OmitAutoProperties());
            
            // fixture.Customize<DefaultHttpContext>(composer => composer
            //     .Without(context => context.));

            fixture.Behaviors.Add(new TracingBehavior());
        }
    }

    public class ActionExecutingContextBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is ParameterInfo paramInfo
                && paramInfo.ParameterType == typeof(object)
                && paramInfo.Name == "controller")
            {
                return context.Create<Controller>();
            }

            return new NoSpecimen();
        }
    }

    public class DefaultHttpContextBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            return context.Create<DefaultHttpContext>();
        }
    }
}