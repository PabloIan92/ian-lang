using System;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            Console.WriteLine("i@N Language v0.5.0 - Unified Runtime");
            Console.WriteLine();
            Console.WriteLine("Usage: ian <module> [args...]");
            Console.WriteLine();
            Console.WriteLine("Modules:");
            Console.WriteLine("  engine     Run a .ian script file");
            Console.WriteLine("  shell      Start interactive shell");
            Console.WriteLine("  brain      Memory system (learn, query, import)");
            Console.WriteLine("  agent      Compile prompts to .ian programs");
            Console.WriteLine("  api        REST API client");
            Console.WriteLine("  architect  Roadmap and capabilities");
            Console.WriteLine("  human      User profile and contextual responses");
            Console.WriteLine("  teacher    Teaching and learning module");
            Console.WriteLine("  supervisor Code review and quality");
            Console.WriteLine("  daily      Daily learning from web sources");
            Console.WriteLine("  auto       Autonomous execution");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  ian shell");
            Console.WriteLine("  ian engine script.ian");
            Console.WriteLine("  ian brain aprender \"pregunta\" \"codigo\"");
            Console.WriteLine("  ian api preguntar \"whois ejemplo.com\"");
            return 0;
        }

        string module = args[0].ToLowerInvariant();
        string[] moduleArgs = args.Length > 1 ? new string[args.Length - 1] : Array.Empty<string>();
        if (args.Length > 1)
            Array.Copy(args, 1, moduleArgs, 0, args.Length - 1);

        switch (module)
        {
            case "shell":
                return IanShell.Run(moduleArgs);
            case "brain":
                return IanBrain.Run(moduleArgs);
            case "agent":
                return IanAgent.Run(moduleArgs);
            case "api":
                return IanApi.Run(moduleArgs);
            case "architect":
                return IanArchitect.Run(moduleArgs);
            case "human":
                return IanHuman.Run(moduleArgs);
            case "teacher":
                return IanTeacher.Run(moduleArgs);
            case "supervisor":
                return IanSupervisor.Run(moduleArgs);
            case "daily":
                return IanDaily.Run(moduleArgs);
            case "auto":
                return IanAuto.Run(moduleArgs);
            case "engine":
            default:
                return IanEngine.Run(moduleArgs);
        }
    }
}
