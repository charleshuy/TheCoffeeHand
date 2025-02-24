namespace Services.Interfaces.Interfaces
{
    public interface IFirebaseAuthService
    {
        Task<string> SignInWithFirebaseAsync(string idToken, string? fcmToken);
    }
}
