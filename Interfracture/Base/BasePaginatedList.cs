namespace Interfracture.Base
{
    public class BasePaginatedList<T>
    {
        public List<T> Items { get; private set; } = new(); // Initialize to prevent null issues

        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }

        // Parameterless constructor for deserialization
        public BasePaginatedList() { }

        public BasePaginatedList(List<T> items, int count, int pageNumber, int pageSize)
        {
            TotalItems = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (pageSize > 0) ? (int)Math.Ceiling(count / (double)pageSize) : (count > 0 ? 1 : 0);
            Items = items;
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        public static BasePaginatedList<T> Create(IEnumerable<T> source, int pageNumber, int pageSize)
        {
            var count = source.Count();
            var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return new BasePaginatedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
