﻿using System;

namespace ThingsGateway.Web.Page
{
    public class HisPageInput : VariablePageInput
    {
        public DateTime? StartTime { get; set; } = DateTime.Now.AddDays(-1);

        public DateTime? EndTime { get; set; } = DateTime.Now.AddDays(1);

    }

}