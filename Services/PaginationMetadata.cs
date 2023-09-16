namespace SmartTasksAPI.Services
{
    public class PaginationMetadata
    {
        public int TotatItemCount { get; set; }
        public int TotalPageCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }

        public PaginationMetadata(int totatItemCount, int pageSize, int currentPage)
        {
            TotatItemCount = totatItemCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPageCount = (int)Math.Ceiling((double)totatItemCount/pageSize);
        }
    }
}
