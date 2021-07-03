using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;

namespace Palantir.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ActorSystem actorSystem;
        private readonly Worker worker;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(ActorSystem actorSystem, Worker worker, ILogger<DeviceController> logger)
        {
            this.actorSystem = actorSystem ?? throw new ArgumentNullException(nameof(actorSystem));
            this.worker = worker ?? throw new ArgumentNullException(nameof(worker));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<DeviceStates> GetAsync()
        {
            return await this.actorSystem.Root.RequestAsync<DeviceStates>(this.worker.homaticRoot, new GetDeviceStates());
        }
    }
}
