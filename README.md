### Swagger

```xml
<PackageReference Include="Swashbuckle.AspNetCore.Swagger" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerGen" />
<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUi" />

```



Startup:ConfigureServices

```csharp
services.AddSwaggerGen(c => {
             c.SwaggerDoc("v1", new OpenApiInfo { 
                                  Title = "Weather API", 
                                  Version = "v1" });
            });
```



Startup:Configure

```csharp
app.UseSwagger();
app.UseSwaggerUI(c => {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", 
                                  "Weather API v1");
            });
```



### Store passwords

Guideline NIST SP 800-63

- password salted with at least 32 bits of data
- hashed with one-way key derivation such as Password-Based Key Derivation Function (PBKDF2) or Balloon
- function must be iterable at least 10.000 times
- Recommendation: to keep a second part of the hash and another salt in other separated store. If password hashes will be stolen, brute-force will become impractical

| user name | password hash          | password salt         |
| --------- | ---------------------- | --------------------- |
| String    | Base64String(byte[32]) | Base64String(byte[8]) |



.NET classes

PBKDF2: RFC2898

Salt (random): RNGCryptoServiceProvider



### Entity framework



Install entity framework tools

```powershell
dotnet tool install --global dotnet-ef
```

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Design
```



Scaffolding

```powershell
dotnet ef dbcontext scaffold "Host=_;Database=_;Username=_;Password=_;SSL Mode=Require;Trust Server Certificate=true" Npgsql.EntityFrameworkCore.PostgreSQL --context GoTryItContext --output-dir Repositories --context-dir Repositories --force
```



Packages

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="3.1.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Abstractions"/>
<PackageReference Include="Microsoft.EntityFrameworkCore.Design">
   <IncludeAssets>runtime; build; buildtransitive</IncludeAssets>
   <PrivateAssets>all</PrivateAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" />
<PackageReference Include="Npgsql" />
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
```



### Create JWT token



```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
```



1. Create token

```csharp
public string GetToken()
{
  var tokenHandler = new JwtSecurityTokenHandler();
  var key = Convert.FromBase64String(configuration["Jwt:Key"]);
  var tokenDescriptor = new SecurityTokenDescriptor
  {
  	Subject = new ClaimsIdentity(new Claim[] 
                  {
                  new Claim(ClaimTypes.Name, UserName)
                  }),
  	Expires = DateTime.UtcNow.AddDays(7),
  	SigningCredentials = new SigningCredentials(
  														new SymmetricSecurityKey(key), 
  														SecurityAlgorithms.HmacSha256Signature)
  };
  
  var token = tokenHandler.CreateToken(tokenDescriptor);
  
  return tokenHandler.WriteToken(token);
}
```



2. Check token

Startup:ConfigureServices

```csharp
services.AddAuthentication(x =>
{
	x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
  x.RequireHttpsMetadata = false;
  x.SaveToken = true;
  x.TokenValidationParameters = new TokenValidationParameters
  {
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
    ValidateIssuer = false,
    ValidateAudience = false
	};
});
```



Startup:Configure

```csharp
app.UseAuthentication();
app.UseAuthorization();
```


