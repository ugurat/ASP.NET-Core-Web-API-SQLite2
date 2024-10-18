
// Datei: /Interfaces/IBenutzerService.cs

using Bsp01.Controllers;
using Bsp01.Data;
using Microsoft.AspNetCore.Mvc;

namespace Bsp01.Interfaces
{
    public interface IBenutzerService
    {

        // CRUD Methoden
        Task<IEnumerable<Benutzer>> Get();
        Task<Benutzer?> Get(long id);
        Task<long> Create(Benutzer benutzer);
        Task Update(Benutzer benutzer);
        Task Delete(long id);

        // REGISTRIEREN
        Task<long> Register(RegisterRequest registerRequest);
        Task Change(ChangeRequest changeRequest);


        // Weitere Methoden
        Task<int> Count();
        Task<Benutzer> GetByEmail(string email);
        Task<bool> ExistsById(long id);
        Task<bool> ExistsByEmail(string email);

        // Passwort-Handling
        string GenerateSalt();
        string HashPassword(string password, string salt);
        bool VerifyPassword(string password, string salt, string hashedPassword);
        string GenerateJwtToken(Benutzer benutzer);

    }

}
