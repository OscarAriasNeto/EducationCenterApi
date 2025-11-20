using System.Collections.Generic;

namespace EducationCenter.DTOs;

public class Resource<T>
{
    public T Data { get; set; } = default!;
    public IEnumerable<LinkDto> Links { get; set; } = new List<LinkDto>();
}
