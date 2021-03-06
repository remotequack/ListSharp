﻿using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ListSharp
{
    class Program
    {

        static void Main(string[] args)
        {
            Regex _regex;
            Match match;
            string currentdir = Environment.CurrentDirectory;
            string allcode = System.IO.File.ReadAllText(currentdir + "\\ListSharp.ls");
            allcode = allcode.Replace("<here>", currentdir);
            Console.WriteLine("Original Code \n-----------------------");
            Console.WriteLine(allcode);
            Console.WriteLine("-----------------------");
            Console.WriteLine(Environment.NewLine);
            List<string> maincode = allcode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();
            maincode.RemoveAll(string.IsNullOrWhiteSpace);
            List<string> alllists = new List<string>();
            string code = "public class MainClass" + Environment.NewLine + "{ " + Environment.NewLine + "public string Execute()" + Environment.NewLine + "{" + Environment.NewLine + "string temp_contents = \"\";" + Environment.NewLine + "string output = \"\";" + Environment.NewLine;
            //creation of variables starts here
            foreach (string singleline in maincode) {




                if (singleline.Substring(0,4) == "LIST")
                {
                    _regex = new Regex(@"LIST([^>]*)=");
                    match = _regex.Match(singleline);
                    alllists.Add("System.Collections.Generic.List<string> " + match.Groups[1].Value.Trim() + ";" + Environment.NewLine);
                }

 
                if (singleline.Substring(0, 4) == "ROWS")
                {
                    _regex = new Regex(@"ROWS([^>]*)=");
                    match = _regex.Match(singleline);
                    alllists.Add("string[] " + match.Groups[1].Value.Trim() + ";" + Environment.NewLine);
                }


                if (singleline.Substring(0, 4) == "STRG")
                {
                    _regex = new Regex(@"STRG([^>]*)=");
                    match = _regex.Match(singleline);
                    alllists.Add("string " + match.Groups[1].Value.Trim() + ";" + Environment.NewLine);
                }

            }

            alllists = alllists.Distinct().ToList<string>();
            foreach (string temp_string in alllists)
            {
                code += temp_string;
            }

            int line = 0;
            foreach (string singleline in maincode)
            {
                
                line++;
                string[] splitline = singleline.Split(new char[] { '=' }, 2);
                splitline[1] = splitline[1].Substring(1);

                if (splitline[0].Substring(0, 2) == "//")
                {
                    code += "//skipped";
                    code += Environment.NewLine;
                    continue;
                }


                if (splitline[0].Length < 4 || splitline[1].Length < 3)
                {
                    Console.WriteLine("ListSharp exception: Line: " + line + " invalid");
                    while(true)
                    {
                        Thread.Sleep(1000);
                    }
                    
                }


                string varname = splitline[0].Substring(4).Trim();


                if (splitline[0].Substring(0, 4) == "STRG")
                {



                    if (splitline[1].Substring(0, 4) == "READ")
                    {
                        _regex = new Regex(@"READ\[([^>]*)\]");
                        match = _regex.Match(splitline[1]);
                        string path = @match.Groups[1].Value.Trim();

                        if (!File.Exists(path))
                        {
                            Console.WriteLine("ListSharp exception: File does not exist, aborting operation\nadditional information: File:\"" + path + "\"");

                            while (true)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                        code += "temp_contents = System.IO.File.ReadAllText(@\"" + path + "\");";
                        code += Environment.NewLine;

                    }

                    code += varname + " = temp_contents;";

                }

                if (splitline[0].Substring(0, 4) == "ROWS")
                {
                    if (splitline[1].Substring(0, 8) == "ROWSPLIT")
                    {
                        _regex = new Regex(@"ROWSPLIT([^>]*)BY");
                        match = _regex.Match(splitline[1]);
                        string invar = match.Groups[1].Value.Trim();

                        _regex = new Regex(@"\[(.*)\]");
                        match = _regex.Match(splitline[1]);
                        string bywhat = match.Groups[1].Value.Trim();

                        bywhat = bywhat.Replace("<newline>", "System.Environment.NewLine");

                        code += varname + " = " + invar + ".Split(new string[] { " + bywhat + " }, System.StringSplitOptions.None);"; 
                    }
                }

                if (splitline[0].Substring(0, 4) == "LIST")
                {
                    _regex = new Regex(@"LIST([^>]*)");
                    match = _regex.Match(splitline[0]);
                    //code += match.Groups[1].Value.Trim() + " = ";   
                }

                if (splitline[0].Substring(0, 4) == "SHOW")
                {
                        _regex = new Regex(@"([^>]*);");
                        match = _regex.Match(splitline[1]);
                        code += "output = makeOutput(" + match.Groups[1].Value.Trim() + " , output);";
                        code += Environment.NewLine;
                }
                code += Environment.NewLine;
            }

            code += "return output;";

            code += Environment.NewLine + "}" + Environment.NewLine;

            
            code += "public string getString(object thevar)";
            code += Environment.NewLine;
            code += "{"; 
            code +=  Environment.NewLine;
            code +=  "if (thevar.GetType() == typeof(string))"; 
            code +=  Environment.NewLine;
            code += "return \"[0]\" + (string)thevar + \"[/0]\";"; 
            code +=  Environment.NewLine; 
            code +=  "if (thevar.GetType() == typeof(string[]))"; 
            code +=  Environment.NewLine;
            code += "return arr2str((string[])thevar);"; 
            code +=  Environment.NewLine; 
            code +=  "if (thevar.GetType() == typeof(System.Collections.Generic.List<string>))"; 
            code +=  Environment.NewLine;
            code += "return arr2str(((System.Collections.Generic.List<string>)thevar).ToArray());";
            code +=  Environment.NewLine;
            code +=  "return \"Show Error\";";
            code +=  Environment.NewLine ;
            code +=  "}";

            code += Environment.NewLine;
            code += Environment.NewLine;
            code += "public string makeOutput(object thein,string output)";
            code += Environment.NewLine;
            code += "{";
            code += Environment.NewLine;
            code += "output += System.Environment.NewLine + \"---------------output--------------\" + System.Environment.NewLine + getString(thein).Replace(System.Environment.NewLine,\"<newline>\" + System.Environment.NewLine) + System.Environment.NewLine + \"-----------------------------------\" + System.Environment.NewLine;";
            code += Environment.NewLine;
            code += "return output;";
            code += Environment.NewLine;
            code += "}";

            code += Environment.NewLine;
            code += Environment.NewLine;
            code += "public string arr2str(string[] arr)";
            code += Environment.NewLine;
            code += "{";
            code += Environment.NewLine;
            code += "string r = \"\";";
            code += Environment.NewLine;
            code += "for (int i = 0; i < arr.Length; i++)";
            code += Environment.NewLine;
            code += "r += \"[\" + i + \"]\" + arr[i] + \"[/\" + i + \"]\";";
            code += Environment.NewLine;
            code += "return r;";
            code += Environment.NewLine;
            code += "}";
/*
public void makeOutput(object thein)
{
string inp = getString(thein);
output += System.Environment.NewLine + "---------------output--------------" + System.Environment.NewLine + getString(inp) + System.Environment.NewLine + "-----------------------------------" + System.Environment.NewLine;
}
*/




            code += Environment.NewLine;
            code += Environment.NewLine;
            code += Environment.NewLine + "}";
            Console.WriteLine(Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine("Interperted Code \n-----------------------");

            int index = 1;
            char[] delimiterChars = { '\n' };
            string[] allLines = code.Split(delimiterChars);
            

            foreach (string currentLine in allLines)
            {
                System.Console.WriteLine("Line: " + index + "    " +currentLine);
                index++;
            }



            Console.WriteLine("-----------------------");
            Console.WriteLine(Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine);
            Console.WriteLine("Initializing ListSharp");

            using (Microsoft.CSharp.CSharpCodeProvider CodeProv =
            new Microsoft.CSharp.CSharpCodeProvider())
            {
                CompilerResults results = CodeProv.CompileAssemblyFromSource(
                     new System.CodeDom.Compiler.CompilerParameters()
                     {
                         GenerateInMemory = true
                     },
                     code);
                foreach (CompilerError er in results.Errors)
                {
                    Console.WriteLine(er.ToString());
                }
                if (results.Errors.HasErrors)
                {
                    Console.WriteLine(Environment.NewLine + "Got compiling errors,aborting ListSharp");
                    while (true)
                    {
                        Thread.Sleep(1000);
                    }
                }


                 
                 var type = results.CompiledAssembly.GetType("MainClass");

                 var obj = Activator.CreateInstance(type);

                 var output = type.GetMethod("Execute").Invoke(obj, new object[] { });
                 
                 Console.WriteLine(output);
            }
            Thread.Sleep(1000000);
        }
    }
}


