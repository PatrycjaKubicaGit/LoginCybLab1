using LoginCybLab1.Models;
using LoginCybLab1.Data;
using Microsoft.EntityFrameworkCore;

public interface IUserActivityService
{
    Task<List<UserActivityLog>> GetAllLogsAsync();  
    Task LogActivity(string userName, string action, string description);

    Task LogLogin(string username);
    Task LogLogout(string username);
}

public class UserActivityService : IUserActivityService
{
    private readonly CybDbContext _context;

    public UserActivityService(CybDbContext context)
    {
        _context = context;
    }
    public async Task<List<UserActivityLog>> GetAllLogsAsync()
    {
        return await _context.UserActivityLogs
                         .OrderByDescending(log => log.ActionTime)
                         .ToListAsync();
    }

    // Metoda rejestrująca logowanie użytkownika
    public async Task LogLogin(string username)
    {
        var log = new UserActivityLog
        {
            UserName = username,
            Action = "Login",
            Description = $"{username} zalogował się.",
            ActionTime = DateTime.UtcNow
        };

        _context.UserActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // Metoda rejestrująca wylogowanie użytkownika
    public async Task LogLogout(string username)
    {
        var log = new UserActivityLog
        {
            UserName = username,
            Action = "Logout",
            Description = $"{username} Wylogował się.",
            ActionTime = DateTime.UtcNow
        };

        _context.UserActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    // Metoda rejestrująca operacje CRUD na uŻytkownikach
    public async Task LogActivity(string userName, string action, string description)
    {

        var log = new UserActivityLog
        {
            UserName = userName,
            ActionTime = DateTime.UtcNow,
            Action = action,
            Description = description
        };

        _context.UserActivityLogs.Add(log);
        await _context.SaveChangesAsync();
    }

}
