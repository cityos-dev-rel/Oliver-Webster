namespace ODrive.Models
{
    public class UploadedFile
    {
        public string FileId { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public DateTime Created_At { get; set; }

        // Nullable to enable returning file in an array without the data.
        public byte[]? Data { get; set; }
    }
}
