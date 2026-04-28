using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentalServer.Data;
using RentalServer.DTO;
using RentalServer.Models;

namespace RentalServer.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app, IConfiguration config)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (registerRequest request, RentalDbContext db) =>
        {
            if (await db.Users.AnyAsync(U => U.Email == request.Email))
            {
                return Results.BadRequest(new { Message = "User with that Email already exists." });
            }
            
            if (await db.Users.AnyAsync(U => U.Username == request.UserName))
            {
                return Results.BadRequest(new { Message = "User with that Username already exists." });
            }
            
            if (request.Password.Length < 8 || !request.Password.Any(char.IsDigit))
            {
                return Results.BadRequest(new { Message = "Password must contain at least 8 characters." });
            }
            
            
            
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Email = request.Email,
                PasswordHash = hashedPassword,
                Phone  = request.Phone,
                Username = request.UserName
            };
            
            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return Results.Ok(newUser);
        });

        group.MapPost("/login", async (AuthRequest request, RentalDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => 
                u.Username == request.Login || 
                u.Email == request.Login || 
                u.Phone == request.Login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            string secretKey = config["Jwt:Key"] ?? "MySuperSecretKeyForDevelopmentOnly123!";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: credentials);

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new AuthResponse
            {
                Token = jwt,
                UserId = user.UserId,
                UserName = user.Username,
                Email = user.Email,
                phone = user.Phone,
            });
        });
        
        // Используем PATCH для частичного обновления (Independent Updates)
        group.MapPatch("/profile", async (UpdateProfileRequest request, RentalDbContext db, ClaimsPrincipal user) =>
        {

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Results.Unauthorized();
    
            var userId = Guid.Parse(userIdClaim.Value);
    
            // 2. Ищем пользователя в БД
            var dbUser = await db.Users.FindAsync(userId);
            if (dbUser == null) return Results.NotFound(new { Message = "Пользователь не найден" });


            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                dbUser.Phone = request.Phone;
            }

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                dbUser.Username = request.UserName;
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                dbUser.Email = request.Email;
            }

            // 4. Сохраняем изменения
            await db.SaveChangesAsync();
    
            return Results.Ok(new 
            { 
                Message = "Profile updated",
                UpdatedFields = new { 
                    Phone = request.Phone != null ? "Updated" : "Kept original",
                    FullName = request.UserName != null ? "Updated" : "Kept original",
                    Email = request.Email != null ? "Updated" : "Kept original",
                }
            });
        }).RequireAuthorization();
        
    }
}