﻿using HydroPiApi.Controllers.Common.Processor;

namespace HydroPiApi.Controllers.Sensors.Version1.Processors.Request
{
    public class GetSensorReadingByGpioProcessorRequestVersionOne : IProcessorRequest
    {
        public int GpioPin { get; set; }
    }
}