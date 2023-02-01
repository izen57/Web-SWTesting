/*
 * NotStopAlarm
 *
 * No description provided (generated by Swagger Codegen https://github.com/swagger-api/swagger-codegen)
 *
 * OpenAPI spec version: 0.1.0
 * 
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */
using Logic;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Model;

using Repositories;

using RepositoriesImplementations;

using Serilog;

using System.Text;

using WebAPI;

namespace IO.Swagger
{
	/// <summary>
	/// Startup
	/// </summary>
	public class Startup
	{
		readonly IWebHostEnvironment _hostingEnv;
		IConfiguration Configuration { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="env"></param>
		/// <param name="configuration"></param>
		public Startup(IWebHostEnvironment env, IConfiguration configuration)
		{
			_hostingEnv = env;
			Configuration = configuration;

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.WriteTo.File("logs/log.txt")
				.CreateLogger();
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		/// <param name="services"></param>
		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddMvc()
				.AddXmlSerializerFormatters();

			services
				.AddControllers()
				.AddNewtonsoftJson();

			services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options => 
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = AuthOptions.ISSUER,
						ValidateAudience = true,
						ValidAudience = AuthOptions.AUDIENCE,
						ValidateLifetime = true,
						IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
						ValidateIssuerSigningKey = true,
					}
				);

			services
				.AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
				.AddSingleton<IAlarmClockRepo, AlarmClockFileRepo>()
				.AddSingleton<INoteRepo, NoteFileRepo>()
				.AddSingleton<IUserRepo, UserFileRepo>()
				//.AddSingleton(new Stopwatch(
				//	"����������",
				//	System.Drawing.Color.White,
				//	new System.Diagnostics.Stopwatch(),
				//	new SortedSet<DateTime>(),
				//	false/*,
				//	Guid.Parse(services.*//*httpContextAccessor.HttpContext.Items["User ID"].ToString()*//*)*/
				//))
				.AddSingleton<Dictionary<Guid, Stopwatch>>()
				.AddSingleton<IAlarmClockService, AlarmClockService>()
				.AddSingleton<INoteService, NoteService>()
				.AddSingleton<IStopwatchService, StopwatchService>()
				.AddSingleton<IUserService, UserService>();

			services
				.AddSwaggerGen(setup =>
				{
					setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
					{
						Name = "Authorization",
						Type = SecuritySchemeType.ApiKey,
						Scheme = "Bearer",
						BearerFormat = "JWT",
						In = ParameterLocation.Header,
						Description = "JWT Authorization header using the Bearer scheme."
					});
					setup.AddSecurityRequirement(new OpenApiSecurityRequirement
					{{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}});
					setup.SwaggerDoc("0.1.0", new OpenApiInfo
					{
						Version = "0.1.0",
						Title = "NotStopAlarm",
						Description = "NotStopAlarm (ASP.NET Core 3.1)",
						Contact = new OpenApiContact()
						{
							Name = "Swagger Codegen Contributors",
							Url = new Uri("https://github.com/swagger-api/swagger-codegen"),
							Email = ""
						},
						TermsOfService = new Uri("https://example.com/terms")
					});
					setup.CustomSchemaIds(type => type.FullName);
					setup.IncludeXmlComments($"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{_hostingEnv.ApplicationName}.xml");
					setup.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
				});
		}

		/// <summary>
		/// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		/// <param name="loggerFactory"></param>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
		{
			app
				.UseStaticFiles()
				.UseRouting()
				.UseAuthentication()
				.UseAuthorization()
				.UseSwagger()
				.UseSwaggerUI(c =>
				{
					c.RoutePrefix = "api/v1";
					c.SwaggerEndpoint("/swagger/0.1.0/swagger.json", "NotStopAlarm");
				})
				.UseMiddleware<JWTMiddleware>()
				.UseEndpoints(endpoints => endpoints.MapControllers())
				.UseDeveloperExceptionPage();
		}
	}

	public class AuthOptions
	{
		public const string ISSUER = "MyAuthServer"; // �������� ������
		public const string AUDIENCE = "MyAuthClient"; // ����������� ������
		const string KEY = "mysupersecret_secretkey!123"; // ���� ��� ��������

		public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
			new(Encoding.UTF8.GetBytes(KEY));
	}
}
