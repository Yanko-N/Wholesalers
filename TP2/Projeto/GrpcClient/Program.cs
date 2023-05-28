using Grpc.Net.Client;
using GrpcService;
using Microsoft.VisualBasic;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Channels;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7275");
            var user = new AuthUser();
            string authToken = "";
            bool isAdmin = false;

            LoginMenu(channel).ContinueWith((task) => {
                user = task.Result.user;
                isAdmin = task.Result.isAdmin ?? false;
                authToken = task.Result.authToken ?? "";
            }).Wait();

            if (authToken == "")
            {
                Console.WriteLine("Exiting the program...");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }


            if (isAdmin)
            {
                await AdminMenu(channel, user, authToken);
            }
            else
            {
                await OperatorMenu(channel, user, authToken);
            }
            Console.ReadLine();
        }

        static async Task OperatorMenu(GrpcChannel channel, AuthUser user, string authToken)
        {
            bool exit = false;
            var operatorClient = new OperatorActions.OperatorActionsClient(channel);
            while (!exit)
            {
                Console.Clear();

                Console.WriteLine("Operator Options:");
                Console.WriteLine("1. Activate adress");
                Console.WriteLine("2. Desactivate adress");
                Console.WriteLine("3. Reserve adress");
                Console.WriteLine("4. Terminate adress");
                Console.WriteLine("5. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                string adressUid;
                List<OperatorActionUidReply> UIDS = new List<OperatorActionUidReply>();

                switch (choice)
                {
                    case "1":

                        using (var call = operatorClient.ListUid(new OperatorActionUidRequest
                        {
                            Operator = user.Username,
                            Token = authToken
                        }))
                        {
                            while (await call.ResponseStream.MoveNext(CancellationToken.None))
                            {
                                var curr = call.ResponseStream.Current;
                                UIDS.Add(curr);
                                Console.WriteLine($"{curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} the uid code is: {curr.Uid}");

                            }
                        }

                        Console.WriteLine("Que Morada Deseja Ativar :");
                        do
                        {
                            adressUid = Console.ReadLine();
                        } while (!UIDS.Any(m => m.Uid == adressUid));

                        var call2 = operatorClient.Activate(new OperatorActionsRequest
                        {
                            Operator = user.Username,
                            Token = authToken,
                            Uid = adressUid
                        });

                        Console.WriteLine($"{call2.Status} and the estimated time is {call2.Et}");
                        UIDS.Clear();
                        break;
                    case "2":
                        using (var listUID = operatorClient.ListUid(new OperatorActionUidRequest
                        {
                            Operator = user.Username,
                            Token = authToken
                        }))
                        {
                            while (await listUID.ResponseStream.MoveNext(CancellationToken.None))
                            {
                                var curr = listUID.ResponseStream.Current;
                                UIDS.Add(curr);
                                Console.WriteLine($"{curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} the uid code is: {curr.Uid}");

                            }
                        }


                        Console.WriteLine("Que Morada Deseja Ativar :");
                        do
                        {
                            adressUid = Console.ReadLine();
                        } while (!UIDS.Any(m => m.Uid == adressUid));

                        var call3 = operatorClient.Deactivate(new OperatorActionsRequest
                        {
                            Operator = user.Username,
                            Token = authToken,
                            Uid = adressUid
                        });

                        Console.WriteLine($"{call3.Status} and the estimated time is {call3.Et}");

                        UIDS.Clear();

                        break;
                    case "3":
                        Console.WriteLine("RESERVE");
                        break;
                    case "4":
                        Console.WriteLine("TERMINAR");
                        break;
                    case "5":
                        exit = true;
                        Console.WriteLine("Exiting the program...");
                        break;
                }
            }
        }

        static async Task<(AuthUser user, bool? isAdmin, string? authToken)> LoginMenu(GrpcChannel channel)
        {
            var user = new AuthUser();
            var authClient = new Auth.AuthClient(channel);
            string? authToken = null;
            bool? isAdmin = null;
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("Welcome");
                Console.WriteLine("Please select an option:");
                Console.WriteLine("1. Log In");
                Console.WriteLine("2. Register");
                Console.WriteLine("3. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();
                //GRPC de Login

                AuthReply reply = new AuthReply();

                switch (choice)
                {
                    case "1":   //LOGIN
                        Console.Clear();

                        Console.Write("Username: ");
                        user.Username = Console.ReadLine();
                        Console.Write("Password: ");
                        user.Password = Utils.ReadPassword();

                        reply = await authClient.LogInAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        if (reply.Status == "OK")
                        {
                            isAdmin = reply.IsAdmin;
                            authToken = reply.Token;
                            exit = true;
                        }
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Console.Clear();

                        break;

                    case "2":   //REGISTO
                        string confirm;
                        do
                        {
                            Console.Clear();

                            Console.Write("Username: ");
                            user.Username = Console.ReadLine();
                            Console.Write("Password: ");
                            user.Password = Utils.ReadPassword();
                            Console.Write("Confirm Password: ");
                            confirm = Utils.ReadPassword();

                            if (user.Password != confirm) //se as passwords dão match
                                Console.WriteLine("Passwords do not match!");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        } while (user.Password != confirm);

                        reply = await authClient.RegisterAsync(user);
                        Console.WriteLine($"{reply.Status} - {reply.Message}");
                        Console.ReadLine();

                        break;
                    case "3":
                        exit = true;
                        isAdmin = null;
                        authToken = null;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.ReadLine();
                        break;
                }
            }


            return (user, isAdmin, authToken);
        }

        static async Task AdminMenu(GrpcChannel channel, AuthUser user, string authToken)
        {
            bool exit = false;
            var adminClient = new AdminActions.AdminActionsClient(channel);

            while (!exit)
            {
                Console.Clear();

                Console.WriteLine("Admin Options:");
                Console.WriteLine("1. List all Coberturas");
                Console.WriteLine("2. List Coberturas by Operator");
                Console.WriteLine("3. List Services");
                Console.WriteLine("4. Exit");
                Console.Write("Enter your choice: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        Console.WriteLine("Listing all Coberturas\n");
                        using (var call = adminClient.ListAllCoberturas(new AdminActionsListAllCoberturas
                        {
                            Operator = user.Username,
                            Token = authToken
                        }))
                        {
                            while (await call.ResponseStream.MoveNext(CancellationToken.None))
                            {
                                var curr = call.ResponseStream.Current;

                                Console.WriteLine($"{curr.Operator} - {curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} {curr.Estado}");

                            }

                        }
                        Console.WriteLine();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case "2":
                        string op;
                        do
                        {
                            Console.Clear();
                            Console.Write("Operator to search: ");
                            op = Console.ReadLine();

                            if (string.IsNullOrEmpty(op))
                            {
                                Console.WriteLine("Operator name cannot be empty. Please try again.");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            }
                        } while (string.IsNullOrEmpty(op));


                        Console.WriteLine($"Listing all Coberturas by Operator ({op})\n");
                        using (var call = adminClient.ListCoberturasOperator(new AdminActionsCoberturasOperatorRequest
                        {
                            Operator = user.Username,
                            Token = authToken,
                            Operatorsearch = op
                        }))
                        {
                            while (await call.ResponseStream.MoveNext(CancellationToken.None))
                            {
                                var curr = call.ResponseStream.Current;

                                Console.WriteLine($"{curr.Operator} - {curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""} {curr.Estado}");

                            }

                        }

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case "3":
                        Console.Clear();


                        bool active = false;
                        bool deactivated = false;
                        bool reserved = false;
                        bool terminated = false;

                        bool done = false;
                        while (!done)
                        {
                            Console.Clear(); // Clear the console screen

                            Console.WriteLine("Select services status to display:");
                            Console.WriteLine("1. Active (" + (active ? "Selected" : "Not Selected") + ")");
                            Console.WriteLine("2. Deactivated (" + (deactivated ? "Selected" : "Not Selected") + ")");
                            Console.WriteLine("3. Reserved (" + (reserved ? "Selected" : "Not Selected") + ")");
                            Console.WriteLine("4. Terminated (" + (terminated ? "Selected" : "Not Selected") + ")");
                            Console.WriteLine("5. Done");

                            Console.Write("Enter your choice: ");
                            string service = Console.ReadLine();

                            switch (service)
                            {
                                case "1":
                                    active = !active;
                                    Console.WriteLine("Active " + (active ? "selected." : "deselected."));
                                    break;
                                case "2":
                                    deactivated = !deactivated;
                                    Console.WriteLine("Deactivated " + (deactivated ? "selected." : "deselected."));
                                    break;
                                case "3":
                                    reserved = !reserved;
                                    Console.WriteLine("Reserved " + (reserved ? "selected." : "deselected."));
                                    break;
                                case "4":
                                    terminated = !terminated;
                                    Console.WriteLine("Terminated " + (terminated ? "selected." : "deselected."));
                                    break;
                                case "5":
                                    done = true;
                                    Console.WriteLine("Selection completed.\n");
                                    break;
                                default:
                                    Console.WriteLine("Invalid choice. Please try again.");
                                    break;
                            }
                        }

                        List<string> selectedServices = new List<string>();
                        if (active)
                            selectedServices.Add("Active");
                        if (deactivated)
                            selectedServices.Add("Deactivated");
                        if (reserved)
                            selectedServices.Add("Reserved");
                        if (terminated)
                            selectedServices.Add("Terminated");

                        Console.WriteLine("Services to be displayed: " + string.Join(", ", selectedServices) + "\n");

                        using (var call = adminClient.ListServices(new AdminActionsServicesRequest()
                        {
                            Operator = user.Username,
                            Token = authToken,
                            Active = active,
                            Deactivated = deactivated,
                            Reserved = reserved,
                            Terminated = terminated

                        }))
                        {
                            while (await call.ResponseStream.MoveNext(CancellationToken.None))
                            {
                                var curr = call.ResponseStream.Current;

                                Console.WriteLine($"{curr.Operator} - {curr.Timestamp} - {curr.Action} - {curr.Municipio} {curr.Rua} {curr.Numero} {curr.Apartamento ?? ""}");

                            }

                        }

                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case "4":
                        exit = true;
                        Console.WriteLine("Exiting the program...");
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                }

                Console.WriteLine();
            }
        }

    }

}
