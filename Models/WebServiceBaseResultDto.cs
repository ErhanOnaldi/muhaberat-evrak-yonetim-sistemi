namespace muhaberat_evrak_yonetim.Models
{
    public class WebServiceBaseResultDto
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
        public int? MessageCode { get; set; }
        public int? MessageDetailCode { get; set; }
    }
}
