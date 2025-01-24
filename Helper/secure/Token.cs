using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using dbContex.Models;
using Microsoft.Identity.Client;

namespace Helper.secure
{
    public class Token
    {
        private readonly IConfiguration _configuration;
        private readonly HubtelWalletDbContextExtended _context;

        public Token(IConfiguration configuration, HubtelWalletDbContextExtended dbContext)
        {
            _configuration = configuration;
            _context = dbContext;
        }

        public async Task<Dictionary<string, string>> GenerateJwtToken(TUserAccess user, Guid userTypeId, string userDetailsId, string registrationId)
        {
            var tokens = new Dictionary<string, string>();

            try
            {
                var jwtKey = _configuration["Jwt:Key"];
                var keyBytesFromJson = _configuration["EncryptionKeys:keyBytes"];
                var ivBytesFromJson = _configuration["EncryptionKeys:ivBytes"];

                if (string.IsNullOrWhiteSpace(keyBytesFromJson) || string.IsNullOrWhiteSpace(ivBytesFromJson))
                {
                    tokens.Add("accessToken", "Encryption keys not available!");
                    tokens.Add("refreshToken", "JnF Error");
                    return tokens;
                }

                byte[] keyBytes = Convert.FromBase64String(keyBytesFromJson);
                byte[] ivBytes = Convert.FromBase64String(ivBytesFromJson);

                var encryptionService = new EncryptionService(keyBytes, ivBytes);
                string decryptedSystemAccess = user.EmailPhoneNumber;

                var systemUserType = await _context.TUserTypes!.FirstOrDefaultAsync(e => e.Id.Equals(userTypeId));

                if (systemUserType != null && jwtKey != null)
                {
                    var tokenBytes = Encoding.UTF8.GetBytes(jwtKey);
                    var key = new SymmetricSecurityKey(tokenBytes);
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

                    var issuer = _configuration["Jwt:Issuer"];
                    if (issuer != null)
                    {

                        var accessToken = CreateToken(issuer, decryptedSystemAccess, systemUserType.Name!, user.Id.ToString(), creds, DateTime.Now.AddHours(2));

                        var refreshToken = CreateToken(issuer, decryptedSystemAccess, systemUserType.Name!, user.Id.ToString(), creds, DateTime.Now.AddHours(816));

                        tokens.Add("accessToken", new JwtSecurityTokenHandler().WriteToken(accessToken));
                        tokens.Add("refreshToken", new JwtSecurityTokenHandler().WriteToken(refreshToken));

                        return tokens;
                    }
                }

                tokens.Add("accessToken", "Error generating tokens!");
                tokens.Add("refreshToken", "Error generating tokens!");
                return tokens;
            }
            catch (Exception ex)
            {
                tokens.Add("accessToken", $"Error: {ex.Message}");
                tokens.Add("refreshToken", string.Empty);
                return tokens;
            }
        }

       
        private JwtSecurityToken CreateToken(string issuer, string name, string role, string userId, SigningCredentials creds, DateTime expiration)
        {
            return new JwtSecurityToken(
                issuer: issuer,
                audience: issuer,
                claims: new[]
                {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.Expiration, expiration.ToString()),
                    new Claim(ClaimTypes.Expired, DateTime.Now.ToString()),
                    new Claim(ClaimTypes.CookiePath, userId)
                },
                expires: expiration,
                signingCredentials: creds
            );
        }

        public ClaimsPrincipal ValidateJwtToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null!;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null!;
            }
        }
    }
}