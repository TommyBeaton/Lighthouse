namespace Kurrent.Models.Data.API;

public class DockerListTagsResponse
{
    public long Count { get; set; }
    public Uri Next { get; set; }
    public object Previous { get; set; }
    public Result[] Results { get; set; }
}

public class Result
{
    public long Creator { get; set; }
    public long Id { get; set; }
    public DockerImage[] Images { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public long LastUpdater { get; set; }
    public string LastUpdaterUsername { get; set; }
    public string Name { get; set; }
    public long Repository { get; set; }
    public long FullSize { get; set; }
    public bool V2 { get; set; }
    public string TagStatus { get; set; }
    public DateTimeOffset TagLastPulled { get; set; }
    public DateTimeOffset TagLastPushed { get; set; }
    public string MediaType { get; set; }
    public string ContentType { get; set; }
    public string Digest { get; set; }
}

public class DockerImage
{
    public string Architecture { get; set; }
    public string Features { get; set; }
    public string? Variant { get; set; }
    public string Digest { get; set; }
    public string OS { get; set; }
    public string OsFeatures { get; set; }
    public object OsVersion { get; set; }
    public long Size { get; set; }
    public string Status { get; set; }
    public DateTimeOffset LastPulled { get; set; }
    public DateTimeOffset LastPushed { get; set; }
}