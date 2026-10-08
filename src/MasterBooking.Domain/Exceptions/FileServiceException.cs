namespace MasterBooking.Domain.Exceptions
{
    public class FileServiceException : System.Exception
    {
        public FileServiceException(string message) : base(message)
        {
        }
    }
}