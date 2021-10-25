namespace AuthenticationService.Business.BusinessObjects.Responses
{
    public class LoginResponse
    {
        public string DsToken { get; set; }
        public int IdUser { get; set; }
        public string DsUser { get; set; } 
        public string DsMessage { get; set; }
        public bool FlLoginSuccess { get; set; }
        public LoginResponse()
        {
            DsUser = string.Empty;
            DsToken = string.Empty;
        }
    }
}
