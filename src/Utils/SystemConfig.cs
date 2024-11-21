using System.ComponentModel.DataAnnotations;

namespace Kurrent.Utils;

public class SystemConfig
{
    [Required]
    public string DockerHubUri { get; set; }
}