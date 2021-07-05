﻿using Microsoft.Extensions.Logging;
using Proto;
using Proto.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Palantir
{
    public class HomaticRoot : IActor
    {
        private readonly ILogger<HomaticRoot> logger;
        private PID deviceController;

        public HomaticRoot(ILogger<HomaticRoot> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.logger.LogInformation("homematic root created");
        }

        public Task ReceiveAsync(IContext context)
            => context.Message switch
            {
                Started => this.OnStarted(context),
                DeviceData => this.ForwardToDevceController(context),
                GetDeviceStates => this.ForwardToDevceController(context),
                _ => Task.CompletedTask
            };

        private Task OnStarted(IContext context)
        {
            this.logger.LogInformation("started");
            var props = context.System.DI().PropsFor<DeviceController>();
            this.deviceController = context.Spawn(props);
            this.logger.LogInformation("device contoller spawned");

            return Task.CompletedTask;
        }

        private Task ForwardToDevceController(IContext context)
        {
            context.Forward(this.deviceController);
            this.logger.LogInformation("forwarded {message} to device controller", context.Message);
            
            return Task.CompletedTask;
        }
    }
}