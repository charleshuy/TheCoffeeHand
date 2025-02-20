

namespace Interfracture.DTOs
{
    public class CategoryResponseDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryRequestDTO
    {
        public string Name { get; set; } = string.Empty;
    }
}
