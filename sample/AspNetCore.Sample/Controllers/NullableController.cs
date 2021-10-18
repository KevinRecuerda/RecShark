﻿using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecShark.AspNetCore.Extensions;

namespace RecShark.AspNetCore.Sample.Controllers
{
    [ApiController]
    [ApiVersion("2")]
    [DefaultRoute]
    [SwaggerTagBadgeDefault.Tech]
    public class NullableController : ControllerBase
    {
        /// <summary> object containing nullable properties </summary>
        [HttpPost]
        public Issue NullableProperties(Issue issue)
        {
            return issue;
        }
    }

    public class Issue
    {
        [JsonIgnore] public long Unused { get; set; }

        public long Id { get; set; }
        public string Name { get; set; }

        public long AssigneeId { get; set; }
        public long? AssigneeIdNullable { get; set; }

        public DateTime CloseDateId { get; set; }
        public DateTime? CloseDateNullable { get; set; }
    }
}