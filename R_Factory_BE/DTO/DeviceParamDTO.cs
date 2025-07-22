using R_Factory_BE.Models;

namespace R_Factory_BE.DTO
{
    public class DeviceParamDTO : DeviceParameters
    {
        public string CommunicationName { get; set; }
        public DeviceCommunicationParamConfig[]? ConfigValues { get; set; }
    }
}
