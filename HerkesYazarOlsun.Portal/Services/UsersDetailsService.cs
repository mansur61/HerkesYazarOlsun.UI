
using HerkesYazarOlsun.Model.Entity;
using HerkesYazarOlsun.Model.ViewModel;
using Newtonsoft.Json;

namespace HerkesYazarOlsun.Portal.Services
{

    public class UsersDetailsService : BaseService
    {


        public UsersDetails GetUsersDetailsByLoginId(long loginId)
        {
            Task<string> jsonContent = GetData("api/UsersDetails/GetUsersDetailsByLoginId?" + "loginId=" + loginId);
            Task.WaitAll(jsonContent);

            var result = JsonConvert.DeserializeObject<UsersDetails>(jsonContent.Result);
            return result;
        }

    }
}
