using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces.Interfaces
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken);
    }
}
