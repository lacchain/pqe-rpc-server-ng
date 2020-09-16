using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IBCQC_NetCore.Functions
{
    public class JwtTokenHandler
    {


		//generate a JWT token
		public string GenerateToken(string certSerialNumber,string expiry,string secret,string issuer,string audience)
		{

			
			
			var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
							

			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(new Claim[]
				{
			new Claim(ClaimTypes.SerialNumber,certSerialNumber),
				}),
				Expires = DateTime.UtcNow.AddHours(Convert.ToInt16(expiry)),
				Issuer = issuer,
				Audience = audience,
				SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			return tokenHandler.WriteToken(token);
		}


		//Validate the JWT token


		public bool ValidateCurrentToken(string token)
		{

			var tokenExpiry = Startup.StaticConfig["Config:jwt_lifetime"];
			var tokenSecret = Startup.StaticConfig["Config:jwt_secret"];
			var tokenIssuer = Startup.StaticConfig["Config:jwt_issuer"];
			var tokenAudience = Startup.StaticConfig["Config:jwt_audience"];

			
			var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenSecret));

		//TODO: Check expired tokens are not validated

			var tokenHandler = new JwtSecurityTokenHandler();
			try
			{
				tokenHandler.ValidateToken(token, new TokenValidationParameters
				{
					ValidateLifetime = true,
					ValidateTokenReplay = true,
					ValidateIssuerSigningKey = true,
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidIssuer = tokenIssuer,
					ValidAudience = tokenAudience,
					IssuerSigningKey = mySecurityKey
				}, out SecurityToken validatedToken);
			}
			catch
			{
				return false;
			}
			return true;
		}


	}

    
}
