using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Palantir.Homatic;
using System;
using System.Threading.Tasks;

namespace Palantir.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ConnectorService connectorService;
        private readonly ILogger<DeviceController> logger;

        public DeviceController(ConnectorService connectorService, ILogger<DeviceController> logger)
        {
            this.connectorService = connectorService ?? throw new ArgumentNullException(nameof(connectorService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<DeviceStates> GetAsync()
            => await this.connectorService.RequestAsync<DeviceStates>(new GetDeviceStates())
                                          .ConfigureAwait(false);
    }
}
