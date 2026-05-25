using System.Collections.Generic;

namespace Had360back.Common;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMeta Pagination { get; set; } = new();
}
