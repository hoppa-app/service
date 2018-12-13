using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Expressions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Extensions;


namespace hoppa.Service.Controllers
{
    [Route("api/v1.0/openapi")]
    public class OpenApiController : ControllerBase
    {
        public IActionResult Get()
        {
            var document = new OpenApiDocument
            {
                Info = new OpenApiInfo
                {
                    Version = "1.0.0",
                    Title = "hoppa",
                    License = new OpenApiLicense
                    {
                        Name = "MIT"
                    },
                    Contact = new OpenApiContact
                    {
                        Name = "Nick Duijvelshoff",
                        Email = "online@hoppa.app"
                    }
                },
                Servers = new List<OpenApiServer>
                {
                    new OpenApiServer { Url = "https://service.dev.hoppa.app/api/v1.0/" }
                },
                Tags = new List<OpenApiTag>
                {
                    new OpenApiTag {
                        Name = "People",
                        Description = "People operations"
                    }
                },
                Paths = new OpenApiPaths
                {
                    ["/people"] = new OpenApiPathItem
                    {
                        Operations = new Dictionary<OperationType, OpenApiOperation>
                        {
                            [OperationType.Get] = new OpenApiOperation
                            {
                                Tags = new List<OpenApiTag>{
                                    new OpenApiTag {
                                        Name = "People"
                                    }
                                },
                                Description = "Returns all people from the system that the user has access to.",
                                Responses = new OpenApiResponses
                                {
                                    ["200"] = new OpenApiResponse
                                    {
                                        Description = "OK"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return Ok(document.Serialize(OpenApiSpecVersion.OpenApi3_0, OpenApiFormat.Json));
        }
    }
}
        