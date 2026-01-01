using System.Collections.Generic;

namespace CleanArchitecture.Core.Wrappers
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public List<T> Data { get; set; }

        public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords)
        {
            TotalRecords = totalRecords;
            PageNumber = pageNumber;
            PageSize = pageSize;
            Data = data;
        }
    }
}
