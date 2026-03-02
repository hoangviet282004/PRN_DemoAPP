namespace Service.Dtos.Response
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public List<string> Errors { get; set; }

        public ErrorResponse(int statusCode, string message, object data = null, List<string> errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors ?? new List<string>();
        }
    }
}