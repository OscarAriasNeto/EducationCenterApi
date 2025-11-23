using System;

namespace EducationCenter.Models;

public class Audit
{
    public long Id { get; set; }
    public string? TableName { get; set; }
    public string? Operation { get; set; }
    public string? PkValue { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public DateTime? DoneAt { get; set; }
    public string? DoneBy { get; set; }
}
