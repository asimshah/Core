using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fastnet.Core
{
    internal class Option
    {
        internal string Name { get; set; }
        internal string Letter { get; set; }
        public string HelpText { get; set; }
        public dynamic Method { get; set; }
    }
    internal class Parameter
    {
        public string Name { get; set; }
        public Type Type { get; set; }
        public bool IsList { get; set; }
        public string HelpText { get; set; }
        public dynamic Method { get; set; }
    }
    /// <summary>
    /// internally used for command line processing
    /// </summary>
    public abstract class Verb
    {
        internal string Name { get; set; }
        internal string Letter { get; set; }
        internal string HelpText { get; set; }
        internal abstract void CallVerbMethod();
        internal abstract void SetParameter(dynamic method, string arg);
        internal abstract void SetOption(dynamic method, bool val);
        internal List<Option> Options { get; set; }
        internal List<Parameter> Parameters { get; set; }
        internal Verb()
        {
            Parameters = new List<Parameter>();
            Options = new List<Option>();
        }
        internal bool HasListParameter()
        {
            return Parameters.Count() > 0 && Parameters.Last().IsList;
        }
    }
    /// <summary>
    /// internally used to provide fluent methods for defing command line verbs
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class Verb<V> : Verb
    {
        internal V Instance { get; set; }
        internal Action<V> Method { get; set; }
        /// <summary>
        /// Add a parameter definition for the current verb - can be called multiple times
        /// </summary>
        /// <typeparam name="T">type of parameter - often string</typeparam>
        /// <param name="name">name of parameter - used in help displays</param>
        /// <param name="method">lambda called with verb instance and the parameter value</param>
        /// <returns></returns>
        public Verb<V> AddParameter<T>(string name, Action<V, T> method)
        {
            // the remaining parameter (i.e. after all options have gone) as T to method
            if (!HasListParameter())
            {
                Parameters.Add(new Parameter
                {
                    Name = name.ToLower(),
                    IsList = false,
                    Type = typeof(T),
                    Method = method
                });

            }
            return this;
        }
        /// <summary>
        /// not yet implemented
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <returns></returns>
        public Verb<V> AddParameters<T>(Action<List<T>> method)
        {
            // the remaining parameter list (i.e. after all options have gone) as T to method
            return this;
        }
        /// <summary>
        /// add an option for the current verb (i.e -option on the command line)
        /// </summary>
        /// <typeparam name="T">only bool is implemented</typeparam>
        /// <param name="letter">single letter short form for this option</param>
        /// <param name="verb">full name form of this option</param>
        /// <param name="helpText">help text to use in help displays</param>
        /// <param name="method">lambda called with verb instance and a true value (missing options do not call the lambda)</param>
        /// <returns></returns>
        public Verb<V> AddOption<T>(char letter, string verb, string helpText, Action<V, bool> method)
        {
            // an option starting with - 
            var option = new Option
            {
                Letter = letter.ToString().ToLower(),
                Name = verb.ToLower(),
                HelpText = helpText,
                Method = method
            };
            Options.Add(option);
            return this;
        }
        internal override void CallVerbMethod()
        {
            this.Method(this.Instance);
        }
        internal override void SetParameter(dynamic method, string arg)
        {
            method(this.Instance, arg);
        }
        internal override void SetOption(dynamic method, bool val)
        {
            method(this.Instance, val);
        }
    }
    internal class Help
    {
        public string Command { get; set; }
    }
    /// <summary>
    /// Command line parser for a console app
    /// </summary>
    public class CommandLine
    {
        private List<Verb> verbs = new List<Verb>();
        private List<Option> options = new List<Option>();
        /// <summary>
        /// 
        /// </summary>
        public CommandLine()
        {
            AddVerb<Help>('h', "help", "help for a command", (help) => { ShowCommandHelp(help); })
                .AddParameter<string>("help", (help, c) => { help.Command = c.ToLower(); })
                ;
        }
        /// <summary>
        /// Parses the arg list and executes the handler for the selected verb
        /// Define verbs, aoptions and parameters before calling this method
        /// </summary>
        /// <param name="args"></param>
        public void Parse(string[] args)
        {
            //Console.WriteLine($"args are: {(string.Join(",", args))}");
            if (args.Length > 0)
            {
                var verb = FindVerb(args[0]);
                var remainingArgs = FindOptions(verb, args.Skip(1).ToList());
                if (verb.HasListParameter() || remainingArgs.Count() == verb.Parameters.Count())
                {
                    remainingArgs = FindParameters(verb, remainingArgs);

                    if (verb != null && remainingArgs.Count() == 0)
                    {
                        //Console.WriteLine($"found verb {verb.Name}");
                        verb.CallVerbMethod();
                    }
                    else
                    {
                        Console.WriteLine($"unknown args: {(string.Join(", ", remainingArgs.ToArray()))}");
                    }
                }
                else
                {
                    if (remainingArgs.Count() < verb.Parameters.Count())
                    {
                        Console.WriteLine($"insufficient parameters");
                    }
                    else
                    {
                        Console.WriteLine($"too many parameters");
                    }
                }
            }
            else
            {
                ShowHelp();
            }
        }
        private List<string> FindParameters(Verb verb, List<string> args)
        {
            var targs = args.ToArray();
            for (int i = 0; i < targs.Length; ++i)
            {
                var method = verb.Parameters.Skip(i).First().Method;
                verb.SetParameter(method, targs[i]);

                args.Remove(targs[i]);
            }
            return args;
        }
        private bool ScanAvailableOptions(Verb verb, string arg)
        {
            foreach (var opt in verb.Options)
            {
                var r = MatchOption(arg, opt);
                if (r != null)
                {
                    verb.SetOption(opt.Method, true);
                    this.options.Add(opt);
                    return true;
                }
            }
            return false;
        }
        private List<string> FindOptions(Verb verb, List<string> args)
        {
            foreach (var item in args.ToArray())
            {
                if (item.StartsWith("-"))
                {
                    if (ScanAvailableOptions(verb, item.Substring(1)))
                    {

                    }
                    args.Remove(item);
                }
            }
            return args;
        }

        private Verb FindVerb(string arg0)
        {
            foreach (var v in verbs)
            {
                var match = MatchVerb(arg0, v);
                if (match != null)
                {
                    return v;
                }
            }
            return null;
        }
        private object MatchVerb(string arg0, Verb v)
        {
            if (arg0.Length == 1)
            {
                if (arg0.ToLower() == v.Letter)
                {
                    return v;
                }
            }
            else if (arg0.ToLower() == v.Name)
            {
                return v;
            }
            return null;
        }
        private Option MatchOption(string arg, Option option)
        {
            if (arg.Length == 1)
            {
                if (arg.ToLower() == option.Letter)
                {
                    return option;
                }
            }
            else if (arg.ToLower() == option.Name)
            {
                return option;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="letter"></param>
        /// <param name="verb"></param>
        /// <param name="helpText"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public Verb<T> AddVerb<T>(char letter, string verb, string helpText, Action<T> method) where T : new()
        {
            var v = new Verb<T>
            {
                Letter = letter.ToString().ToLower(),
                Name = verb.ToLower(),
                Instance = new T(),
                Method = method,
                HelpText = helpText
            };
            verbs.Add(v);
            return v;
        }
        internal void ShowHelp()
        {
            Console.WriteLine(GetCurrentExecutableDescription());
            Console.WriteLine("Usage: nglget <command> [args] [options]");
            Console.WriteLine("\nAvailable commands:\n");
            foreach (var verb in verbs)
            {
                var command = $" {verb.Letter}|{verb.Name}".PadRight(16);
                Console.WriteLine($"{command}{verb.HelpText}");
            }
        }

        internal void ShowCommandHelp(Help help)
        {
            Console.WriteLine($"help for {help.Command}");
            var verb = verbs.SingleOrDefault(x => x.Name == help.Command);
            if (verb != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append($"usage: nglget {verb.Letter}|{verb.Name}");
                foreach (var para in verb.Parameters)
                {
                    sb.Append($" <{para.Name}>");
                }
                sb.AppendLine();
                sb.AppendLine();
                sb.AppendLine($"{verb.HelpText}");
                Console.WriteLine(sb.ToString());
                if (verb.Options.Count() > 0)
                {
                    sb.Clear();
                    sb.AppendLine($"options:");
                    sb.AppendLine();
                    foreach (var opt in verb.Options)
                    {
                        sb.Append($" -{opt.Letter}|{opt.Name}".PadRight(16));
                        sb.AppendLine($"{opt.HelpText}");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }
        private string GetCurrentExecutableDescription()
        {
            var executingAssembly = Assembly.GetEntryAssembly().GetName();
            var version = executingAssembly.Version.ToString();
            var location = new Uri(executingAssembly.CodeBase);
            return $"{new FileInfo(location.AbsolutePath).FullName}, version {version}";
        }
    }

}
