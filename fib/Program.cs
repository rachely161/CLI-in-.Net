// See https://aka.ms/new-console-template for more information

using System.Collections.Specialized;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;

#region create options
var outputOption = new Option<FileInfo>("--output", "file path and name");
outputOption.AddAlias("-o");

var languageOption = new Option<string>("--language", "language") { IsRequired = true }.FromAmong("c#", "c++", "c", "sql", "java", "ts", "js", "html", "css", "python", "all");
languageOption.AddAlias("-l");

var noteOption = new Option<bool>("--note", "note");
noteOption.AddAlias("-n");

var sortOption = new Option<string>("--sort", "sort").FromAmong("abc", "type");
sortOption.SetDefaultValue("abc");
sortOption.AddAlias("-s");

var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "remove-empty-lines");
removeEmptyLinesOption.AddAlias("-r");

var authorOption = new Option<string>("--author", "name of the author of the file");
authorOption.AddAlias("-a");

#endregion


//create boundle command
var boundleCommand = new Command("boundle", "boundle code files to a single file");

#region set option tocommand
//set option to command
boundleCommand.AddOption(outputOption);
boundleCommand.AddOption(languageOption);
boundleCommand.AddOption(noteOption);
boundleCommand.AddOption(sortOption);
boundleCommand.AddOption(removeEmptyLinesOption);
boundleCommand.AddOption(authorOption);
#endregion

Dictionary<string, string> typeFiles = new Dictionary<string, string>();
typeFiles.Add("c#", "*.cs");
typeFiles.Add("java", "*.java");
typeFiles.Add("python", "*.python");
typeFiles.Add("sql", "*.sql");
typeFiles.Add("css", "*.css");
typeFiles.Add("html", "*.html");
typeFiles.Add("js", "*.js");
typeFiles.Add("ts", "*.ts");
typeFiles.Add("c", "*.c");
typeFiles.Add("c++", "*.cpp");
typeFiles.Add("all", "*.*");



//set function to bound
boundleCommand.SetHandler((output,language,note,sort,removeEmptyLines,author) =>
{
    try
    {
        //create file in path
        var newFile = File.Create(output.FullName);
        Console.WriteLine("file was created");
        string[] files;
        var directory= Directory.GetCurrentDirectory();

        if (language == "all")
        {
            // קבלת כל הקבצים עם הסיומות המבוקשות
            string[] extensions = { "*.cs", "*.java", "*.sql", "*.cpp","*.python","*.css","*.c","*.js","*.ts","*.html" ,"*.ts"};
            files = extensions.SelectMany(ext => Directory.GetFiles(directory, ext, SearchOption.AllDirectories)).ToArray();
        }
        else
        {
            string typeFile;
            typeFile = typeFiles[language];

            // קבלת קבצים עם הסיומת שנבחרה
            files = Directory.GetFiles(directory, typeFile, SearchOption.AllDirectories);
        }

        // מיון הקבצים לפי הקריטריון שנבחר
        if (sort == "abc")
        {
            files = files.OrderBy(file => Path.GetFileName(file)).ToArray();
        }
        else if (sort == "type")
        {
            files = files.OrderBy(file => Path.GetExtension(file)).ToArray();
        }


        using (StreamWriter writer = new StreamWriter(newFile))
        {
            if(author != null)
                writer.WriteLine("Author:  "+author);
            foreach (string file in files)
            {
                // קריאת תוכן הקובץ וכתיבתו לקובץ החדש

                if (note)
                    writer.WriteLine($"--- content of:  {Path.GetRelativePath(Path.GetDirectoryName(output.FullName), file)}  ---");
                if (removeEmptyLines)
                {
                    foreach (var line in File.ReadAllLines(file).Where(line => !string.IsNullOrWhiteSpace(line)))
                    {
                        writer.WriteLine(line);
                    }
                }
                else
                    writer.WriteLine( File.ReadAllText(file));
                writer.WriteLine();
            }
        }
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: file path is invalid");
    }
    catch(Exception ex) 
    {
        Console.WriteLine(ex.GetType);
    }

}, outputOption,languageOption,noteOption,sortOption,removeEmptyLinesOption,authorOption);


var createRspCommand = new Command("create-rsp", "create response file");

createRspCommand.AddOption(outputOption);

createRspCommand.SetHandler((output) =>
{
    try
    {
        //create file in path
        var newFile = File.Create(output.FullName);
        Console.WriteLine("file was created");

        string outputFile, language = "", sort = "", author = "";
        bool note, removeEmptyLines, ifAuthor;
        Console.WriteLine("press the path of the output file");
        outputFile=Console.ReadLine();
        while (!typeFiles.ContainsKey(language))
        {
            Console.WriteLine("select a language from the follow list: ");
            typeFiles.Keys.ToList().ForEach(l => Console.Write(l + " "));
            Console.WriteLine();
            language = Console.ReadLine();
        }
        while (sort != "abc" && sort != "type")
        {
            Console.WriteLine("select a option to sort: abc/type");
            sort = Console.ReadLine();
        }
        Console.WriteLine("do you want to write the source file at the top of the file content? true/false");
        note = bool.Parse(Console.ReadLine());
        Console.WriteLine("do you want to write the author name at the top of the file? true/false");
        ifAuthor = bool.Parse(Console.ReadLine());
        if (ifAuthor)
        {
            Console.WriteLine("press the name of the author");
            author = Console.ReadLine();
        }
        Console.WriteLine("do you want to remove empty lines?");
        removeEmptyLines = bool.Parse(Console.ReadLine());

        using (var writer = new StreamWriter(newFile))
        {
            writer.Write("boundle -o " + outputFile+" -l "+language+" -s "+sort);
            if (note)
                writer.Write(" -n");
            if (ifAuthor)
                writer.Write(" -a " + author);
            if (removeEmptyLines)
                writer.Write(" -r");
        }
    }
    catch (DirectoryNotFoundException ex)
    {
        Console.WriteLine("Error: file path is invalid");
    }
    catch (FormatException)
    {
        Console.WriteLine("שגיאה: הקלט אינו בפורמט תקין.");
    }
    catch (ArgumentNullException)
    {
        Console.WriteLine("שגיאה: הקלט לא יכול להיות null.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"שגיאה לא צפויה: {ex.Message}");
    }



},outputOption);







var rootCommand = new RootCommand("root command for files boundler CLI");
rootCommand.AddCommand(boundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);


