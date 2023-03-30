using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace WebApplicationCalendar.Models
{
    public interface ITokenService
    {
        MyToken RefreshToken();
  
    }
}
