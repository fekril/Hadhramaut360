namespace Had360back.Common;

public class PaginationMeta
{
    public int CurrentPage { get; set; }
    public int? NextPage { get; set; }
    public int PerPage { get; set; }
    public int? PreviousPage { get; set; }
    public int TotalEntries { get; set; }
    public int TotalPages { get; set; }
}
