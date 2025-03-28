namespace Services.ServiceInterfaces
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken);
        Task<string> SignInWithEmailAndPasswordAsync(string email, string password);
    }
}
