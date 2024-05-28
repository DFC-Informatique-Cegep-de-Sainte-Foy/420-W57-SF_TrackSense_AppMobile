using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Configurations;
using TrackSense.Entities;
using TrackSense.Services.API.APIDTO;

namespace TrackSense.Services
{
    public class UserService
    {
        HttpClient httpClient;
        IConfigurationManager _config;

        public UserService(IConfigurationManager config)
        {
            httpClient = new HttpClient();
            _config = config;
        }

        public async Task<HttpResponseMessage> CreateUser(User user)
        {
            Settings userSettings = _config.LoadSettings();
            string url = $"{userSettings.ApiUrl}/Authentication/SignUp";

            UserDTO userDTO = new UserDTO(user);

            return await httpClient.PostAsJsonAsync(url, userDTO);
        }

        public async Task<String> GetCurrentUser()
        {
            Settings userSettings = _config.LoadSettings();

            return userSettings.Username;
        }

        public async Task<bool> IsUserLoginAvailable(string userLogin)
        {
            Settings userSettings = _config.LoadSettings();
            string url = $"{userSettings.ApiUrl}/Users/availibityLogin/?p_userLogin={userLogin}";

            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                AvailabilityDTO availabilityDTO = response.Content.ReadFromJsonAsync<AvailabilityDTO>().Result;
                return availabilityDTO.IsAvailable;
            }
            else
            {
                return false;
            }
        }

    }
}
