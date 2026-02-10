using LancasterCreditCardDiversion.Data;
using LancasterCreditCardDiversion.Models;
using LancasterCreditCardDiversion.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.CodeDom;
using System.Data;
using System.Text;

namespace LancasterCreditCardDiversion.Services
{
    /// <summary>
    /// Handles user functions
    /// </summary>
    public class UserService
    {
        private readonly PaLancCcdpDevDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _sessionUser;
        private readonly AuthDbContext _authcontext;
        private readonly IConfiguration _configuration;

        public UserService(PaLancCcdpDevDbContext context, IHttpContextAccessor httpContextAccessor, AuthDbContext authcontext, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _sessionUser = _httpContextAccessor.HttpContext?.Session.GetString("Username");
            _authcontext = authcontext;
            _configuration = configuration;
        }

        public async Task<bool> CreateUser(UserViewModel user)
        {

            string? schemaName = _configuration["DatabaseSettings:SchemaName"];
            string sql = $"EXEC {schemaName}.CreateUser @username = @Username, @password = @Password, @email = @Email, @fullName = @FullName, @Result = @Result OUTPUT;";
            var userExists = await _context.Users.Where(u => u.UserName == user.UserName || u.Email == user.Email).FirstOrDefaultAsync();
            if (userExists != null) return false;

            
            var resultParam = new SqlParameter("@Result", SqlDbType.VarChar, 5)
            {
                Direction = ParameterDirection.Output
            };

            try
            {
                await _authcontext.Database.ExecuteSqlRawAsync(
                    sql,
                    new SqlParameter("@Username", SqlDbType.VarChar) { Value = user.UserName },
                    new SqlParameter("@Password", SqlDbType.VarChar) { Value = user.PasswordHash },
                    new SqlParameter("@Email", SqlDbType.VarChar) { Value = user.Email },
                    new SqlParameter("@FullName", SqlDbType.VarChar) { Value = user.FullName },
                    resultParam
                );

                return resultParam.Value?.ToString() == "TRUE";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void UpdateUser(User user)
        {
            user.ModifiedUser = _httpContextAccessor.HttpContext?.Session.GetString("Username") ?? "System";
            user.ModifiedDttm = DateTime.UtcNow;

            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public async Task<List<UserViewModel>> ListUsersAsync()
        {
            var getUsers = await _context.Users.ToListAsync();
        

            return getUsers.Select(u => new UserViewModel
            {
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                FullName = u.FullName,
                CreatedUser = u.CreatedUser ?? "",
                CreatedDttm = u.CreatedDttm ?? DateTime.UtcNow,
                ModifiedUser = u.ModifiedUser,
                ModifiedDttm = u.ModifiedDttm,
                RecordStatus = u.RecordStatus == "A" ? "Active" : "Deleted",
            }).OrderByDescending(c => c.CreatedDttm).ToList();
        }

        public async Task<UserViewModel> GetUserDetails(string username)
        {
            var userDetails = await _context.Users.Where(u => u.UserName == username).FirstOrDefaultAsync();
            return new UserViewModel
            {
                Email = userDetails!.Email?? "",
                UserName = userDetails.UserName ?? "",
                FullName = userDetails.FullName,
                CreatedUser = userDetails.CreatedUser ?? "",
                CreatedDttm = userDetails.CreatedDttm ?? DateTime.UtcNow,
                ModifiedUser = userDetails.ModifiedUser ?? "",
                ModifiedDttm = userDetails.ModifiedDttm,
                RecordStatus = userDetails.RecordStatus ?? "",
            };
        }

        public async Task<bool> DeleteUserAsync(string username)
        {
            var existingUser = await _context.Users.FindAsync(username);
            if (existingUser == null) return false;

            existingUser.RecordStatus = "D";
            existingUser.ModifiedUser = _sessionUser ?? "Unknown";
            await _context.SaveChangesAsync();
            return true;
        }


    }
}