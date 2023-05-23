using Grpc.Core;
using GrpcService.Models;

namespace GrpcService.Services {
    public class AuthService : Auth.AuthBase {
        private readonly ILogger<AuthService> _logger;
        public readonly dataContext dataContext = new dataContext();

        public AuthService(ILogger<AuthService> logger) {
            _logger = logger;
        }

        public override Task<AuthReply> LogIn(AuthUser request, ServerCallContext context) {
            var user = dataContext.Users.FirstOrDefault(u => u.Username == request.Username);
            if (user != null)
                if (BCrypt.Net.BCrypt.Verify(request.Password, user.Password)) {
                    string token = Guid.NewGuid().ToString().Replace("-", "");
                    dataContext.UserLoginLogs.Add(new UserLoginLogs {
                        User = request.Username,
                        Date = DateTime.Now,
                        LogMessage = "Successfull Login Attempt",
                        Token = token
                    });
                    dataContext.SaveChanges();
                    return Task.FromResult(new AuthReply {
                        Status = "OK",
                        Message = "Successfully logged in as " + request.Username,
                        IsAdmin = user.isAdmin,
                        Token = token
                    });
                }


            dataContext.UserLoginLogs.Add(new UserLoginLogs {
                User = request.Username,
                Date = DateTime.Now,
                LogMessage = "Failed Login Attempt",
            });
            dataContext.SaveChanges();
            return Task.FromResult(new AuthReply {
                Status = "ERROR",
                Message = "Invalid Credentials!"
            });
        }

        public override Task<AuthReply> Register(AuthUser request, ServerCallContext context) {
            if (dataContext.Users.FirstOrDefault(u => u.Username == request.Username) != null)
                return Task.FromResult(new AuthReply {
                    Status = "ERROR",
                    Message = "Username already in use!"
                });
            var pwd = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new User { Username = request.Username, Password = pwd, isAdmin = false};
            dataContext.Users.Add(user);
            dataContext.SaveChanges();
            return Task.FromResult(new AuthReply {
                Status = "OK",
                Message = "Successfully registered as " + request.Username
            });

        }
    }

}