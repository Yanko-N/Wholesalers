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
            var operatorClient = new OperatorActions.OperatorActionsClient(channel);

            Dictionary<int, OperatorActionUidReply> UIDS = new Dictionary<int, OperatorActionUidReply>();
            OperatorActionUidReply? selectedAddress = null;
            
            bool exit = false;
            while (!exit) {
                int i = 0;
                UIDS.Clear();
                using (var call = operatorClient.ListUid(new OperatorActionUidRequest {
                           Operator = user.Username,
                           Token = authToken
                       })) {
                    while (await call.ResponseStream.MoveNext(CancellationToken.None)) {
                        i++;
                        var curr = call.ResponseStream.Current;
                        UIDS.Add(i, curr);
                    }
                }
                Console.Clear();

                if (selectedAddress == null) {
                    Console.WriteLine("Menu Options:");
                    Console.WriteLine("1. Select / List UIDs");
                    Console.WriteLine("2. Reserve");
                } else {
                    Console.WriteLine($"Selected Adress: {selectedAddress.Municipio} - {selectedAddress.Rua} {selectedAddress.Numero} " + (String.IsNullOrEmpty(selectedAddress.Apartamento) ? "" : $"{selectedAddress.Apartamento}\n"));
                    Console.WriteLine("Menu Options:");
                    Console.WriteLine("1. Select / List UIDs");
                    Console.WriteLine("2. Reserve");
                    Console.WriteLine("3. Activate");
                    Console.WriteLine("4. Deactivate");
                    Console.WriteLine("5. Terminate");
                }

                Console.WriteLine("0. Exit");

                Console.Write("Enter your choice: ");
                string choice = Console.ReadLine();

                Console.WriteLine();

                switch (choice) {
                    case "0":
                        Console.WriteLine("Exiting the program...");
                        Console.WriteLine("Press any key to continue...");
                        return;
                    case "1":
                        bool done = false;
                        Console.Clear();
                        Console.WriteLine("Listing UIDs");

                        if (UIDS.Count == 0) {
                            Console.WriteLine("There are no UIDs in the DB!");
                            Console.ReadKey();
                            Console.WriteLine("Press any key to continue...");
                            break;
                        }

                        while (!done) {

                            Console.WriteLine("Select an option:");
                            Console.WriteLine();

                            foreach (var item in UIDS) {
                                Console.WriteLine($"{item.Key}. Domicilio: {item.Value.Municipio} - {item.Value.Rua} {item.Value.Numero} " + (String.IsNullOrEmpty(item.Value.Apartamento) ? "" : $"{item.Value.Apartamento}"));
                            }

                            Console.WriteLine("0. Exit");
                            Console.WriteLine();

                            Console.Write("Enter your choice: ");
                            string ichoice = Console.ReadLine();

                            Console.WriteLine();

                            if (ichoice == "0") {
                                done = true;
                                break;
                            }

                            if (int.TryParse(ichoice, out int selectedOption)) {
                                if (UIDS.ContainsKey(selectedOption)) {
                                    selectedAddress = UIDS[selectedOption];
                                    done = true;
                                } else {
                                    Console.WriteLine("Invalid choice. Please try again.");
                                }
                            } else {
                                Console.WriteLine("Invalid choice. Please try again.");
                            }

                            Console.WriteLine();
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }



                        break;
                    case "2":
                        string? mun, rua, n, apt, mod;
                        Console.Write("Reserving address\nMunicipio:");
                        mun = Console.ReadLine();
                        Console.Write("Rua: ");
                        rua = Console.ReadLine();
                        Console.Write("Numero:");
                        n = Console.ReadLine();
                        Console.Write("Apartamento: ");
                        apt = Console.ReadLine();
                        Console.Write("Modalidade: ");
                        mod = Console.ReadLine();

                        var morada = new OperatorActionsReserveRequest {
                            Operator = user.Username,
                            Token = authToken,
                            Municipio = mun,
                            Rua = rua,
                            Numero = n,
                            Apartamento = apt,
                            Modalidade = mod
                        };

                        var reply = operatorClient.Reserve(morada);
                        if (reply.Status == "OK") {
                            selectedAddress = new OperatorActionUidReply {
                                Rua = rua,
                                Apartamento = apt,
                                Municipio = mun,
                                Numero = n,
                                Uid = reply.Uid
                            };
                        Console.WriteLine($"{reply.Status} - Successfully reserved!");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();                        
                        } else {
                            Console.WriteLine($"{reply.Status} - Not reserved!");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }
                        break;
                    case "3":
                        if (selectedAddress != null) {
                            Console.WriteLine("Scheduling activation of address...");

                            var call2 = operatorClient.Activate(new OperatorActionsRequest {
                                Operator = user.Username,
                                Token = authToken,
                                Uid = selectedAddress.Uid
                            });


                            if (call2.Status == "OK") {
                                Console.WriteLine($"Activation scheduled successfully. ET: {call2.Et}s");
                                selectedAddress = null;
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            } else {
                                Console.WriteLine($"{call2.Status}");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            }
                        } else {
                            Console.WriteLine("Invalid choice. Please try again.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }

                        break;
                    case "4":
                        if (selectedAddress != null) {
                            Console.WriteLine("Scheduling deactivation of address...");

                            var call2 = operatorClient.Deactivate(new OperatorActionsRequest {
                                Operator = user.Username,
                                Token = authToken,
                                Uid = selectedAddress.Uid
                            });


                            if (call2.Status == "OK") {
                                Console.WriteLine($"Deactivation scheduled successfully. ET: {call2.Et}s");
                                selectedAddress = null;
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            } else {
                                Console.WriteLine($"{call2.Status}");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            }
                        } else {
                            Console.WriteLine("Invalid choice. Please try again.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }

                        break;
                    case "5":
                        if (selectedAddress != null) {
                            Console.WriteLine("Scheduling termination of address...");

                            var call2 = operatorClient.Terminate(new OperatorActionsRequest {
                                Operator = user.Username,
                                Token = authToken,
                                Uid = selectedAddress.Uid
                            });


                            if (call2.Status == "OK") {
                                Console.WriteLine($"Termination scheduled successfully. ET: {call2.Et}s");
                                selectedAddress = null;
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            } else {
                                Console.WriteLine($"{call2.Status}");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();
                            }
                        } else {
                            Console.WriteLine("Invalid choice. Please try again.");
                            Console.WriteLine("Press any key to continue...");
                            Console.ReadKey();
                        }

                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
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
