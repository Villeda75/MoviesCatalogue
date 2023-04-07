namespace MoviesCatalogue.Classes.Wrappers
{
    public class PaginatedResponse<T> : Response<T>
    {
            public PaginatedResponse(T data, int pageNumber, int pageSize, int totalPages, int totalRecords)
            {

                PageNumber = pageNumber;
                PageSize = pageSize;
                TotalPages = totalPages;
                TotalRecords = totalRecords;
                Success = true;
                Status = 200;
                Message = "";
                Data = data;
                Error = "";

            }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
