namespace BlazorPortal.Models
{
    public class TcpaModel
    {
        public ResultModel results { get; set; } = new ResultModel();
    }

    public class ResultModel
    {
        public string status { get; set; } = string.Empty;
    }
}
